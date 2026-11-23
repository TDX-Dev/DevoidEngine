using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;
using Buffer = SharpDX.Direct3D11.Buffer;


namespace DevoidGPU.DX11
{
    internal sealed class Dx11CommandList : ICommandList
    {
        private const int MaxSrVs = 24;


        public CommandListType Type { get; }


        private readonly DeviceContext _deviceContext;

        // binding cache;
        private Dx11Framebuffer? _currentFramebuffer;
        private (int, int, int, int) _currentViewport;

        private readonly DX11Texture?[] _boundPsSrVs = new DX11Texture?[MaxSrVs];
        private readonly DX11Texture?[] _boundVsSrVs = new DX11Texture?[MaxSrVs];
        private readonly DX11Texture?[] _boundCsSrVs = new DX11Texture?[MaxSrVs];


        private readonly DX11Texture?[] _boundRtVs = new DX11Texture?[8];
        private DX11Texture? _boundDsv;

        private readonly Dx11ShaderStorageBuffer?[] _boundPsSsbOs = new Dx11ShaderStorageBuffer?[MaxSrVs];
        private readonly Dx11ShaderStorageBuffer?[] _boundVsSsbOs = new Dx11ShaderStorageBuffer?[MaxSrVs];
        private readonly Dx11ShaderStorageBuffer?[] _boundCsSsbOs = new Dx11ShaderStorageBuffer?[MaxSrVs];

        private readonly DX11Texture?[] _boundCsUavTextures = new DX11Texture?[8];
        private readonly Dx11ShaderStorageBuffer?[] _boundCsUavBuffers = new Dx11ShaderStorageBuffer?[8];

        private readonly SamplerState?[] _boundVsSamplers = new SamplerState?[MaxSrVs];
        private readonly SamplerState?[] _boundPsSamplers = new SamplerState?[MaxSrVs];
        private readonly SamplerState?[] _boundCsSamplers = new SamplerState?[MaxSrVs];
        private readonly SamplerState?[] _boundGsSamplers = new SamplerState?[MaxSrVs];

        public Dx11CommandList(DeviceContext context) { _deviceContext = context; }

        public void Begin()
        {
            _currentFramebuffer = null;
            _currentViewport = default;

            _deviceContext.InputAssembler.InputLayout = null;
        }

        public void End()
        {

            /* No Op */
        }

        public void Reset()
        {
            _currentFramebuffer = null;
            _currentViewport = default;
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            if ((x, y, width, height) == _currentViewport)
                return;
            _deviceContext.Rasterizer.SetViewport(x, y, width, height);
            _currentViewport = (x, y, width, height);
        }

        public void SetScissor(int left, int top, int right, int bottom)
        {
            _deviceContext.Rasterizer.SetScissorRectangle(
                left,
                top,
                right,
                bottom);
        }

        public void SetFramebuffer(IFrameBuffer? framebuffer)
        {
            if (framebuffer == null)
            {
                _deviceContext.OutputMerger.SetRenderTargets(
                    null,
                    (RenderTargetView[])null!);

                _currentFramebuffer = null;

                Array.Clear(_boundRtVs);
                _boundDsv = null;

                return;
            }

            Dx11Framebuffer dx11Fb = (Dx11Framebuffer)framebuffer;

            if (ReferenceEquals(_currentFramebuffer, dx11Fb) && !dx11Fb.Dirty)
                return;

            _currentFramebuffer = dx11Fb;
            dx11Fb.Dirty = false;

            dx11Fb.ValidateFrameBuffer();

            Array.Clear(_boundRtVs);
            _boundDsv = null;

            for (int i = 0; i < dx11Fb.ColorAttachments.Count; i++)
            {
                if (dx11Fb.ColorAttachments[i] is DX11Texture tex)
                {
                    ResolveForRtv(tex);
                    _boundRtVs[i] = tex;
                }
                else
                {
                    _boundRtVs[i] = null;
                }
            }

            if (dx11Fb.DepthAttachment is DX11Texture depth)
            {
                ResolveForRtv(depth);
                _boundDsv = depth;
            }
            else
            {
                _boundDsv = null;
            }

            _deviceContext.OutputMerger.SetRenderTargets(
                dx11Fb.Dsv,
                dx11Fb.RtVs);
        }
        public void ClearColor(int attachmentIndex, Vector4 color)
        {
            if (_currentFramebuffer == null)
                throw new InvalidOperationException("Framebuffer not bound");

            if (attachmentIndex < 0 || attachmentIndex >= _currentFramebuffer.RtVs.Length)
                throw new ArgumentOutOfRangeException(nameof(attachmentIndex));

            _deviceContext.ClearRenderTargetView(_currentFramebuffer.RtVs[attachmentIndex], new SharpDX.Mathematics.Interop.RawColor4(color.X, color.Y, color.Z, color.W));
        }

