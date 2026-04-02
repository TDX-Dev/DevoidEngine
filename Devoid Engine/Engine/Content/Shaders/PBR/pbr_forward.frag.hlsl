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
#include "../Skybox/skybox_constants.hlsl"

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
    float NdotV = max(dot(N, V), 1e-4);

    float3 F = FresnelSchlickRoughness(NdotV, F0, roughness);

    float3 kS = F;
    float3 kD = (1.0 - kS) * (1.0 - metallic);

    // Diffuse IBL
    float3 irradiance = ENV_Irradiance.Sample(ENV_Irradiance_Sampler, N).rgb;
    float3 diffuse = irradiance * albedo;// / PI;

    // Specular IBL
    float3 R = normalize(reflect(-V, N));
    //R = normalize(lerp(R, N, roughness * roughness));
    
    float alpha = max(roughness, 0.045);
    float3 prefiltered = ENV_Prefilter.SampleLevel(
        ENV_Prefilter_Sampler,
        R,
        alpha * MAX_PREFILTER_MIP_LEVEL
    ).rgb;

    float2 brdf = ENV_BRDF.Sample(
        ENV_BRDF_Sampler,
        float2(NdotV, alpha)
    ).rg;
    
    //float2 brdf = float2(1.0, 0.0);

    float3 specular = prefiltered * (F * brdf.x + brdf.y);

    //float energyTerm = max(brdf.x, 0.02);
    //float3 energyCompensation = 1.0 + F * (1.0 / energyTerm - 1.0);
    //specular *= energyCompensation;

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
    float metallicTex = MAT_MetallicMap.Sample(MAT_MetallicSampler, uv).b;
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
    
    for (uint i = 0; i < pointLightCount; i++)
    {
        if (PointLights[i].position.w == 0)
            continue;

        Lo += ComputePointLight(
            PointLights[i],
            input.WorldspacePosition,
            N,
            V,
            albedo,
            metallic,
            roughness,
            F0
        );
    }

    for (uint i = 0; i < spotLightCount; i++)
    {
        if (SpotLights[i].position.w == 0)
            continue;

        Lo += ComputeSpotLight(
            SpotLights[i],
            input.WorldspacePosition,
            N,
            V,
            albedo,
            metallic,
            roughness,
            F0
        );
    }

    for (uint i = 0; i < directionalLightCount; i++)
    {
        if (DirectionalLights[i].Direction.w == 0)
            continue;

        Lo += ComputeDirectionalLight(
            DirectionalLights[i],
            N,
            V,
            albedo,
            metallic,
            roughness,
            F0
        );
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
