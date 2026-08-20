struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

Texture2D Albedo_Texture : register(t0);
SamplerState Albedo_Sampler : register(s0);

#include "./Common/RenderConstants.hlsl"

float4 PSMain(PSInput input) : SV_Target0
{
    float3 viewDir = normalize(input.WorldspacePosition - CameraPosition);
    
    float3 sunDir = normalize(float3(0.0, 0.7071, 0.7071));

    
    float3 zenith = float3(0.10, 0.28, 0.85);
    float3 horizon = float3(0.82, 0.88, 1.00);

    float t = saturate(viewDir.y);
    t = pow(t, 0.35);

    float3 color = lerp(horizon, zenith, t);
    
    float horizonGlow = pow(1.0 - saturate(abs(viewDir.y)), 1.0);
    color += horizonGlow * float3(0.18, 0.20, 0.24);
    
    float sunAmount = saturate(dot(viewDir, sunDir));

    color += pow(sunAmount, 8.0) * float3(0.35, 0.20, 0.05);

    color += pow(sunAmount, 64.0) * float3(2.0, 1.8, 1.5);

    color += pow(sunAmount, 512.0) * float3(8.0, 7.5, 6.5);

    float disc = smoothstep(0.99998, 0.999999, sunAmount);

    color += disc * float3(40.0, 38.0, 30.0);
    float groundFade = smoothstep(-0.02, -0.35, viewDir.y);
    float3 ground = float3(0.22, 0.25, 0.32);

    color = lerp(color, ground, groundFade);
    
    return float4(color.x, color.y, color.z, 1);
}