        public void ClearDepthStencil(float depth, byte stencil)
        {
            if (_currentFramebuffer?.Dsv == null)
                throw new InvalidOperationException("No depth stencil bound.");

            _deviceContext.ClearDepthStencilView(
                _currentFramebuffer.Dsv,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                depth,
                stencil
            );
        }

        public void GenerateMipmaps(ITexture texture)
        {
            var dx11Texture = (DX11Texture)texture;

            _deviceContext.GenerateMips(dx11Texture.Srv);
        }

        public void SetPipeline(IPipeline pipeline)
        {
            var p = (Dx11GraphicsPipeline)pipeline;

            _deviceContext.InputAssembler.PrimitiveTopology = p.Topology;
            _deviceContext.InputAssembler.InputLayout = p.InputLayout;

            _deviceContext.VertexShader.Set(p.Vs);
            _deviceContext.PixelShader.Set(p.Ps);
            _deviceContext.ComputeShader.Set(null);

            _deviceContext.Rasterizer.State = p.RasterizerState;
            _deviceContext.OutputMerger.SetDepthStencilState(p.DepthStencilState);
            _deviceContext.OutputMerger.SetBlendState(p.BlendState);
        }
        public void SetComputePipeline(IComputePipeline pipeline)
        {
            var dx = (Dx11ComputePipeline)pipeline;

            _deviceContext.ComputeShader.Set(dx.Shader.Cs);
            _deviceContext.VertexShader.Set(null);
            _deviceContext.PixelShader.Set(null);
            _deviceContext.GeometryShader.Set(null);
        }
        public void SetVertexBuffer(IVertexBuffer buffer)
        {
            var vb = (Dx11VertexBuffer)buffer;

            _deviceContext.InputAssembler.SetVertexBuffers(
                vb.Slot,
                new VertexBufferBinding(vb.Buffer, vb.Stride, 0)
            );
        }

        public void SetIndexBuffer(IIndexBuffer buffer)
        {
            var ib = (Dx11IndexBuffer)buffer;

            _deviceContext.InputAssembler.SetIndexBuffer(
                ib.Buffer,
                Dx11StateMapper.ToDxIndexFormat(buffer.Format),
                0
            );
        }

