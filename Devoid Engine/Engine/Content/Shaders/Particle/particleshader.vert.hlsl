struct VSInput
{
    float2 quadPos : POSITION;
    float2 uv : TEXCOORD;

    float3 instancePos : INSTANCE_POS;
    float instanceSize : INSTANCE_SIZE;
    float4 instanceCol : INSTANCE_COLOR;
};

struct VSOutput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR0;
};

#include "../Common/render_constants.hlsl"

VSOutput VSMain(VSInput input)
{
    VSOutput output;

    // Build a quad around the particle position in world space
    float3 worldPos = input.instancePos + float3(
        input.quadPos.x * input.instanceSize,
        input.quadPos.y * input.instanceSize,
        0.0
    );

    float4 world = float4(worldPos, 1.0);

    // Standard transform
    output.position = mul(Projection, mul(View, world));

    output.uv = input.uv;
    output.color = input.instanceCol;

    return output;
}