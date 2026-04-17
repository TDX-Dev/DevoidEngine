Texture2D ParticleTexture : register(t0);
SamplerState ParticleTextureSampler : register(s0);

struct PSInput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
};

float4 PSMain(PSInput input) : SV_Target
{
    float4 tex = ParticleTexture.Sample(ParticleTextureSampler, input.uv);
    return tex * input.color;
}