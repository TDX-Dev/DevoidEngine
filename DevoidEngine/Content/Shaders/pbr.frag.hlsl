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

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 uv = input.UV;
    
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
    // Position is camera position!
    float3 V = normalize(CameraPosition - input.WorldspacePosition);
    
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

        float3 lightContribution = ComputeSpotLight(
            SpotLights[i],
            input.WorldspacePosition,
            N,
            V,
            albedo,
            metallic,
            roughness,
            F0
        );
        
        float shadow = 0;

        if (SpotLights[i].shadowIndex != -1)
        {
            
            float3 toLight = SpotLights[i].position.xyz - input.WorldspacePosition;
            float3 L = normalize(toLight);

            //shadow = ComputeShadow(
            //    SpotLights[i].shadowIndex,
            //    input.WorldspacePosition,
            //    N,
            //    L
            //);
            
            shadow = ComputeShadow(
                SpotLights[i].shadowIndex,
                input.WorldspacePosition
            );
        }

        Lo += (1 - shadow) * lightContribution;
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
    
    //float3 ambient = ComputeIBL(
    //    N,
    //    V,
    //    albedo,
    //    metallic,
    //    roughness,
    //    ao,
    //    F0
    //);
    
    float3 ambient = 0.05 * albedo;// * ao;
    float3 color = ambient + Lo;// + emission;
    
    //return float4(N * 0.5 + 0.5 + (color * 0.00001), 1.0);
    return float4(color, 1);
    //return float4(color, 1.0);
}