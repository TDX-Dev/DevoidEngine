using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Rendering.RGraph
{
    public class RenderGraph
    {
        private struct ResourceAlloc
        {
            public RID resourceHandle;
            public int firstUse;
            public int lastUse;
        }

        private struct ActiveEntry
        {
            public int endTime;
            public TextureDescription desc;
            public Texture resource;
            public string resourcename;
        }

        private struct FreeTextureEntry
        {
            public Texture Resource;
            public string PreviousResource;
            public int PreviousLastUse;
        }

        private struct TextureReuseRecord
        {
            public string PreviousResource;
            public string ReusedForResource;
            public Texture Resource;
            public int PreviousLastUse;
            public int NewFirstUse;
        }


        private readonly RIDOwner<RGTextureResource> _resources;
        private readonly Dictionary<RID, List<Tuple<int, RGResourceAccessType>>> _resourceDependencyMap;
        private readonly List<List<Tuple<RID, RGResourceAccessType>>> _passDependencyList;
        private readonly Dictionary<RID, int> _resourceFirstUse;
        private readonly Dictionary<RID, int> _resourceLastUse;
        private readonly List<TextureReuseRecord> _textureReuseRecords;

        private readonly List<RenderPass> _passes;
        private readonly List<int> _passOrder;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsWriteAccess(RGResourceAccessType accessType) => accessType == RGResourceAccessType.Write || accessType == RGResourceAccessType.ReadWrite;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IsReadAccess(RGResourceAccessType accessType) => accessType == RGResourceAccessType.Read || accessType == RGResourceAccessType.ReadWrite;

        public RenderGraph()
        {
            _resources = new RIDOwner<RGTextureResource>();
            _resourceDependencyMap = [];
            _passDependencyList = [];
            _resourceFirstUse = [];
            _resourceLastUse = [];
            _textureReuseRecords = [];


            _passes = [];
            _passOrder = [];
        }

        void ResetGraph()
        {
            _resources.FreeWhere(res => !res.Imported);

            _resourceDependencyMap.Clear();
            _passDependencyList.Clear();
            _resourceFirstUse.Clear();
            _resourceLastUse.Clear();
            _passOrder.Clear();

            _textureReuseRecords.Clear();
        }

        public void AddPass(RenderPass pass)
        {
            _passes.Add(pass);
        }
        public void Compile()
        {
            ResetGraph();


            SetupPassesAndRecordDependencies();

            int resCount = _resources.GetCount();

            if (_resourceDependencyMap.Count != resCount)
                throw new Exception("Number of resources created does not match with number of dependencies for that resource. Check if created resources are being used otherwise, dont create.");

            ComputeResourceLifetimes(_resourceDependencyMap);


            List<HashSet<int>> adjacencyList = [];
            List<int> dagIndegrees = [];
            int passCount = _passes.Count;

            for (int i = 0; i < passCount; i++)
            {
                adjacencyList.Add([]);
                dagIndegrees.Add(0);
            }

            BuildAdjacencyGraph(_resourceDependencyMap, adjacencyList, dagIndegrees);

            List<bool> neededPasses = CullPasses(_resourceDependencyMap);

            BuildTopologicalOrder(neededPasses, adjacencyList, dagIndegrees);

            {
                Dictionary<int, int> passToExecutionOrder = [];
                passToExecutionOrder.EnsureCapacity(_passOrder.Count);

                for (int j = 0; j < _passOrder.Count; j++)
                {
                    passToExecutionOrder.Add(_passOrder[j], j);
                }

                int totalResourceCount = _resources.GetCount();
                List<int> execFirstUse = new(totalResourceCount);
                List<int> execLastUse = new(totalResourceCount);
                List<bool> resourceIsNeeded = new(totalResourceCount);

                for (int j = 0; j < totalResourceCount; j++)
                {
                    execFirstUse.Add(int.MaxValue);
                    execLastUse.Add(0);
                    resourceIsNeeded.Add(false);
                }

                ReadOnlySpan<Slot<RGTextureResource>> textureResources = _resources.AsSpan();
                int i = -1;
                foreach (var kvp in _resourceDependencyMap)
                {
                    i++;
                    RGTextureResource resource = _resources.Get(kvp.Key);
                    if (resource.Imported || i >= _resourceDependencyMap.Count) continue;

                    for (int j = 0; j < kvp.Value.Count; j++)
                    {
                        int passIdx = kvp.Value[j].Item1;
                        RGResourceAccessType accessType = kvp.Value[j].Item2;

                        if (!neededPasses[passIdx]) continue;

                        int executionIdx = passToExecutionOrder[passIdx];

                        execFirstUse[i] = int.Min(execFirstUse[i], executionIdx);
                        execLastUse[i] = int.Max(execLastUse[i], executionIdx);
                        resourceIsNeeded[i] = true;

                    }
                }


                List<ResourceAlloc> toAllocate = [];
                i = -1;
                foreach (var kvp in _resourceDependencyMap)
                {
                    i++;
                    RGTextureResource resource = _resources.Get(kvp.Key);
                    if (resource.Imported || i >= _resourceDependencyMap.Count || !resourceIsNeeded[i]) continue;

                    toAllocate.Add(new ResourceAlloc()
                    {
                        resourceHandle = kvp.Key,
                        firstUse = execFirstUse[i],
                        lastUse = execLastUse[i],
                    });
                }

                toAllocate.Sort((a, b) => a.firstUse.CompareTo(b.firstUse));

                PriorityQueue<ActiveEntry, int> activeTextures = new();

                Dictionary<TextureDescription, List<FreeTextureEntry>> frameFreePool = [];

                for (int j = 0; j < toAllocate.Count; j++)
                {
                    ref var resource = ref _resources.GetRef(toAllocate[j].resourceHandle);

                    while (activeTextures.Count != 0 && activeTextures.Peek().endTime < toAllocate[j].firstUse)
                    {
                        var expired = activeTextures.Dequeue();
                        if (!frameFreePool.TryGetValue(expired.desc, out var textureFreeList))
                        {
                            textureFreeList = [];
                            frameFreePool.Add(expired.desc, textureFreeList);
                        }

                        textureFreeList.Add(new FreeTextureEntry
                        {
                            Resource = expired.resource,
                            PreviousResource = expired.resourcename,
                            PreviousLastUse = expired.endTime
                        });
                    }


                    bool reused = false;

                    if (frameFreePool.TryGetValue(resource.Description, out var freeList) && freeList.Count > 0)
                    {
                        FreeTextureEntry freeEntry = freeList[^1];
                        freeList.RemoveAt(freeList.Count - 1);

                        resource.Resource = freeEntry.Resource;
                        resource.Allocated = true;
                        reused = true;

                        _textureReuseRecords.Add(new TextureReuseRecord
                        {
                            PreviousResource = freeEntry.PreviousResource,
                            ReusedForResource = resource.Name,
                            Resource = freeEntry.Resource,
                            PreviousLastUse = freeEntry.PreviousLastUse,
                            NewFirstUse = toAllocate[j].firstUse
                        });
                    }

                    if (!reused)
                    {
                        resource.Resource = new Texture(resource.Description);
                        resource.Allocated = true;
                    }
                    activeTextures.Enqueue(new()
                    {
                        desc = resource.Description,
                        endTime = toAllocate[j].lastUse,
                        resource = resource.Resource!,
                        resourcename = resource.Name
                    }, toAllocate[j].lastUse);
                }
            }


            PrintDependencies();
        }

        void BuildTopologicalOrder(List<bool> neededPasses, List<HashSet<int>> adj, List<int> indegrees)
        {
            int passCount = neededPasses.Count;
            Queue<int> q = [];
            List<int> filteredIndegrees = new(passCount);
            int neededPassCount = 0;

            for (int i = 0; i < passCount; i++)
                filteredIndegrees.Add(0);

            for (int i = 0; i < passCount; i++)
            {
                if (!neededPasses[i])
                    continue;

                neededPassCount++;

                foreach (int v in adj[i])
                {
                    if (neededPasses[v])
                        filteredIndegrees[v]++;
                }
            }

            for (int i = 0; i < passCount; i++)
            {
                if (neededPasses[i] && filteredIndegrees[i] == 0)
                    q.Enqueue(i);
            }

            _passOrder.Clear();

            while (q.Count != 0)
            {
                int u = q.Dequeue();
                _passOrder.Add(u);

                foreach (int v in adj[u])
                {
                    if (!neededPasses[v])
                        continue;

                    filteredIndegrees[v]--;

                    if (filteredIndegrees[v] == 0)
                        q.Enqueue(v);
                }
            }

            if (_passOrder.Count != neededPassCount)
            {
                Console.WriteLine("Remaining passes involved in a cycle:");

                for (int i = 0; i < passCount; i++)
                {
                    if (neededPasses[i] && filteredIndegrees[i] > 0)
                    {
                        Console.WriteLine($"Pass {i}: {_passes[i].Name}");

                        foreach (int dependency in adj[i])
                        {
                            if (neededPasses[dependency] && filteredIndegrees[dependency] > 0)
                                Console.WriteLine($"  -> Pass {dependency}: {_passes[dependency].Name}");
                        }
                    }
                }

                throw new Exception("Cycle detected in passes");
            }
        }

        List<bool> CullPasses(Dictionary<RID, List<Tuple<int, RGResourceAccessType>>> dependencyMap)
        {
            int passCount = _passes.Count;
            List<bool> neededPasses = new(passCount);
            List<HashSet<int>> dependencies = new(passCount);
            Queue<int> q = [];
            int resCount = _resources.GetCount();

            for (int i = 0; i < passCount; i++)
            {
                neededPasses.Add(false);
                dependencies.Add([]);
            }

            foreach (var kvp in dependencyMap)
            {
                List<Tuple<int, RGResourceAccessType>> touches = kvp.Value;

                if (touches.Count < 2)
                    continue;

                int lastWriter = -1;

                touches.Sort((a, b) => a.Item1.CompareTo(b.Item1));

                for (int i = 0; i < touches.Count; i++)
                {
                    Tuple<int, RGResourceAccessType> passIdxAccess = touches[i];

                    bool isWrite = IsWriteAccess(passIdxAccess.Item2);
                    bool isRead = IsReadAccess(passIdxAccess.Item2);

                    if (isRead && lastWriter != -1 && lastWriter != passIdxAccess.Item1)
                    {
                        dependencies[passIdxAccess.Item1].Add(lastWriter);
                    }

                    if (isWrite)
                        lastWriter = passIdxAccess.Item1;
                }
            }

            for (int i = 0; i < passCount; i++)
            {
                bool isRoot = false;

                for (int j = 0; j < _passDependencyList[i].Count; j++)
                {
                    Tuple<RID, RGResourceAccessType> dependency = _passDependencyList[i][j];
                    RGTextureResource resource = _resources.Get(dependency.Item1);

                    if (resource.Imported)
                    {
                        if (IsWriteAccess(dependency.Item2))
                        {
                            isRoot = true;
                            break;
                        }
                    }
                }

                if (isRoot)
                {
                    if (!neededPasses[i])
                    {
                        neededPasses[i] = true;
                        q.Enqueue(i);
                    }
                }
            }

            while (q.Count != 0)
            {
                int u = q.Peek();
                q.Dequeue();

                foreach (int v in dependencies[u])
                {
                    if (!neededPasses[v])
                    {
                        neededPasses[v] = true;
                        q.Enqueue(v);
                    }
                }
            }

            return neededPasses;
        }

        void BuildAdjacencyGraph(Dictionary<RID, List<Tuple<int, RGResourceAccessType>>> dependencyMap, List<HashSet<int>> adjacencyList, List<int> dagIndegreeList)
        {
            int resCount = dependencyMap.Count;
            foreach (var kvp in dependencyMap)
            {
                List<Tuple<int, RGResourceAccessType>> touches = kvp.Value;
                if (touches.Count < 2)
                    continue;

                touches.Sort((a, b) => a.Item1.CompareTo(b.Item1));

                RGTextureResource resource = _resources.Get(kvp.Key);

                bool isTransient = !resource.Imported;

                for (int i = 0; i < touches.Count; i++)
                {
                    for (int j = i + 1; j < touches.Count; j++)
                    {
                        int passAIndex = touches[i].Item1;
                        RGResourceAccessType passA_AccessType = touches[i].Item2;

                        int passBIndex = touches[j].Item1;
                        RGResourceAccessType passB_AccessType = touches[j].Item2;

                        bool aWrites = IsWriteAccess(passA_AccessType);
                        bool bWrites = IsWriteAccess(passB_AccessType);

                        // Since neither passes write, we can skip this because no hazard for R-R :D
                        if (!aWrites && !bWrites) continue;

                        int src = passAIndex;
                        int dst = passBIndex;

                        if (isTransient)
                        {
                            if (!aWrites && bWrites)
                            {
                                src = passBIndex;
                                dst = passAIndex;
                            }
                        }

                        if (adjacencyList[src].Add(dst))
                        {
                            dagIndegreeList[dst]++;
                        }

                    }
                }
            }
        }

        void ComputeResourceLifetimes(Dictionary<RID, List<Tuple<int, RGResourceAccessType>>> dependencyMap)
        {

            foreach (var id in dependencyMap.Keys)
            {
                _resourceFirstUse.Add(id, int.MaxValue);
                _resourceLastUse.Add(id, 0);
            }

            foreach (var kvp in dependencyMap)
            {
                if (kvp.Value.Count == 0)
                    continue;
                int first = int.MaxValue;
                int last = 0;
                foreach (var value in kvp.Value)
                {
                    first = int.Min(first, value.Item1);
                    last = int.Max(last, value.Item1);
                }
                _resourceFirstUse[kvp.Key] = first;
                _resourceLastUse[kvp.Key] = last;
            }
        }

        void SetupPassesAndRecordDependencies()
        {
            RenderGraphBuilder builder = new(this);
            for (int i = 0; i < _passes.Count; i++)
            {
                builder.RenderPassIndexCurrent = i;
                _passes[i].Setup(builder);
            }
        }

        public RID CreateTexture(string name, TextureDescription textureDescription)
        {
            RGTextureResource virtualResource = new()
            {
                Name = name,
                Description = textureDescription,
            };
            return _resources.MakeRID(virtualResource);
        }

        public RID? FindTexture(string name)
        {
            ReadOnlySpan<Slot<RGTextureResource>> textureResources = _resources.AsSpan();
            int i = 0;

            foreach (var resource in textureResources)
            {
                if (resource.Value.Name == name)
                    return new RID(i, resource.Generation);
                i++;
            }
            return null;
        }

        public RID RegisterPersistentResource(string name, Texture texture)
        {
            RGTextureResource resource = new()
            {
                Name = name,
                Imported = true,
                Resource = texture,
                Description = texture.GPU.Description
            };

            return _resources.MakeRID(resource);
        }

        public void AddTextureDependency(RID handle, int passIndex, RGResourceAccessType accessType)
        {
            List<Tuple<int, RGResourceAccessType>> dependencyList;

            if (_resourceDependencyMap.TryGetValue(handle, out var value))
            {
                dependencyList = value;
            }
            else
            {
                dependencyList = [];
                _resourceDependencyMap.Add(handle, dependencyList);
            }
            dependencyList.Add(new Tuple<int, RGResourceAccessType>(passIndex, accessType));

            if (_passDependencyList.Count < passIndex + 1)
            {
                _passDependencyList.Add([new(handle, accessType)]);
            }
            else
            {
                _passDependencyList[passIndex].Add(new(handle, accessType));
            }
        }

        public void PrintDependencies()
        {
            Console.WriteLine("Dependencies");

            foreach (var key in _resourceDependencyMap.Keys)
            {
                Console.WriteLine("Resource: " + _resources.Get(key).Name);
                foreach (var value in _resourceDependencyMap[key])
                {
                    Console.WriteLine($"\tPass: {_passes[value.Item1].Name} -> Operation: {value.Item2}");
                }
            }


            Console.WriteLine("\nPass Execution Order");

            for (int i = 0; i < _passOrder.Count; i++)
            {
                Console.WriteLine((i + 1) + ". " + _passes[_passOrder[i]].Name);
            }

            Console.WriteLine("\nReused Textures");

            if (_textureReuseRecords.Count == 0)
            {
                Console.WriteLine("No textures were reused.");
            }

            foreach (var reuse in _textureReuseRecords)
            {
                Console.WriteLine(
                    $"{reuse.PreviousResource} -> {reuse.ReusedForResource} " +
                    $"Previous lifetime ended: {reuse.PreviousLastUse} " +
                    $"New lifetime starts: {reuse.NewFirstUse} "
                );
            }
        }
    }
}
