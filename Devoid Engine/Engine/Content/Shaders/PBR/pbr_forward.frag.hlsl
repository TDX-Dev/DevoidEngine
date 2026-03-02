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
#include "../Common/light_constructs.hlsl"

cbuffer Material : register(b3)
{
    float4 Albedo; // base color multiplier
    float Metallic;
    float Roughness;
    float AO;
    float3 SpecularColor; // dielectric F0 override (usually 0.04)
};

Texture2D MAT_AlbedoMap : register(t0);
Texture2D MAT_NormalMap : register(t1);
Texture2D MAT_MetallicMap : register(t2);
Texture2D MAT_RoughnessMap : register(t3);
Texture2D MAT_AOMap : register(t4);
Texture2D MAT_SpecularMap : register(t5);

SamplerState MAT_AlbedoSampler : register(s0);
SamplerState MAT_NormalSampler : register(s1);
SamplerState MAT_MetallicSampler : register(s2);
SamplerState MAT_RoughnessSampler : register(s3);
SamplerState MAT_AOSampler : register(s4);
SamplerState MAT_SpecularSampler : register(s5);

#include "./pbr_methods.hlsl"

float3 GetNormalFromMap(PSInput input)
{
    float3 N = normalize(input.Normal);

    float3 T = normalize(input.Tangent.xyz);
    float3 B = normalize(cross(N, T) * input.Tangent.w);

    float3 normalTex = MAT_NormalMap.Sample(MAT_NormalSampler, input.UV0).xyz;
    normalTex = normalTex * 2.0 - 1.0;

    float3x3 TBN = float3x3(T, B, N);

    return normalize(mul(normalTex, TBN));
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 uv = input.UV0;
    
    float3 albedoTex = MAT_AlbedoMap.Sample(MAT_AlbedoSampler, uv).rgb;
    float metallicTex = MAT_MetallicMap.Sample(MAT_MetallicSampler, uv).r;
    float roughnessTex = MAT_RoughnessMap.Sample(MAT_RoughnessSampler, uv).r;
    float aoTex = MAT_AOMap.Sample(MAT_AOSampler, uv).r;
    float3 specTex = MAT_SpecularMap.Sample(MAT_SpecularSampler, uv).rgb;
    
    float3 albedo = albedoTex * Albedo.rgb;
    float metallic = saturate(metallicTex * Metallic);
    float roughness = saturate(roughnessTex * Roughness);
    float ao = aoTex * AO;
    float3 specColor = specTex * SpecularColor;
    
    roughness = max(roughness, 0.04); // avoid zero roughness
    
    float3 N = GetNormalFromMap(input);
    float3 V = normalize(Position - input.WorldspacePosition);
    
    float3 F0 = lerp(specColor, albedo, metallic);
    float3 Lo = 0;
    
    for (uint p = 0; p < pointLightCount; ++p)
    {
        if (PointLights[p].position.w == 0)
            continue;

        float3 lightPos = PointLights[p].position.xyz;
        float3 toLight = lightPos - input.WorldspacePosition;
        float lightDistance = length(toLight);

        float attenuation = ComputeAttenuation(PointLights[p], lightDistance);
        if (attenuation <= 0.0)
            continue;

        float3 L = normalize(toLight);
        float3 H = normalize(V + L);

        float3 lightColor = PointLights[p].color.rgb * PointLights[p].color.w;
        float3 radiance = lightColor * attenuation;

        float NDF = DistributionGGX(N, H, roughness);
        float G = GeometrySmith(N, V, L, roughness);
        float3 F = FresnelSchlick(max(dot(H, V), 0.0), F0);

        float3 numerator = NDF * G * F;
        float denom = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
        float3 specular = numerator / denom;

        float3 kS = F;
        float3 kD = (1.0 - kS) * (1.0 - metallic);

        float NdotL = max(dot(N, L), 0.0);

        Lo += (kD * albedo / PI + specular) * radiance * NdotL;
    }
    
    float3 ambient = 0.03 * albedo * ao;

    float3 color = ambient + Lo;

    return float4(input.Tangent.xyz * 0.5 + 0.5, 1);
    //return float4(N, 1.0);
    
    //return float4(normalize(input.Normal) * 0.5 + 0.5, 1);
    //return float4(color, 1.0);
}
