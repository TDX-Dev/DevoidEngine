TextureCube<float4> Sky : register(t0);

RWTexture2DArray<float4> DebugOutput : register(u0);

[numthreads(8, 8, 1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
    uint2 pixel = DTid.xy;
    uint face = DTid.z;

    // Fill with something obvious
    DebugOutput[uint3(pixel, face)] = float4(1, 0, 0, 1);
}