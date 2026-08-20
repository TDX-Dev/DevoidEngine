struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

#include "./Common/RenderConstants.hlsl"
#include "./Common/LightConstructs.hlsl"

cbuffer Material : register(b5)
{
    float4 Albedo; // base color multiplier

    float Metallic; // metallic multiplier
    float Roughness; // roughness multiplier
    float AO; // ambient occlusion multiplier
    float EmissiveStrength; // emissive intensity

    float3 EmissiveColor; // emissive color
    int useNormalMap; // toggle normal map

    float NormalStrength; // normal intensity
    float Clearcoat;
    float ClearcoatRoughness;
    float3 ClearcoatTint;
    float Alpha;
};

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

float3 GetNormalFromMap(PSInput input)
{
    float3 N = normalize(input.Normal);
    if (useNormalMap == 0)
        return N;

    float3 T = normalize(input.Tangent.xyz);
    T = normalize(T - N * dot(N, T));
    float3 B = cross(N, T) * input.Tangent.w;

    float3 normalTex = MAT_NormalMap.Sample(MAT_NormalSampler, input.UV).xyz;
    normalTex = normalTex * 2.0 - 1.0;
    normalTex.xy *= NormalStrength;
    normalTex = normalize(normalTex);

    float3x3 TBN = float3x3(T, B, N);

    return normalize(mul(normalTex, TBN));
}

