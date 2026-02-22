struct PSInput
{
    float4 Position : SV_POSITION;
    float2 NDC : TEXCOORD4;
    float2 UV0 : TEXCOORD0;
};


cbuffer MeshData : register(b1)
{
    float4x4 Model;
    float4x4 invModel;
};

Texture2D MAT_fontSDFAtlas : register(t0);
SamplerState MAT_fontSDFAtlasSampler : register(s0);

float4 PSMain(PSInput input) : SV_TARGET
{
    float distance = MAT_fontSDFAtlas.Sample(MAT_fontSDFAtlasSampler, input.UV0).r;
    float smoothing = fwidth(distance);
    float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, distance);
    
    if (alpha <= 0.01)
        discard;
    return float4(float3(1,1,1), alpha);

}
