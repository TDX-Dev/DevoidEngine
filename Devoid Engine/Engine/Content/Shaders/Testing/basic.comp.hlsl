struct GPUPointLight
{
    float4 position; // xyz = pos, w = 1
    float4 color; // rgb = color, a = intensity
    float4 range; // x = radius, y/z/w = falloff params
};

struct GPUSpotLight
{
    float4 position; // xyz = pos, w = 1
    float4 color; // rgb = color, a = intensity
    float4 direction; // xyz = direction, w = radius
    float innerCutoff;
    float outerCutoff;
    float2 _padding; // pad to 16-byte alignment
};

struct GPUDirectionalLight
{
    float4 direction; // xyz = dir, w unused
    float4 color; // rgb = color, a = intensity
};

cbuffer LightCounts : register(b0)
{
    uint numPointLights;
    uint numSpotLights;
    uint numDirectionalLights;
    uint _padding;
};

StructuredBuffer<GPUPointLight> g_PointLights : register(t0);
StructuredBuffer<GPUSpotLight> g_SpotLights : register(t1);
StructuredBuffer<GPUDirectionalLight> g_DirectionalLights : register(t2);

RWTexture2D<float4> OutputTexture : register(u0);

Texture2D MAT_REFLECTION_YAY : register(t3);

[numthreads(8, 8, 1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
    float4 avg = 0.0f;

    uint count = numPointLights;
    for (uint i = 0; i < count; i++)
    {
        avg += g_PointLights[i].color;
    }

    if (count > 0)
        avg /= count;

    // Correct 2D indexing for RWTexture2D
    OutputTexture[DTid.xy] = avg;
}
