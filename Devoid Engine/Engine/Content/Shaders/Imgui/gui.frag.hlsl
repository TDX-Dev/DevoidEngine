Texture2D in_fontTexture : register(t0);
SamplerState fontSampler : register(s0);

struct PSInput
{
    float4 Position : SV_Position; // needed by rasterizer
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};


float4 PSMain(PSInput input) : SV_Target
{
    return input.Color * in_fontTexture.Sample(fontSampler, input.TexCoord);
}
