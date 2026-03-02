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
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 FragmentPosition : TEXCOORD2;
    float3 WorldspacePosition : TEXCOORD3;
};


#include "../Common/render_constants.hlsl"


PSInput VSMain(VSInput input)
{
    PSInput output = (PSInput) 0;

    float4 worldPos = mul(Model, float4(input.Position, 1.0f));
    float4 viewPos = mul(View, worldPos);

    output.Position = mul(Projection, viewPos);
    output.WorldspacePosition = worldPos.xyz;

    // Proper normal matrix (important if non-uniform scaling exists)
    float3x3 normalMatrix = (float3x3) transpose(invModel);
    float3x3 model3x3 = (float3x3) Model;

    float3 N = normalize(mul(normalMatrix, input.Normal));
    float3 T = normalize(mul(model3x3, input.Tangent));
    float3 B = normalize(mul(model3x3, input.BiTangent));

    float handedness = (dot(cross(N, T), B) < 0.0f) ? -1.0f : 1.0f;
    
    // Generate handedness
    //float handedness = (dot(cross(N, T), B) < 0.0f) ? -1.0f : 1.0f;

    output.Normal = N;
    output.Tangent.xyz = T;
    output.Tangent.w = handedness;

    output.UV0 = input.UV0;
    output.UV1 = input.UV1;

    return output;
}
