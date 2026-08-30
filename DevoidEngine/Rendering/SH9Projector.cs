using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SH9ReduceData
    {
        public uint InputCount;
        public uint FinalPass;
        public uint OutputOffset;
        public uint Padding;
    }

    public sealed class SH9Projector : IDisposable
    {
        readonly MaterialInstance projectMaterial;
        readonly MaterialInstance reduceMaterial;

        readonly IComputePipeline projectPipeline;
        readonly IComputePipeline reducePipeline;

        readonly ShaderStorageBuffer<SH9> partialSH;
        readonly ShaderStorageBuffer<SH9> partialSH2;
        readonly ShaderStorageBuffer<SH9> outputSH;

        readonly UniformBuffer reduceInputBuffer;

        readonly uint partialCount;

        public ShaderStorageBuffer<SH9> Output =>
            outputSH;

        public SH9Projector(int resolution, int layers = 6)
        {
            if (resolution <= 0 || resolution % 8 != 0)
            {
                throw new ArgumentException("SH9 projection resolution must be greater than zero and divisible by 8.", nameof(resolution));
            }

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(layers);

            projectMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/sh9_project.dsd"))));

            reduceMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine( Engine.BasePath, "Content/DevoidShaderDescriptors/sh9_reduce.dsd"))));

            projectPipeline = projectMaterial.BaseMaterial.DefaultPass.GetComputePipeline();

            reducePipeline = reduceMaterial.BaseMaterial.DefaultPass.GetComputePipeline();

            uint groupsX = (uint)resolution / 8;

            uint groupsY = (uint)resolution / 8;

            partialCount = groupsX * groupsY * (uint)layers;

            partialSH = ShaderStorageBuffer<SH9>.Create(ResourceUsage.Default, partialCount, BufferBind.StorageWritable);

            partialSH2 = ShaderStorageBuffer<SH9>.Create( ResourceUsage.Default, partialCount, BufferBind.StorageWritable);

            outputSH = ShaderStorageBuffer<SH9>.Create( ResourceUsage.Default, 1, BufferBind.StorageWritable);

            reduceInputBuffer = UniformBuffer.Create( ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<SH9ReduceData>());
        }

        public void ProjectCubemap(ICommandList cmd, Texture cubemap, int resolution)
        {
            projectMaterial.SetTexture(
                "MAT_Cubemap",
                cubemap);

            projectMaterial.SetFloat(
                "CubemapResolution",
                resolution);

            projectMaterial.DescriptorSet
                .SetRWShaderStorageBuffer(
                    0,
                    partialSH.GPU);

            cmd.SetComputePipeline(
                projectPipeline);

            cmd.SetDescriptorSet(
                0,
                projectMaterial.DescriptorSet);

            cmd.Dispatch(
                (uint)resolution / 8,
                (uint)resolution / 8,
                6);
        }

        public void Reduce(ICommandList cmd, ShaderStorageBuffer<SH9>? destinationBuffer = null, uint destinationOffset = 0)
        {
            uint count = partialCount;

            ShaderStorageBuffer<SH9> input = partialSH;

            ShaderStorageBuffer<SH9> output = partialSH2;

            while (count > 1)
            {
                uint outputCount = (count + 1) / 2;

                bool finalPass = outputCount == 1;

                ShaderStorageBuffer<SH9> destination = finalPass ? destinationBuffer ?? outputSH : output;

                reduceMaterial.DescriptorSet.SetShaderStorageBuffer(0, input.GPU);

                reduceMaterial.DescriptorSet.SetRWShaderStorageBuffer(0, destination.GPU);

                reduceInputBuffer.Update(new SH9ReduceData
                {
                    InputCount = count,
                    FinalPass = finalPass ? 1u : 0u,
                    OutputOffset = destinationBuffer != null ? destinationOffset : 0
                });

                reduceMaterial.DescriptorSet.SetUniformBuffer(0, reduceInputBuffer.GPU);

                cmd.SetComputePipeline(reducePipeline);

                cmd.SetDescriptorSet(0, reduceMaterial.DescriptorSet);

                uint groups = (outputCount + 63) / 64;

                cmd.Dispatch(groups, 1, 1);

                if (!finalPass)
                {
                    (input, output) = (output, input);
                }

                count = outputCount;
            }
        }

        public void ProjectAndReduce(ICommandList cmd, Texture cubemap, int resolution, ShaderStorageBuffer<SH9>? destinationBuffer = null, uint destinationOffset = 0)
        {
            ProjectCubemap(cmd, cubemap, resolution);

            Reduce(cmd, destinationBuffer, destinationOffset);
        }

        public void Dispose()
        {
            partialSH.Dispose();
            partialSH2.Dispose();
            outputSH.Dispose();

            reduceInputBuffer.Dispose();

            projectMaterial.Dispose();
            reduceMaterial.Dispose();
        }
    }
}