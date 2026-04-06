cbuffer Shadow : register(b0)
{
    float4x4 LightMVP;
}

struct VSInput
{
    float3 Position : POSITION;
};

struct PSInput
{
    float4 Position : SV_POSITION;
};

PSInput VSMain(VSInput input)
{
    PSInput o;

    o.Position = mul(LightMVP, float4(input.Position, 1));

    return o;
}