using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System.Diagnostics;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;
using Buffer = SharpDX.Direct3D11.Buffer;


namespace DevoidGPU.DX11
{
    internal sealed class DX11CommandList : ICommandList
    {
        private const int MaxSRVs = 24;


        public CommandListType Type { get; }


        private readonly DeviceContext deviceContext;

        // binding cache;
        private DX11Framebuffer? currentFramebuffer;
        private (int, int, int, int) currentViewport;

        private readonly DX11Texture?[] boundPS_SRVs = new DX11Texture?[MaxSRVs];
        private readonly DX11Texture?[] boundVS_SRVs = new DX11Texture?[MaxSRVs];
        private readonly DX11Texture?[] boundCS_SRVs = new DX11Texture?[MaxSRVs];


        private readonly DX11Texture?[] boundRTVs = new DX11Texture?[8];
        private DX11Texture? boundDSV;

        private readonly DX11ShaderStorageBuffer?[] boundPS_SSBOs = new DX11ShaderStorageBuffer?[MaxSRVs];
        private readonly DX11ShaderStorageBuffer?[] boundVS_SSBOs = new DX11ShaderStorageBuffer?[MaxSRVs];
        private readonly DX11ShaderStorageBuffer?[] boundCS_SSBOs = new DX11ShaderStorageBuffer?[MaxSRVs];

        private readonly DX11Texture?[] boundCS_UAVTextures = new DX11Texture?[8];
        private readonly DX11ShaderStorageBuffer?[] boundCS_UAVBuffers = new DX11ShaderStorageBuffer?[8];

        private readonly SamplerState?[] boundVS_Samplers = new SamplerState?[MaxSRVs];
        private readonly SamplerState?[] boundPS_Samplers = new SamplerState?[MaxSRVs];
        private readonly SamplerState?[] boundCS_Samplers = new SamplerState?[MaxSRVs];
        private readonly SamplerState?[] boundGS_Samplers = new SamplerState?[MaxSRVs];

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

        public void SetScissor(int left, int top, int right, int bottom)
        {
            deviceContext.Rasterizer.SetScissorRectangle(
                left,
                top,
                right,
                bottom);
        }

        public void SetFramebuffer(IFrameBuffer? framebuffer)
        {
            if (framebuffer == null)
            {
                deviceContext.OutputMerger.SetRenderTargets(
                    null,
                    (RenderTargetView[])null!);

                currentFramebuffer = null;

                Array.Clear(boundRTVs);
                boundDSV = null;

                return;
            }

            DX11Framebuffer dx11Fb = (DX11Framebuffer)framebuffer;

            if (ReferenceEquals(currentFramebuffer, dx11Fb) && !dx11Fb.Dirty)
                return;

            currentFramebuffer = dx11Fb;
            dx11Fb.Dirty = false;

            dx11Fb.ValidateFrameBuffer();

            Array.Clear(boundRTVs);
            boundDSV = null;

            for (int i = 0; i < dx11Fb.ColorAttachments.Count; i++)
            {
                if (dx11Fb.ColorAttachments[i] is DX11Texture tex)
                {
                    ResolveForRTV(tex);
                    boundRTVs[i] = tex;
                }
                else
                {
                    boundRTVs[i] = null;
                }
            }

            if (dx11Fb.DepthAttachment is DX11Texture depth)
            {
                ResolveForRTV(depth);
                boundDSV = depth;
            }
            else
            {
                boundDSV = null;
            }

            deviceContext.OutputMerger.SetRenderTargets(
                dx11Fb.DSV,
                dx11Fb.RTVs);
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
            deviceContext.ComputeShader.Set(null);

            deviceContext.Rasterizer.State = p.RasterizerState;
            deviceContext.OutputMerger.SetDepthStencilState(p.DepthStencilState);
            deviceContext.OutputMerger.SetBlendState(p.BlendState);
        }
        public void SetComputePipeline(IComputePipeline pipeline)
        {
            var dx = (DX11ComputePipeline)pipeline;

            deviceContext.ComputeShader.Set(dx.Shader.CS);
            deviceContext.VertexShader.Set(null);
            deviceContext.PixelShader.Set(null);
            deviceContext.GeometryShader.Set(null);
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

                            BindShaderResourceView(binding.Binding, binding.Stages, dxStorageBuffer);

                            break;
                        }
                    case DescriptorType.RWStorageBuffer:
                        {
                            if (!dxSet.rwStorageBuffers.TryGetValue(binding.Binding, out var storageBuffer))
                                continue;

                            var dxStorageBuffer = (DX11ShaderStorageBuffer)storageBuffer;

                            BindUnorderedAccessView(
                                binding.Binding,
                                binding.Stages,
                                dxStorageBuffer);


                            break;
                        }
                    case DescriptorType.RWTexture:
                        {
                            if (!dxSet.rwTextures.TryGetValue(binding.Binding, out var texture))
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
            deviceContext.Draw(vertexCount, startVertexLocation);
        }
        public void DrawIndexed(int indexCount, int startIndexLocation, int baseVertexLocation)
        {
            deviceContext.DrawIndexed(indexCount, startIndexLocation, baseVertexLocation);
        }
        public void DrawInstancedIndexed(int indexCountPerInstance, int instanceCount, int startIndexLocation, int baseVertexLocation, int startInstanceLocation)
        {
            deviceContext.DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
        }

