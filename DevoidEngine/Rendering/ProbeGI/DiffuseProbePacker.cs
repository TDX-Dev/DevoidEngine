using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering.ProbeGI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DiffuseProbePackData
    {
        public uint ProbeIndex;
        public float Weight;
        public uint Padding0;
        public uint Padding1;
    }

    public sealed class DiffuseProbePacker : IDisposable
    {
        readonly MaterialInstance material;
        readonly IComputePipeline pipeline;
        readonly UniformBuffer dataBuffer;

        public DiffuseProbePacker()
        {
            material =
                new MaterialInstance(
                    new Material(
                        Shader.FromDescriptorFile(
                            Engine.GraphicsDevice,
                            Path.Combine(
                                Engine.BasePath,
                                "Content/DevoidShaderDescriptors/diffuse_probe_pack.dsd"))));

            pipeline =
                material.BaseMaterial.DefaultPass.GetComputePipeline();

            dataBuffer =
                UniformBuffer.Create(
                    ResourceUsage.Dynamic,
                    (uint)Unsafe.SizeOf<DiffuseProbePackData>());
        }

        public void Pack(ICommandList cmd, ShaderStorageBuffer<SH9> input, ShaderStorageBuffer<DiffuseProbe> output, uint probeIndex, float weight)
        {
            dataBuffer.Update(new DiffuseProbePackData
            {
                ProbeIndex = probeIndex,
                Weight = weight
            });

            material.DescriptorSet.SetShaderStorageBuffer(0, input.GPU);
            material.DescriptorSet.SetRWShaderStorageBuffer(0, output.GPU);
            material.DescriptorSet.SetUniformBuffer(0, dataBuffer.GPU);

            cmd.SetComputePipeline(pipeline);
            cmd.SetDescriptorSet(0, material.DescriptorSet);
            cmd.Dispatch(1, 1, 1);
        }

        public void Dispose()
        {
            dataBuffer.Dispose();
            material.Dispose();
        }
    }
}