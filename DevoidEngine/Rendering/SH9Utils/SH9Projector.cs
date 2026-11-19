using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering.SH9Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Sh9ReduceData
    {
        public uint InputCount;
        public uint FinalPass;
        public uint OutputOffset;
        public uint Padding;
    }

    public sealed class Sh9Projector : IDisposable
    {
        readonly MaterialInstance _projectMaterial;
        readonly MaterialInstance _reduceMaterial;

        readonly IComputePipeline _projectPipeline;
        readonly IComputePipeline _reducePipeline;

        readonly ShaderStorageBuffer<Sh9> _partialSh;
        readonly ShaderStorageBuffer<Sh9> _partialSh2;
        readonly ShaderStorageBuffer<Sh9> _outputSh;

        readonly UniformBuffer _reduceInputBuffer;

        readonly uint _partialCount;

        public ShaderStorageBuffer<Sh9> Output =>
            _outputSh;

        public Sh9Projector(int resolution, int layers = 6)
        {
            if (resolution <= 0 || resolution % 8 != 0)
            {
                throw new ArgumentException("SH9 projection resolution must be greater than zero and divisible by 8.", nameof(resolution));
            }

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(layers);

            _projectMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/sh9_project.dsd"))));

            _reduceMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/sh9_reduce.dsd"))));

            _projectPipeline = _projectMaterial.BaseMaterial.DefaultPass.GetComputePipeline();

            _reducePipeline = _reduceMaterial.BaseMaterial.DefaultPass.GetComputePipeline();

            uint groupsX = (uint)resolution / 8;

            uint groupsY = (uint)resolution / 8;

            _partialCount = groupsX * groupsY * (uint)layers;

            _partialSh = ShaderStorageBuffer<Sh9>.Create(ResourceUsage.Default, _partialCount, BufferBind.StorageWritable);

            _partialSh2 = ShaderStorageBuffer<Sh9>.Create(ResourceUsage.Default, _partialCount, BufferBind.StorageWritable);

            _outputSh = ShaderStorageBuffer<Sh9>.Create(ResourceUsage.Default, 1, BufferBind.StorageWritable);

            _reduceInputBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<Sh9ReduceData>());
        }

        public void ProjectCubemap(ICommandList cmd, Texture cubemap, int resolution)
        {
            _projectMaterial.SetTexture(
                "MAT_Cubemap",
                cubemap);

            _projectMaterial.SetFloat(
                "CubemapResolution",
                resolution);

            _projectMaterial.DescriptorSet
                .SetRWShaderStorageBuffer(
                    0,
                    _partialSh.Gpu);

            cmd.SetComputePipeline(
                _projectPipeline);

            cmd.SetDescriptorSet(
                0,
                _projectMaterial.DescriptorSet);

            cmd.Dispatch(
                (uint)resolution / 8,
                (uint)resolution / 8,
                6);
        }

        public void Reduce(ICommandList cmd, ShaderStorageBuffer<Sh9>? destinationBuffer = null, uint destinationOffset = 0)
        {
            uint count = _partialCount;

            ShaderStorageBuffer<Sh9> input = _partialSh;

            ShaderStorageBuffer<Sh9> output = _partialSh2;

            while (count > 1)
            {
                uint outputCount = (count + 1) / 2;

                bool finalPass = outputCount == 1;

                ShaderStorageBuffer<Sh9> destination = finalPass ? destinationBuffer ?? _outputSh : output;

                _reduceMaterial.DescriptorSet.SetShaderStorageBuffer(0, input.Gpu);

                _reduceMaterial.DescriptorSet.SetRWShaderStorageBuffer(0, destination.Gpu);

                _reduceInputBuffer.Update(new Sh9ReduceData
                {
                    InputCount = count,
                    FinalPass = finalPass ? 1u : 0u,
                    OutputOffset = destinationBuffer != null ? destinationOffset : 0
                });

                _reduceMaterial.DescriptorSet.SetUniformBuffer(0, _reduceInputBuffer.Gpu);

                cmd.SetComputePipeline(_reducePipeline);

                cmd.SetDescriptorSet(0, _reduceMaterial.DescriptorSet);

                uint groups = (outputCount + 63) / 64;

                cmd.Dispatch(groups, 1, 1);

                if (!finalPass)
                {
                    (input, output) = (output, input);
                }

                count = outputCount;
            }
        }

        public void ProjectAndReduce(ICommandList cmd, Texture cubemap, int resolution, ShaderStorageBuffer<Sh9>? destinationBuffer = null, uint destinationOffset = 0)
        {
            ProjectCubemap(cmd, cubemap, resolution);

            Reduce(cmd, destinationBuffer, destinationOffset);
        }

        public void Dispose()
        {
            _partialSh.Dispose();
            _partialSh2.Dispose();
            _outputSh.Dispose();

            _reduceInputBuffer.Dispose();

            _projectMaterial.Dispose();
            _reduceMaterial.Dispose();
        }
    }
}