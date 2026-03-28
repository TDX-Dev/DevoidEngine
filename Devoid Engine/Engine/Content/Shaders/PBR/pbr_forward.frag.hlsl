struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz + handedness
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 WorldspacePosition : TEXCOORD3;
};

#include "../Common/render_constants.hlsl"
#include "../Common/light_constructs.hlsl"

//cbuffer Material : register(b3)
//{
//    float4 Albedo; // base color multiplier
//    float Metallic;
//    float Roughness;
//    float AO;
//    float3 SpecularColor; // dielectric F0 override (usually 0.04)
    
//    float3 EmissiveColor;
//    float EmissiveStrength;
//    uint useNormalMap;
//    float _padding;
    
//};

cbuffer Material : register(b3)
{
    float4 Albedo; // base color multiplier

    float Metallic; // metallic multiplier
    float Roughness; // roughness multiplier
    float AO; // ambient occlusion multiplier
    float EmissiveStrength; // emissive intensity

    float3 EmissiveColor; // emissive color
    int useNormalMap; // toggle normal map

    float NormalStrength; // normal intensity
    float3 _padding1; // padding for 16-byte alignment
};

TextureCube ENV_Irradiance : register(t6);
TextureCube ENV_Prefilter : register(t7);
Texture2D ENV_BRDF : register(t8);

SamplerState ENV_Irradiance_Sampler : register(s6);
SamplerState ENV_Prefilter_Sampler : register(s7);
SamplerState ENV_BRDF_Sampler : register(s8);

Texture2D MAT_AlbedoMap : register(t0);
Texture2D MAT_NormalMap : register(t1);
Texture2D MAT_MetallicMap : register(t2);
Texture2D MAT_RoughnessMap : register(t3);
Texture2D MAT_AOMap : register(t4);
Texture2D MAT_EmissiveMap : register(t5);

SamplerState MAT_AlbedoSampler : register(s0);
SamplerState MAT_NormalSampler : register(s1);
SamplerState MAT_MetallicSampler : register(s2);
SamplerState MAT_RoughnessSampler : register(s3);
SamplerState MAT_AOSampler : register(s4);
SamplerState MAT_EmissiveSampler : register(s5);

#include "./pbr_methods.hlsl"

float3 GetNormalFromMap(PSInput input)
{
    float3 N = normalize(input.Normal);
    if (useNormalMap == 0)
        return N;

    float3 T = normalize(input.Tangent.xyz);
    T = normalize(T - N * dot(N, T));
    float3 B = cross(N, T) * input.Tangent.w;

    float3 normalTex = MAT_NormalMap.Sample(MAT_NormalSampler, input.UV0).xyz;
    normalTex = normalTex * 2.0 - 1.0;
    normalTex.xy *= NormalStrength;
    normalTex = normalize(normalTex);

    float3x3 TBN = float3x3(T, B, N);

    return normalize(mul(normalTex, TBN));
}



float3 ComputeIBL(
    float3 N,
    float3 V,
    float3 albedo,
    float metallic,
    float roughness,
    float ao,
    float3 F0
)
{
    float NdotV = max(dot(N, V), 0.0);

    // Fresnel (roughness-aware)
    float3 F = FresnelSchlickRoughness(NdotV, F0, roughness);

    float3 kS = F;
    float3 kD = (1.0 - kS) * (1.0 - metallic);

    // --------------------
    // Diffuse IBL
    // --------------------
    float3 irradiance = ENV_Irradiance.Sample(ENV_Irradiance_Sampler, N).rgb;
    float3 diffuse = irradiance * albedo;

    // --------------------
    // Specular IBL
    // --------------------
    float3 R = normalize(reflect(-V, N));

    float MAX_MIP = 4.0; // PREF_MIPS - 1
    float3 prefiltered = ENV_Prefilter.SampleLevel(
        ENV_Prefilter_Sampler,
        R,
        roughness * MAX_MIP
    ).rgb;

    float2 brdf = ENV_BRDF.Sample(
        ENV_BRDF_Sampler,
        float2(NdotV, roughness)
    ).rg;

    float3 specular = prefiltered * (F * brdf.x + brdf.y);

    // --------------------
    // Final ambient
    // --------------------
    return (kD * diffuse + specular) * ao;
}

//float3 GetNormalFromMap(PSInput input)
//{
//    return normalize(input.Normal);
//}

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 uv = input.UV0;
    
    float3 albedoTex = MAT_AlbedoMap.Sample(MAT_AlbedoSampler, uv).rgb;
    //albedoTex = pow(albedoTex, 2.2); // sRGB → linear
    float metallicTex = MAT_MetallicMap.Sample(MAT_RoughnessSampler, uv).b;
    float roughnessTex = MAT_RoughnessMap.Sample(MAT_RoughnessSampler, uv).g;
    float aoTex = MAT_AOMap.Sample(MAT_AOSampler, uv).r;
    float3 emissiveTex = MAT_EmissiveMap.Sample(MAT_EmissiveSampler, uv).rgb;
    
    float3 albedo = albedoTex * Albedo.rgb;
    float metallic = saturate(metallicTex * Metallic);
    float roughness = saturate(roughnessTex * Roughness);
    float ao = aoTex * AO;
    float3 emission = emissiveTex * EmissiveColor * EmissiveStrength;
    
    roughness = max(roughness, 0.04); // avoid zero roughness
    
    float3 N = GetNormalFromMap(input);
    float3 V = normalize(Position - input.WorldspacePosition);
    
    float3 F0 = lerp(float3(0.04, 0.04, 0.04), albedo, metallic);
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
    
    float3 ambient = ComputeIBL(
        N,
        V,
        albedo,
        metallic,
        roughness,
        ao,
        F0
    );

    float3 color = ambient + Lo + emission;
    return float4(color, 1.0);
}
