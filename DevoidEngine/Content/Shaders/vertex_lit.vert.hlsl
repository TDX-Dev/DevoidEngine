struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 NormalViewSpace : TEXCOORD2;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
    float3 ViewspacePosition : TEXCOORD3;
};

#include "./Common/RenderConstants.hlsl"

PSInput VSMain(VSInput input)
{
    PSInput output;
    float4 worldPos = mul(Model, float4(input.Position, 1.0));
    float4 viewPos = mul(View, worldPos);
    output.Position = mul(Projection, viewPos);
    output.WorldspacePosition = worldPos.xyz;
    output.ViewspacePosition = viewPos.xyz;
    output.UV = input.UV;
    
    float3x3 normalMatrix = transpose((float3x3) invModel);
    float3 N = mul(normalMatrix, input.Normal);
    float3 T = normalize(mul(normalMatrix, input.Tangent.xyz));
    
    output.Normal = N;
    output.Tangent = float4(T, input.Tangent.w);
    
    N = normalize(
        mul((float3x3) View, N)
    );
    
    output.NormalViewSpace = N;
    
    return output;
}