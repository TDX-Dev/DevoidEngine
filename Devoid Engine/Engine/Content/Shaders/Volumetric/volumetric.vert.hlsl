
struct VSInput
{
    float3 Position : POSITION;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 WorldspacePosition : TEXCOORD0;
};

#include "../Common/render_constants.hlsl"


PSInput VSMain(VSInput input)
{
    PSInput o;

    float4 world = mul(Model, float4(input.Position, 1));

    o.WorldspacePosition = world.xyz;
    o.Position = mul(Projection, mul(View, world));

    return o;
}