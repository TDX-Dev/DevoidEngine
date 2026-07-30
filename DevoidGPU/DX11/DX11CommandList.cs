using SharpDX.Direct3D11;
using System.Numerics;
using Buffer = SharpDX.Direct3D11.Buffer;


namespace DevoidGPU.DX11
{
    internal sealed class DX11CommandList : ICommandList
    {
        public CommandListType Type { get; }


        private readonly DeviceContext deviceContext;

        // binding cache;
        private DX11Framebuffer? currentFramebuffer;
        private (int, int, int, int) currentViewport;

        private readonly DX11Texture?[] boundPS_SRVs = new DX11Texture?[16];
        private readonly DX11Texture?[] boundVS_SRVs = new DX11Texture?[16];
        private readonly DX11Texture?[] boundCS_SRVs = new DX11Texture?[16];

        private readonly DX11Texture?[] boundRTVs = new DX11Texture?[8];
        private DX11Texture? boundDSV;

        public DX11CommandList(DeviceContext context) { deviceContext = context; }

        public void Begin()
        {
            currentFramebuffer = null;
            currentViewport = default;

            // optional safety reset
            deviceContext.InputAssembler.InputLayout = null;
        }

        public void End()
        {

            /* No Op */
        }

        public void Reset()
        {
            currentFramebuffer = null;
            currentViewport = default;
        }

        public void SetViewport(int x, int y, int width, int height)
        {
            if ((x, y, width, height) == currentViewport)
                return;
            deviceContext.Rasterizer.SetViewport(x, y, width, height);
            currentViewport = (x, y, width, height);
        }

        public void SetScissor(int x, int y, int width, int height)
        {
            deviceContext.Rasterizer.SetScissorRectangle(x, y, width, height);
        }

        public void SetFramebuffer(IFrameBuffer framebuffer)
        {
            DX11Framebuffer dx11Fb = (DX11Framebuffer)framebuffer;
            if (ReferenceEquals(currentFramebuffer, dx11Fb))
                return;

            currentFramebuffer = dx11Fb;

            dx11Fb.ValidateFrameBuffer();

            for (int i = 0; i < dx11Fb.ColorAttachments.Count; i++)
            {
                if (dx11Fb.ColorAttachments[i] is DX11Texture tex)
                {
                    ResolveForRTV(tex);

                    boundRTVs[i] = tex;
                }
            }

            if (dx11Fb.DepthAttachment is DX11Texture depth)
            {
                ResolveForRTV(depth);

                boundDSV = depth;
            }

            deviceContext.OutputMerger.SetRenderTargets(dx11Fb.DSV, dx11Fb.RTVs);
        }

        public void ClearColor(int attachmentIndex, Vector4 color)
        {
            if (currentFramebuffer == null)
                throw new InvalidOperationException("Framebuffer not bound");

            if (attachmentIndex < 0 || attachmentIndex >= currentFramebuffer.RTVs.Length)
                throw new ArgumentOutOfRangeException(nameof(attachmentIndex));

            deviceContext.ClearRenderTargetView(currentFramebuffer.RTVs[attachmentIndex], new SharpDX.Mathematics.Interop.RawColor4(color.X, color.Y, color.Z, color.W));
        }

        public void ClearDepthStencil(float depth, byte stencil)
        {
            if (currentFramebuffer?.DSV == null)
                throw new InvalidOperationException("No depth stencil bound.");

            deviceContext.ClearDepthStencilView(
                currentFramebuffer.DSV,
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil,
                depth,
                stencil
            );
        }

        public void GenerateMipmaps(ITexture texture)
        {
            var dx11Texture = (DX11Texture)texture;

            deviceContext.GenerateMips(dx11Texture.SRV);
        }

        public void SetPipeline(IPipeline pipeline)
        {
            var p = (DX11GraphicsPipeline)pipeline;

            deviceContext.InputAssembler.PrimitiveTopology = p.Topology;
            deviceContext.InputAssembler.InputLayout = p.InputLayout;

            deviceContext.VertexShader.Set(p.VS);
            deviceContext.PixelShader.Set(p.PS);

            deviceContext.Rasterizer.State = p.RasterizerState;
            deviceContext.OutputMerger.SetDepthStencilState(p.DepthStencilState);
            deviceContext.OutputMerger.SetBlendState(p.BlendState);

            deviceContext.OutputMerger.SetBlendState(p.BlendState);
            deviceContext.OutputMerger.SetDepthStencilState(p.DepthStencilState);
        }
        public void SetVertexBuffer(IVertexBuffer buffer)
        {
            var vb = (DX11VertexBuffer)buffer;

            deviceContext.InputAssembler.SetVertexBuffers(
                vb.Slot,
                new VertexBufferBinding(vb.Buffer, vb.Stride, 0)
            );
        }

        public void SetIndexBuffer(IIndexBuffer buffer)
        {
            var ib = (DX11IndexBuffer)buffer;

            deviceContext.InputAssembler.SetIndexBuffer(
                ib.Buffer,
                DX11StateMapper.ToDXIndexFormat(buffer.Format),
                0
            );
        }

