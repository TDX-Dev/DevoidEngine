struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

TextureCube MAT_Skybox : register(t0);
SamplerState MAT_Skybox_Sampler : register(s0);

Texture2D BlueNoise : register(t1);


cbuffer Material : register(b5)
{
    float Roughness;
    float MaxPrefilterMipLevel;
    float EnvironmentMapResolution;
    float padding;
}

#include "./Common/MathConstants.hlsl"
#include "./Common/PBRMethods.hlsl"

float4 PSMain(PSInput input) : SV_Target
{
    //float3 dir = normalize(input.WorldspacePosition);
    //dir.y = -dir.y;

    //return float4(normalize(input.WorldspacePosition) * 0.5 + 0.5, 1);
    
    float3 N = normalize(input.WorldspacePosition);
    float3 R = N;
    float3 V = R;
    
    float2 noise =
    BlueNoise.Load(int3( int2(input.Position.xy) & 127, 0)).rg;
    
    const uint SAMPLE_COUNT = 1024u;
    float3 prefilteredColor = float3(0, 0, 0);
    float totalWeight = 0.0;
    
    for (uint i = 0u; i < SAMPLE_COUNT; ++i)
    {
        float2 Xi = Hammersley(i, SAMPLE_COUNT);
        Xi = frac(Xi + noise);
        float3 H = ImportanceSampleGGX(Xi, N, Roughness);
        float3 L = normalize(2.0 * dot(V, H) * H - V);

        float NdotL = saturate(dot(N, L));
        if (NdotL > 0.0)
        {
            float NdotH = saturate(dot(N, H));
            float HdotV = max(dot(H, V), 0.001);

            float D = DistributionGGX(NdotH, Roughness);
            float pdf = max(D * NdotH / (4.0 * HdotV), 1e-6);

            float saTexel = 4.0 * PI / (6.0 * EnvironmentMapResolution * EnvironmentMapResolution);
            float saSample = 1.0 / (float(SAMPLE_COUNT) * pdf + 0.0001);

            float mipLevel = Roughness < 0.001 ? 0.0 : 0.5 + log2(saSample / saTexel);

            mipLevel = clamp(mipLevel, 0.0, MaxPrefilterMipLevel);

            //L.y = -L.y;
            
            float3 color = MAT_Skybox.SampleLevel(
                MAT_Skybox_Sampler,
                L,
                mipLevel
            ).rgb;

            //color = min(color, 25.0);
            
            prefilteredColor += color * NdotL;
            totalWeight += NdotL;
        }
    }

    prefilteredColor /= max(totalWeight, 0.0001);
    
    return float4(prefilteredColor, 1.0);
}