        public void SetDescriptorSet(uint setIndex, IDescriptorSet set)
        {
            var dxSet = (Dx11DescriptorSet)set;
            var layout = (Dx11DescriptorLayout)dxSet.Layout;

            foreach (var binding in layout.Bindings)
            {
                switch (binding.Type)
                {
                    case DescriptorType.UniformBuffer:
                        {
                            if (!dxSet.UniformBuffers.TryGetValue(binding.Binding, out var ub))
                                continue;

                            var dxUb = (Dx11UniformBuffer)ub;

                            BindConstantBuffer(binding.Binding, binding.Stages, dxUb.Buffer);
                            break;
                        }

                    case DescriptorType.Texture:
                        {
                            if (!dxSet.Textures.TryGetValue(binding.Binding, out var tex))
                                continue;

                            var dxTex = (DX11Texture)tex;
                            if (dxTex.Srv == null)
                            {
                                Console.WriteLine($"[DX11]: Cannot bind texture at {binding.Binding} since it does not have a SRV");
                                continue;
                            }
                            BindShaderResourceView(
                                binding.Binding,
                                binding.Stages,
                                dxTex);
                            break;
                        }

                    case DescriptorType.Sampler:
                        {
                            if (!dxSet.Samplers.TryGetValue(binding.Binding, out var samp))
                                continue;

                            var dxSampler = (Dx11Sampler)samp;

                            BindSampler(binding.Binding, binding.Stages, dxSampler.Sampler);
                            break;
                        }

                    case DescriptorType.StorageBuffer:
                        {
                            if (!dxSet.StorageBuffers.TryGetValue(binding.Binding, out var storageBuffer))
                                continue;

                            var dxStorageBuffer = (Dx11ShaderStorageBuffer)storageBuffer;

                            BindShaderResourceView(binding.Binding, binding.Stages, dxStorageBuffer);

                            break;
                        }
                    case DescriptorType.RwStorageBuffer:
                        {
                            if (!dxSet.RwStorageBuffers.TryGetValue(binding.Binding, out var storageBuffer))
                                continue;

                            var dxStorageBuffer = (Dx11ShaderStorageBuffer)storageBuffer;

                            BindUnorderedAccessView(
                                binding.Binding,
                                binding.Stages,
                                dxStorageBuffer);


                            break;
                        }
                    case DescriptorType.RwTexture:
                        {
                            if (!dxSet.RwTextures.TryGetValue(binding.Binding, out var texture))
                                continue;

                            var dxTexture = (DX11Texture)texture;

                            BindUnorderedAccessView(
                                binding.Binding,
                                binding.Stages,
                                dxTexture);

                            break;
                        }
                }
            }
        }

        public void Draw(int vertexCount, int startVertexLocation)
        {
            _deviceContext.Draw(vertexCount, startVertexLocation);
        }
        public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            _deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }
        public void DrawInstancedIndexed(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            _deviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
        }

        public void Dispatch(uint groupX, uint groupY, uint groupZ)
        {
            _deviceContext.Dispatch(
                (int)groupX,
                (int)groupY,
                (int)groupZ);
        }

        public void MemoryBarrier(MemoryBarrierFlags flags)
        {
            // No memory barriers in dx11 :)
        }

        public void ResolveSubresource(ITexture multi, ITexture single)
        {
            var multisampled = ((DX11Texture)multi);
            var singlesampled = ((DX11Texture)single);

            _deviceContext.ResolveSubresource(multisampled.TextureResource, 0, singlesampled.TextureResource, 0, singlesampled.Dx11Format);
        }

        public void ClearTextureResource(ITexture texture, ClearValue value)
        {
            var dx11Texture = (DX11Texture)texture;

            if (dx11Texture.Uav == null)
            {
                Console.WriteLine("[DX11]: Texture must have UAV to clear.");
                return;
            }
            if (value.Format == ClearValueFormat.Float)
                _deviceContext.ClearUnorderedAccessView(dx11Texture.Uav, new RawVector4(value.X, value.Y, value.Z, value.W));
            else
                _deviceContext.ClearUnorderedAccessView(dx11Texture.Uav, new RawInt4((int)value.X, (int)value.Y, (int)value.Z, (int)value.W));
        }
        public void CopyTextureSubresourceRegion(
            ITexture source,
            ITexture destination,
            int sourceX,
            int sourceY,
            int width,
            int height)
        {
            DX11Texture dx11Source = (DX11Texture)source;
            DX11Texture dx11Destination = (DX11Texture)destination;

            var sourceRegion = new ResourceRegion
            {
                Left = sourceX,
                Top = sourceY,
                Front = 0,
                Right = sourceX + width,
                Bottom = sourceY + height,
                Back = 1
            };

            _deviceContext.CopySubresourceRegion(
                dx11Source.TextureResource,
                0,
                sourceRegion,
                dx11Destination.TextureResource,
                0,
                0,
                0,
                0);
        }
        public MappedTexture MapTexture(ITexture texture, MapMode mode)
        {
            if (texture.Description.ResourceUsage != ResourceUsage.Staging)
                throw new InvalidOperationException(
                    "[DX11]: Cannot map a non-staging texture.");

            DX11Texture dx11Texture = (DX11Texture)texture;

            SharpDX.Direct3D11.MapMode dxMode = mode switch
            {
                MapMode.Read => SharpDX.Direct3D11.MapMode.Read,
                MapMode.Write => SharpDX.Direct3D11.MapMode.Write,
                MapMode.ReadWrite => SharpDX.Direct3D11.MapMode.ReadWrite,
                _ => throw new ArgumentOutOfRangeException(nameof(mode))
            };

            SharpDX.DataBox data = _deviceContext.MapSubresource(dx11Texture.TextureResource, 0, dxMode, MapFlags.None);

            return new MappedTexture(data.DataPointer, data.RowPitch, data.SlicePitch);
        }