        public void SetDescriptorSet(uint setIndex, IDescriptorSet set)
        {
            var dxSet = (DX11DescriptorSet)set;
            var layout = (DX11DescriptorLayout)dxSet.Layout;

            foreach (var binding in layout.Bindings)
            {
                switch (binding.Type)
                {
                    case DescriptorType.UniformBuffer:
                        {
                            if (!dxSet.uniformBuffers.TryGetValue(binding.Binding, out var ub))
                                continue;

                            var dxUB = (DX11UniformBuffer)ub;

                            BindConstantBuffer(binding.Binding, binding.Stages, dxUB.Buffer);
                            break;
                        }

                    case DescriptorType.Texture:
                        {
                            if (!dxSet.textures.TryGetValue(binding.Binding, out var tex))
                                continue;

                            var dxTex = (DX11Texture)tex;
                            if (dxTex.SRV == null)
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
                            if (!dxSet.samplers.TryGetValue(binding.Binding, out var samp))
                                continue;

                            var dxSampler = (DX11Sampler)samp;

                            BindSampler(binding.Binding, binding.Stages, dxSampler.Sampler);
                            break;
                        }

                    case DescriptorType.StorageBuffer:
                        {
                            if (!dxSet.storageBuffers.TryGetValue(binding.Binding, out var storageBuffer))
                                continue;

                            var dxStorageBuffer = (DX11ShaderStorageBuffer)storageBuffer;

                            BindShaderResourceView(binding.Binding, binding.Stages, dxStorageBuffer.SRV!);

                            break;
                        }
                }
            }
        }

        public void Draw(int vertexCount, int startVertexLocation)
        {
            deviceContext.Draw(vertexCount, startVertexLocation);
        }
        public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }


        // InternalMethods

        internal void BindConstantBuffer(uint slot, ShaderStage stages, Buffer buffer)
        {
            if ((stages & ShaderStage.Vertex) != 0)
                deviceContext.VertexShader.SetConstantBuffer((int)slot, buffer);
            if ((stages & ShaderStage.Fragment) != 0)
                deviceContext.PixelShader.SetConstantBuffer((int)slot, buffer);
            if ((stages & ShaderStage.Geometry) != 0)
                deviceContext.GeometryShader.SetConstantBuffer((int)slot, buffer);
            if ((stages & ShaderStage.Compute) != 0)
                deviceContext.ComputeShader.SetConstantBuffer((int)slot, buffer);
        }
        internal void BindShaderResourceView(uint slot, ShaderStage stages, ShaderResourceView view)
        {
            if ((stages & ShaderStage.Vertex) != 0)
            {
                deviceContext.VertexShader.SetShaderResource((int)slot, view);
            }

            if ((stages & ShaderStage.Fragment) != 0)
            {
                deviceContext.PixelShader.SetShaderResource((int)slot, view);
            }

            if ((stages & ShaderStage.Compute) != 0)
            {
                deviceContext.ComputeShader.SetShaderResource((int)slot, view);
            }
        }

        internal void BindShaderResourceView(
            uint slot,
            ShaderStage stages,
            DX11Texture tex
        )
        {
            ResolveForSRV(tex);

            if ((stages & ShaderStage.Vertex) != 0)
            {
                deviceContext.VertexShader.SetShaderResource(
                    (int)slot,
                    tex.SRV);

                boundVS_SRVs[slot] = tex;
            }

            if ((stages & ShaderStage.Fragment) != 0)
            {
                deviceContext.PixelShader.SetShaderResource(
                    (int)slot,
                    tex.SRV);

                boundPS_SRVs[slot] = tex;
            }

            if ((stages & ShaderStage.Compute) != 0)
            {
                deviceContext.ComputeShader.SetShaderResource(
                    (int)slot,
                    tex.SRV);

                boundCS_SRVs[slot] = tex;
            }
        }

        internal void BindSampler(uint slot, ShaderStage stages, SamplerState sampler)
        {
            if ((stages & ShaderStage.Vertex) != 0)
                deviceContext.VertexShader.SetSampler((int)slot, sampler);
            if ((stages & ShaderStage.Fragment) != 0)
                deviceContext.PixelShader.SetSampler((int)slot, sampler);
            if ((stages & ShaderStage.Geometry) != 0)
                deviceContext.GeometryShader.SetSampler((int)slot, sampler);
            if ((stages & ShaderStage.Compute) != 0)
                deviceContext.ComputeShader.SetSampler((int)slot, sampler);
        }

        private void ResolveForSRV(DX11Texture tex)
        {
            bool dirty = false;

            for (int i = 0; i < boundRTVs.Length; i++)
            {
                if (boundRTVs[i] == tex)
                {
                    boundRTVs[i] = null;
                    dirty = true;
                }
            }

            if (boundDSV == tex)
            {
                boundDSV = null;
                dirty = true;
            }

            if (dirty)
            {
                deviceContext.OutputMerger.SetRenderTargets(
                    (DepthStencilView?)null,
                    []);
            }
        }

        private void ResolveForRTV(DX11Texture tex)
        {
            for (int i = 0; i < 16; i++)
            {
                if (boundPS_SRVs[i] == tex)
                {
                    deviceContext.PixelShader.SetShaderResource(i, null);
                    boundPS_SRVs[i] = null;
                }

                if (boundVS_SRVs[i] == tex)
                {
                    deviceContext.VertexShader.SetShaderResource(i, null);
                    boundVS_SRVs[i] = null;
                }

                if (boundCS_SRVs[i] == tex)
                {
                    deviceContext.ComputeShader.SetShaderResource(i, null);
                    boundCS_SRVs[i] = null;
                }
            }
        }

        public void SetFramebuffer(IFrameBuffer framebuffer, int mipLevel = 0, int arraySlice = 0)
        {
            throw new NotImplementedException();
        }
    }
}
