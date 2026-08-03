namespace DevoidGPU
{
    public class ShaderReflectionData
    {
        public List<UniformBufferInfo> UniformBuffers { get; } = [];
        public List<ShaderResourceInfo> Resources { get; } = [];
        public List<InputParameterInfo> InputParameters { get; } = [];


        public int GetUniformBufferSlot(string name)
        {
            var buffers = UniformBuffers;
            for (int j = 0; j < buffers.Count; j++)
            {
                var buffer = buffers[j];
                if (string.Equals(buffer.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return j;
                }
            }
            return -1;
        }

        public static ShaderReflectionData Merge(
            params ShaderReflectionData[] reflections
        )
        {
            ShaderReflectionData result = new();

            HashSet<string> resources = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> inputs = new(StringComparer.OrdinalIgnoreCase);

            foreach (ShaderReflectionData reflection in reflections)
            {
                foreach (UniformBufferInfo buffer in reflection.UniformBuffers)
                {
                    AddUniformBuffer(result, buffer);
                }

                foreach (ShaderResourceInfo resource in reflection.Resources)
                {
                    if (resources.Add(resource.Name))
                    {
                        result.Resources.Add(resource);
                    }
                }

                foreach (ShaderResourceInfo resource in reflection.Resources)
                {
                    AddResource(result, resource);
                }

                foreach (InputParameterInfo input in reflection.InputParameters)
                {
                    if (inputs.Add(input.SemanticName))
                    {
                        result.InputParameters.Add(input);
                    }
                }
            }

            return result;
        }

        private static void AddUniformBuffer(
            ShaderReflectionData result,
            UniformBufferInfo buffer)
        {
            UniformBufferInfo? existing =
                result.UniformBuffers.FirstOrDefault(x =>
                    string.Equals(
                        x.Name,
                        buffer.Name,
                        StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                result.UniformBuffers.Add(buffer);
                return;
            }

            if (existing.BindSlot != buffer.BindSlot)
            {
                throw new Exception(
                    $"Uniform buffer '{buffer.Name}' uses different slots.");
            }

            if (existing.Size != buffer.Size)
            {
                throw new Exception(
                    $"Uniform buffer '{buffer.Name}' uses different sizes.");
            }

            // Merge shader stages
            existing.Stages |= buffer.Stages;
        }

        private static void AddResource(
            ShaderReflectionData result,
            ShaderResourceInfo resource)
        {
            ShaderResourceInfo? existing =
                result.Resources.FirstOrDefault(x =>
                    string.Equals(x.Name, resource.Name,
                        StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                result.Resources.Add(resource);
                return;
            }

            if (existing.BindSlot != resource.BindSlot)
                throw new Exception($"Resource '{resource.Name}' uses different slots.");

            if (existing.Type != resource.Type)
                throw new Exception($"Resource '{resource.Name}' uses different types.");

            existing.Stage |= resource.Stage;
        }


        public static void Print(ShaderReflectionData reflection, string? name = null)
        {
            Console.WriteLine();
            Console.WriteLine("======================================================");
            Console.WriteLine($"Shader Reflection Data{(name != null ? $" : {name}" : "")}");
            Console.WriteLine("======================================================");

            Console.WriteLine($"Uniform Buffers : {reflection.UniformBuffers.Count}");
            Console.WriteLine($"Resources       : {reflection.Resources.Count}");
            Console.WriteLine($"Input Params    : {reflection.InputParameters.Count}");
            Console.WriteLine();

            Console.WriteLine("Uniform Buffers");
            Console.WriteLine("----------------");

            foreach (var buffer in reflection.UniformBuffers)
            {
                Console.WriteLine(
                    $"{buffer.Name}  " +
                    $"Slot={buffer.BindSlot}  " +
                    $"Size={buffer.Size}  " +
                    $"Stages={buffer.Stages}");

                foreach (var variable in buffer.Variables)
                {
                    Console.WriteLine(
                        $"    {variable.Name,-24} " +
                        $"Offset={variable.Offset,3}  " +
                        $"Size={variable.Size,3}  " +
                        $"Type={variable.Type}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Resources");
            Console.WriteLine("---------");

            foreach (var resource in reflection.Resources)
            {
                Console.WriteLine(
                    $"{resource.Name,-24} " +
                    $"Type={resource.Type,-20} " +
                    $"Slot={resource.BindSlot} " +
                    $"Stages={resource.Stage}");
            }

            Console.WriteLine();

            Console.WriteLine("Input Parameters");
            Console.WriteLine("----------------");

            foreach (var input in reflection.InputParameters)
            {
                Console.WriteLine(
                    $"{input.SemanticName}{input.SemanticIndex}");
            }

            Console.WriteLine("======================================================");
            Console.WriteLine();
        }
    }
}