        public void Dispatch(uint groupX, uint groupY, uint groupZ)
        {
            deviceContext.Dispatch(
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

            deviceContext.ResolveSubresource(multisampled.TextureResource, 0, singlesampled.TextureResource, 0, singlesampled.DX11Format);
        }

        public void ClearTextureResource(ITexture texture, ClearValue value)
        {
            var dx11Texture = (DX11Texture)texture;

            if (dx11Texture.UAV == null)
            {
                Console.WriteLine("[DX11]: Texture must have UAV to clear.");
                return;
            }
            if (value.Format == ClearValueFormat.Float)
                deviceContext.ClearUnorderedAccessView(dx11Texture.UAV, new RawVector4(value.X, value.Y, value.Z, value.W));
            else
                deviceContext.ClearUnorderedAccessView(dx11Texture.UAV, new RawInt4((int)value.X, (int)value.Y, (int)value.Z, (int)value.W));
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
        internal void BindShaderResourceView(uint slot, ShaderStage stages, DX11Texture tex)
        {
            ResolveForSRV(tex);

            if ((stages & ShaderStage.Vertex) != 0)
            {
                if (boundVS_SRVs[slot] != tex)
                {
                    deviceContext.VertexShader.SetShaderResource(
                        (int)slot,
                        tex.SRV);

                    boundVS_SRVs[slot] = tex;
                }
            }

            if ((stages & ShaderStage.Fragment) != 0)
            {
                if (boundPS_SRVs[slot] != tex)
                {
                    deviceContext.PixelShader.SetShaderResource(
                        (int)slot,
                        tex.SRV);

                    boundPS_SRVs[slot] = tex;
                }
            }

            if ((stages & ShaderStage.Compute) != 0)
            {
                if (boundCS_SRVs[slot] != tex)
                {
                    deviceContext.ComputeShader.SetShaderResource(
                        (int)slot,
                        tex.SRV);

                    boundCS_SRVs[slot] = tex;
                }
            }
        }
        internal void BindShaderResourceView(uint slot, ShaderStage stages, DX11ShaderStorageBuffer buffer)
        {
            ResolveForSRV(buffer);

            if ((stages & ShaderStage.Vertex) != 0)
            {
                if (boundVS_SSBOs[slot] != buffer)
                {
                    deviceContext.VertexShader.SetShaderResource(
                        (int)slot,
                        buffer.SRV);

                    boundVS_SSBOs[slot] = buffer;
                }
            }

            if ((stages & ShaderStage.Fragment) != 0)
            {
                if (boundPS_SSBOs[slot] != buffer)
                {
                    deviceContext.PixelShader.SetShaderResource(
                        (int)slot,
                        buffer.SRV);

                    boundPS_SSBOs[slot] = buffer;
                }
            }

            if ((stages & ShaderStage.Compute) != 0)
            {
                if (boundCS_SSBOs[slot] != buffer)
                {
                    deviceContext.ComputeShader.SetShaderResource(
                        (int)slot,
                        buffer.SRV);

                    boundCS_SSBOs[slot] = buffer;
                }
            }
        }
        private void BindUnorderedAccessView(uint slot, ShaderStage stages, DX11Texture tex)
        {
            ResolveForUAV(tex);
            if ((stages & ShaderStage.Compute) != 0)
            {
                deviceContext.ComputeShader.SetUnorderedAccessView(
                        (int)slot,
                        tex.UAV);

                boundCS_UAVTextures[slot] = tex;
            }

        }
        private void BindUnorderedAccessView(uint slot, ShaderStage stages, DX11ShaderStorageBuffer buffer)
        {
            ResolveForUAV(buffer);
            if ((stages & ShaderStage.Compute) != 0)
            {
                deviceContext.ComputeShader.SetUnorderedAccessView(
                        (int)slot,
                        buffer.UAV);

                boundCS_UAVBuffers[slot] = buffer;
            }
        }
        internal void BindSampler(uint slot, ShaderStage stages, SamplerState sampler)
        {
            if ((stages & ShaderStage.Vertex) != 0 &&
                boundVS_Samplers[slot] != sampler)
            {
                deviceContext.VertexShader.SetSampler((int)slot, sampler);
                boundVS_Samplers[slot] = sampler;
            }

            if ((stages & ShaderStage.Fragment) != 0 &&
                boundPS_Samplers[slot] != sampler)
            {
                deviceContext.PixelShader.SetSampler((int)slot, sampler);
                boundPS_Samplers[slot] = sampler;
            }

            if ((stages & ShaderStage.Geometry) != 0 &&
                boundGS_Samplers[slot] != sampler)
            {
                deviceContext.GeometryShader.SetSampler((int)slot, sampler);
                boundGS_Samplers[slot] = sampler;
            }

            if ((stages & ShaderStage.Compute) != 0 &&
                boundCS_Samplers[slot] != sampler)
            {
                deviceContext.ComputeShader.SetSampler((int)slot, sampler);
                boundCS_Samplers[slot] = sampler;
            }
        }
        private void ResolveForSRV(DX11Texture tex)
        {
            bool dirty = false;

            for (int i = 0; i < boundRTVs.Length; i++)
            {
                if (boundRTVs[i] == tex)
                {
                    Console.WriteLine(
                        $"[HAZARD] Unbinding RTV[{i}] " +
                        $"because texture is being bound as SRV");
                    dirty = true;
                }
            }

            if (boundDSV == tex)
            {
                Console.WriteLine(
                    "[HAZARD] Unbinding DSV because texture is being bound as SRV");

                dirty = true;
            }

            for (int i = 0; i < boundCS_UAVTextures.Length; i++)
            {
                if (boundCS_UAVTextures[i] == tex)
                {
                    Console.WriteLine(
                        $"[HAZARD] Unbinding CS UAV[{i}] " +
                        $"because texture is being bound as SRV");

                    deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
                    boundCS_UAVTextures[i] = null;
                }
            }

            if (dirty)
            {
                deviceContext.OutputMerger.SetRenderTargets(
                    null,
                    []);

                Array.Clear(boundRTVs);
                boundDSV = null;

                // CRITICAL:
                // The physical OM state no longer matches currentFramebuffer.
                currentFramebuffer = null;
            }
        }
        private void ResolveForSRV(DX11ShaderStorageBuffer buffer)
        {
            for (int i = 0; i < boundCS_UAVBuffers.Length; i++)
            {
                if (boundCS_UAVBuffers[i] == buffer)
                {
                    deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
                    boundCS_UAVBuffers[i] = null;
                }
            }
        }
        private void ResolveForRTV(DX11Texture tex)
        {
            for (int i = 0; i < boundPS_SRVs.Length; i++)
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
            for (int i = 0; i < boundCS_UAVTextures.Length; i++)
            {
                if (boundCS_UAVTextures[i] == tex)
                {
                    deviceContext.ComputeShader.SetUnorderedAccessView(i, null);
                    boundCS_UAVTextures[i] = null;
                }
            }
        }
        private void ResolveForUAV(DX11Texture tex)
        {
            // Remove from all SRV bindings

            for (int i = 0; i < boundVS_SRVs.Length; i++)
            {
                if (boundVS_SRVs[i] == tex)
                {
                    deviceContext.VertexShader.SetShaderResource(i, null);
                    boundVS_SRVs[i] = null;
                }
            }

            for (int i = 0; i < boundPS_SRVs.Length; i++)
            {
                if (boundPS_SRVs[i] == tex)
                {
                    deviceContext.PixelShader.SetShaderResource(i, null);
                    boundPS_SRVs[i] = null;
                }
            }

            for (int i = 0; i < boundCS_SRVs.Length; i++)
            {
                if (boundCS_SRVs[i] == tex)
                {
                    deviceContext.ComputeShader.SetShaderResource(i, null);
                    boundCS_SRVs[i] = null;
                }
            }

            // Remove from any RTV bindings

            bool updateOM = false;

            for (int i = 0; i < boundRTVs.Length; i++)
            {
                if (boundRTVs[i] == tex)
                {
                    boundRTVs[i] = null;

                    if (currentFramebuffer != null)
                        currentFramebuffer.RTVs[i] = null;

                    updateOM = true;
                }
            }

            // Remove from DSV

            if (boundDSV == tex)
            {
                boundDSV = null;

                if (currentFramebuffer != null)
                    currentFramebuffer.DSV = null;

                updateOM = true;
            }

            if (updateOM && currentFramebuffer != null)
            {
                deviceContext.OutputMerger.SetRenderTargets(
                    currentFramebuffer.DSV,
                    currentFramebuffer.RTVs);
            }
        }
        private void ResolveForUAV(DX11ShaderStorageBuffer buffer)
        {
            for (int i = 0; i < boundVS_SSBOs.Length; i++)
            {
                if (boundVS_SSBOs[i] == buffer)
                {
                    deviceContext.VertexShader.SetShaderResource(i, null);
                    boundVS_SSBOs[i] = null;
                }
            }

            for (int i = 0; i < boundPS_SSBOs.Length; i++)
            {
                if (boundPS_SSBOs[i] == buffer)
                {
                    deviceContext.PixelShader.SetShaderResource(i, null);
                    boundPS_SSBOs[i] = null;
                }
            }

            for (int i = 0; i < boundCS_SSBOs.Length; i++)
            {
                if (boundCS_SSBOs[i] == buffer)
                {
                    deviceContext.ComputeShader.SetShaderResource(i, null);
                    boundCS_SSBOs[i] = null;
                }
            }
        }
    
    }
}