float3 ComputeIBL(
    float3 N,
    float3 Ng,
    float3 V,
    float3 albedo,
    float metallic,
    float roughness,
    float3 F0,
    float clearcoat,
    float clearcoatRoughness
)
{
    float NoV = saturate(dot(N, V));

    // Keep coordinate system Y-flip
    float3 R = reflect(-V, N);
    //R.y = -R.y;
    
    // Base Layer (Specular & Diffuse)
    // Always use SampleLevel in IBL shaders to avoid gradient calculation errors
    float2 dfg = BRDFLUT.SampleLevel(EnvironmentSampler, float2(NoV, roughness), 0.0).rg;
    
    // Filament Multi-Scattering Math
    // dfg.x = A (Scale for 1 - F0)
    // dfg.y = B (Scale for F0)
    
    // Single scattering directional albedo (Fss)
    //float3 Fss = dfg.x + dfg.y * F0;
    float3 Fss = lerp(dfg.x.xxx, dfg.y.xxx, F0);
    
    // Total directional albedo (E) - what a perfectly white material would reflect
    float E_white = dfg.x + dfg.y;
    
    // Multiscattering energy compensation
    float3 energyCompensation = 1.0 + F0 * (1.0 / max(E_white, 1e-5) - 1.0);
    
    float3 totalSpecularEnergy = Fss * energyCompensation;
    
    // Evaluate Specular IBL
    float3 prefiltered = PrefilterMap.SampleLevel(EnvironmentSampler, R, roughness * 8.0).rgb;
    float3 specular = prefiltered * totalSpecularEnergy;

    // Evaluate Diffuse IBL
    // Energy Conservation: Light that didn't reflect as specular (E) enters the material.
    // Metals have 0 diffuse. This is much more accurate than the old FresnelSchlick hack.
    float3 kD = (1.0 - saturate(totalSpecularEnergy)) * (1.0 - metallic);
    float3 diffuse = EvaluateIrradianceSH(EnvironmentSH, N) * albedo * kD;
    
    float3 result = diffuse + specular;
    
    // Clearcoat Layer
    float NoVc = saturate(dot(Ng, V));
    
    float3 Rc = reflect(-V, Ng);

    float3 coatPrefilter = PrefilterMap.SampleLevel(
        EnvironmentSampler,
        Rc,
        clearcoatRoughness * 8.0
    ).rgb;

    float2 coatDFG = BRDFLUT.SampleLevel(
        EnvironmentSampler,
        float2(NoVc, clearcoatRoughness),
        0.0
    ).rg;

    // Clearcoat IOR is typically 1.5, which gives an F0 of 0.04
    float coatFss = coatDFG.x + coatDFG.y * 0.04;

    // Fresnel for the clearcoat layer's absorption mask
    float Fc = FresnelSchlick(NoVc, float3(0.04, 0.04, 0.04)).r;

    // The clearcoat absorbs energy from the base layer before reflecting its own light
    result *= (1.0 - clearcoat * Fc);
    result += coatPrefilter * coatFss * clearcoat;
    
    //return result;
    return result;

}

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 uv = input.UV;
    
    float3 albedoTex = MAT_AlbedoMap.Sample(MAT_AlbedoSampler, uv).rgb;
    float metallicTex = MAT_MetallicMap.Sample(MAT_MetallicSampler, uv).b;
    float roughnessTex = MAT_RoughnessMap.Sample(MAT_RoughnessSampler, uv).g;
    float aoTex = MAT_AOMap.Sample(MAT_AOSampler, uv).r;
    float3 emissiveTex = MAT_EmissiveMap.Sample(MAT_EmissiveSampler, uv).rgb;
    
    float2 screenUV = input.Position.xy * (1 / ScreenSize);
    float screenAO = ScreenAO.SampleLevel(
        ScreenAOSampler,
        screenUV,
        0
    ).r;
    
    
    float3 albedo = albedoTex * Albedo.rgb;
    float metallic = saturate(metallicTex * Metallic);
    float roughness = saturate(roughnessTex * Roughness);
    float ao = screenAO * AO;
    float3 emission = emissiveTex * EmissiveColor * EmissiveStrength;
    
    float3 N = GetNormalFromMap(input);
    float3 Ng = normalize(input.Normal);
    float3 V = normalize(CameraPosition - input.WorldspacePosition);
    
    float3 F0 = lerp(float3(0.04, 0.04, 0.04), albedo, metallic);
    float3 Lo = 0;
    
    for (uint iPoint = 0; iPoint < pointLightCount; iPoint++)
    {
        if (PointLights[iPoint].position.w == 0)
            continue;

        Lo += ComputePointLight(
            PointLights[iPoint],
            input.WorldspacePosition,
            N,
            V,
            albedo,
            metallic,
            roughness,
            F0,
            Clearcoat,
            ClearcoatRoughness
        );
    }

    for (uint iSpot = 0; iSpot < spotLightCount; iSpot++)
    {
        if (SpotLights[iSpot].position.w == 0)
            continue;

        float3 lightContribution = ComputeSpotLight(
            SpotLights[iSpot],
            input.WorldspacePosition,
            N,
            V,
            albedo,
            metallic,
            roughness,
            F0,
            Clearcoat,
            ClearcoatRoughness
        );
        
        float shadow = 0;

        if (SpotLights[iSpot].shadowIndex != -1)
        {
            
            float3 toLight = SpotLights[iSpot].position.xyz - input.WorldspacePosition;
            float3 L = normalize(toLight);
            
            shadow = ComputeShadow(
                SpotLights[iSpot].shadowIndex,
                input.WorldspacePosition
            );
        }

        Lo += (1 - shadow) * lightContribution;
    }

    for (uint iDir = 0; iDir < directionalLightCount; iDir++)
    {
        if (DirectionalLights[iDir].Direction.w == 0)
            continue;

        Lo += ComputeDirectionalLight(
            DirectionalLights[iDir],
            N,
            V,
            albedo,
            metallic,
            roughness,
            F0,
            Clearcoat,
            ClearcoatRoughness
        );
    }
    
    
    float3 ambient = ComputeIBL(
        N,
        Ng,
        V,
        albedo,
        metallic,
        roughness,
        F0,
        Clearcoat,
        ClearcoatRoughness
    );

    float3 color = ambient * (ao) + (Lo + emission);
    return float4(color, 1);
}