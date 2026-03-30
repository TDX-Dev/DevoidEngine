struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 Tangent : TANGENT;
    float3 BiTangent : BINORMAL;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 NDC : TEXCOORD4;
    float2 UV0 : TEXCOORD0;
    float2 LocalPos : TEXCOORD1;
};

#include "../Common/render_constants.hlsl"

cbuffer MATERIAL : register(b3)
{
    float4 COLOR;
    float4 CORNER_RADIUS;
    float4 BORDER_COLOR;

    float2 RECT_SIZE;
    float BORDER_THICKNESS;
    float DASH_SIZE;

    float GAP_SIZE;
    int DEBUG_DASHED;
    int useTexture;
    float _pad;
};

PSInput VSMain(VSInput input)
{
    PSInput output;

    float4 local = float4(input.Position.xy, 0.0, 1.0);

    float4 world = mul(Model, local);
    float4 view = mul(View, world);
    float4 clip = mul(Projection, view);

    output.Position = clip;
    output.NDC = clip.xy / clip.w;
    output.UV0 = input.UV0;

    output.LocalPos = input.UV0 * RECT_SIZE - RECT_SIZE * 0.5;

    return output;
}