        public void UnmapTexture(ITexture texture)
        {
            DX11Texture dx11Texture = (DX11Texture)texture;

            _deviceContext.UnmapSubresource(dx11Texture.TextureResource, 0);
        }

        public void WriteTimestamp(IGpuTimestamp timestamp)
        {
            Dx11GpuTimestamp dxTimestamp = (Dx11GpuTimestamp)timestamp;

            _deviceContext.End(dxTimestamp.Query);
        }

        public void BeginTimestampDisjoint(IGpuTimestampDisjoint disjoint)
        {
            Dx11GpuTimestampDisjoint dxDisjoint = (Dx11GpuTimestampDisjoint)disjoint;

            _deviceContext.Begin(dxDisjoint.Query);
        }

        public void EndTimestampDisjoint(IGpuTimestampDisjoint disjoint)
        {
            Dx11GpuTimestampDisjoint dxDisjoint = (Dx11GpuTimestampDisjoint)disjoint;

            _deviceContext.End(dxDisjoint.Query);
        }

        // InternalMethods

        internal void BindConstantBuffer(uint slot, ShaderStage stages, Buffer buffer)
        {
            if ((stages & ShaderStage.Vertex) != 0)
                _deviceContext.VertexShader.SetConstantBuffer((int)slot, buffer);
            if ((stages & ShaderStage.Fragment) != 0)
                _deviceContext.PixelShader.SetConstantBuffer((int)slot, buffer);
            if ((stages & ShaderStage.Geometry) != 0)
                _deviceContext.GeometryShader.SetConstantBuffer((int)slot, buffer);
            if ((stages & ShaderStage.Compute) != 0)
                _deviceContext.ComputeShader.SetConstantBuffer((int)slot, buffer);
        }
        internal void BindShaderResourceView(uint slot, ShaderStage stages, ShaderResourceView view)
        {
            if ((stages & ShaderStage.Vertex) != 0)
            {
                _deviceContext.VertexShader.SetShaderResource((int)slot, view);
            }

            if ((stages & ShaderStage.Fragment) != 0)
            {
                _deviceContext.PixelShader.SetShaderResource((int)slot, view);
            }

            if ((stages & ShaderStage.Compute) != 0)
            {
                _deviceContext.ComputeShader.SetShaderResource((int)slot, view);
            }
        }
        internal void BindShaderResourceView(uint slot, ShaderStage stages, DX11Texture tex)
        {
            ResolveForSrv(tex);

            if ((stages & ShaderStage.Vertex) != 0)
            {
                if (_boundVsSrVs[slot] != tex)
                {
                    _deviceContext.VertexShader.SetShaderResource(
                        (int)slot,
                        tex.Srv);

                    _boundVsSrVs[slot] = tex;
                }
            }

            if ((stages & ShaderStage.Fragment) != 0)
            {
                if (_boundPsSrVs[slot] != tex)
                {
                    _deviceContext.PixelShader.SetShaderResource(
                        (int)slot,
                        tex.Srv);

                    _boundPsSrVs[slot] = tex;
                }
            }

            if ((stages & ShaderStage.Compute) != 0)
            {
                if (_boundCsSrVs[slot] != tex)
                {
                    _deviceContext.ComputeShader.SetShaderResource(
                        (int)slot,
                        tex.Srv);

                    _boundCsSrVs[slot] = tex;
                }
            }
        }
        internal void BindShaderResourceView(uint slot, ShaderStage stages, Dx11ShaderStorageBuffer buffer)
        {
            ResolveForSrv(buffer);

            if ((stages & ShaderStage.Vertex) != 0)
            {
                if (_boundVsSsbOs[slot] != buffer)
                {
                    _deviceContext.VertexShader.SetShaderResource(
                        (int)slot,
                        buffer.Srv);

                    _boundVsSsbOs[slot] = buffer;
                }
            }

            if ((stages & ShaderStage.Fragment) != 0)
            {
                if (_boundPsSsbOs[slot] != buffer)
                {
                    _deviceContext.PixelShader.SetShaderResource(
                        (int)slot,
                        buffer.Srv);

                    _boundPsSsbOs[slot] = buffer;
                }
            }

            if ((stages & ShaderStage.Compute) != 0)
            {
                if (_boundCsSsbOs[slot] != buffer)
                {
                    _deviceContext.ComputeShader.SetShaderResource(
                        (int)slot,
                        buffer.Srv);

                    _boundCsSsbOs[slot] = buffer;
                }
            }
        }
        private void BindUnorderedAccessView(uint slot, ShaderStage stages, DX11Texture tex)
        {
            ResolveForUav(tex);
            if ((stages & ShaderStage.Compute) != 0)
            {
                _deviceContext.ComputeShader.SetUnorderedAccessView(
                        (int)slot,
                        tex.Uav);

                _boundCsUavTextures[slot] = tex;
            }

        }
        private void BindUnorderedAccessView(uint slot, ShaderStage stages, Dx11ShaderStorageBuffer buffer)
        {
            ResolveForUav(buffer);
            if ((stages & ShaderStage.Compute) != 0)
            {
                _deviceContext.ComputeShader.SetUnorderedAccessView(
                        (int)slot,
                        buffer.Uav);

                _boundCsUavBuffers[slot] = buffer;
            }
        }
        internal void BindSampler(uint slot, ShaderStage stages, SamplerState sampler)
        {
            if ((stages & ShaderStage.Vertex) != 0 &&
                _boundVsSamplers[slot] != sampler)
            {
                _deviceContext.VertexShader.SetSampler((int)slot, sampler);
                _boundVsSamplers[slot] = sampler;
            }

            if ((stages & ShaderStage.Fragment) != 0 &&
                _boundPsSamplers[slot] != sampler)
            {
                _deviceContext.PixelShader.SetSampler((int)slot, sampler);
                _boundPsSamplers[slot] = sampler;
            }

            if ((stages & ShaderStage.Geometry) != 0 &&
                _boundGsSamplers[slot] != sampler)
            {
                _deviceContext.GeometryShader.SetSampler((int)slot, sampler);
                _boundGsSamplers[slot] = sampler;
            }

            if ((stages & ShaderStage.Compute) != 0 &&
                _boundCsSamplers[slot] != sampler)
            {
                _deviceContext.ComputeShader.SetSampler((int)slot, sampler);
                _boundCsSamplers[slot] = sampler;
            }
        }
        private void ResolveForSrv(DX11Texture tex)
        {
            bool dirty = false;

            for (int i = 0; i < _boundRtVs.Length; i++)
            {
                if (_boundRtVs[i] == tex)
                {
                    Console.WriteLine(
                        $"[HAZARD] Unbinding RTV[{i}] " +
                        $"because texture is being bound as SRV");
                    dirty = true;
                }
            }

            if (_boundDsv == tex)
            {
                Console.WriteLine(
                    "[HAZARD] Unbinding DSV because texture is being bound as SRV");

                dirty = true;
            }

            for (int i = 0; i < _boundCsUavTextures.Length; i++)
            {
                if (_boundCsUavTextures[i] == tex)
                {
                    Console.WriteLine(
                        $"[HAZARD] Unbinding CS UAV[{i}] " +
                        $"because texture is being bound as SRV");

                    _deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
                    _boundCsUavTextures[i] = null;
                }
            }

            if (dirty)
            {
                _deviceContext.OutputMerger.SetRenderTargets(
                    null,
                    []);

                Array.Clear(_boundRtVs);
                _boundDsv = null;

                // CRITICAL:
                // The physical OM state no longer matches currentFramebuffer.
                _currentFramebuffer = null;
            }
        }
        private void ResolveForSrv(Dx11ShaderStorageBuffer buffer)
        {
            for (int i = 0; i < _boundCsUavBuffers.Length; i++)
            {
                if (_boundCsUavBuffers[i] == buffer)
                {
                    _deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
                    _boundCsUavBuffers[i] = null;
                }
            }
        }
        private void ResolveForRtv(DX11Texture tex)
        {
            for (int i = 0; i < _boundPsSrVs.Length; i++)
            {
                if (_boundPsSrVs[i] == tex)
                {
                    _deviceContext.PixelShader.SetShaderResource(i, null);
                    _boundPsSrVs[i] = null;
                }

                if (_boundVsSrVs[i] == tex)
                {
                    _deviceContext.VertexShader.SetShaderResource(i, null);
                    _boundVsSrVs[i] = null;
                }

                if (_boundCsSrVs[i] == tex)
                {
                    _deviceContext.ComputeShader.SetShaderResource(i, null);
                    _boundCsSrVs[i] = null;
                }
            }
            for (int i = 0; i < _boundCsUavTextures.Length; i++)
            {
                if (_boundCsUavTextures[i] == tex)
                {
                    _deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
                    _boundCsUavTextures[i] = null;
                }
            }
        }
        private void ResolveForUav(DX11Texture tex)
        {
            // Remove from all SRV bindings

            for (int i = 0; i < _boundVsSrVs.Length; i++)
            {
                if (_boundVsSrVs[i] == tex)
                {
                    _deviceContext.VertexShader.SetShaderResource(i, null);
                    _boundVsSrVs[i] = null;
                }
            }

            for (int i = 0; i < _boundPsSrVs.Length; i++)
            {
                if (_boundPsSrVs[i] == tex)
                {
                    _deviceContext.PixelShader.SetShaderResource(i, null);
                    _boundPsSrVs[i] = null;
                }
            }

            for (int i = 0; i < _boundCsSrVs.Length; i++)
            {
                if (_boundCsSrVs[i] == tex)
                {
                    _deviceContext.ComputeShader.SetShaderResource(i, null);
                    _boundCsSrVs[i] = null;
                }
            }

            // Remove from any RTV bindings

            bool updateOm = false;

            for (int i = 0; i < _boundRtVs.Length; i++)
            {
                if (_boundRtVs[i] == tex)
                {
                    _boundRtVs[i] = null;

                    if (_currentFramebuffer != null)
                        _currentFramebuffer.RtVs[i] = null;

                    updateOm = true;
                }
            }

            // Remove from DSV

            if (_boundDsv == tex)
            {
                _boundDsv = null;

                if (_currentFramebuffer != null)
                    _currentFramebuffer.Dsv = null;

                updateOm = true;
            }

            if (updateOm && _currentFramebuffer != null)
            {
                _deviceContext.OutputMerger.SetRenderTargets(
                    _currentFramebuffer.Dsv,
                    _currentFramebuffer.RtVs);
            }
        }
        private void ResolveForUav(Dx11ShaderStorageBuffer buffer)
        {
            for (int i = 0; i < _boundVsSsbOs.Length; i++)
            {
                if (_boundVsSsbOs[i] == buffer)
                {
                    _deviceContext.VertexShader.SetShaderResource(i, null);
                    _boundVsSsbOs[i] = null;
                }
            }

            for (int i = 0; i < _boundPsSsbOs.Length; i++)
            {
                if (_boundPsSsbOs[i] == buffer)
                {
                    _deviceContext.PixelShader.SetShaderResource(i, null);
                    _boundPsSsbOs[i] = null;
                }
            }

            for (int i = 0; i < _boundCsSsbOs.Length; i++)
            {
                if (_boundCsSsbOs[i] == buffer)
                {
                    _deviceContext.ComputeShader.SetShaderResource(i, null);
                    _boundCsSsbOs[i] = null;
                }
            }
        }
    
    }
}
