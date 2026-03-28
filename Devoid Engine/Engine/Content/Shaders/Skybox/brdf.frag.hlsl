struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float3 BiTangent : BINORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 FragPos : TEXCOORD2;
    float3 WorldPos : TEXCOORD3;
};

#include "../PBR/pbr_methods.hlsl"

// --- Hammersley ---
float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10;
}

float2 Hammersley(uint i, uint N)
{
    return float2(float(i) / float(N), RadicalInverse_VdC(i));
}

// --- GGX sampling ---
float3 ImportanceSampleGGX(float2 Xi, float3 N, float roughness)
{
    float a = roughness * roughness;
    // Sample in spherical coordinates
    float Phi = 2.0 * PI * Xi.x;
    float CosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a * a - 1.0) * Xi.y));
    float SinTheta = sqrt(1.0 - CosTheta * CosTheta);
    // Construct tangent space vector
    float3 H;
    H.x = SinTheta * cos(Phi);
    H.y = SinTheta * sin(Phi);
    H.z = CosTheta;
    
    // Tangent to world space
    float3 UpVector = abs(N.z) < 0.999 ? float3(0., 0., 1.0) : float3(1.0, 0., 0.);
    float3 TangentX = normalize(cross(UpVector, N));
    float3 TangentY = cross(N, TangentX);
    return TangentX * H.x + TangentY * H.y + N * H.z;
}

float2 IntegrateBRDF(float NdotV, float roughness)
{
    float3 V;
    V.x = sqrt(1.0 - NdotV * NdotV);
    V.y = 0.0;
    V.z = NdotV;

    float3 N = float3(0.0, 0.0, 1.0);

    const uint SAMPLE_COUNT = 1024u;

    float A = 0.0;
    float B = 0.0;

    for (uint i = 0u; i < SAMPLE_COUNT; ++i)
    {
        float2 Xi = Hammersley(i, SAMPLE_COUNT);
        float3 H = ImportanceSampleGGX(Xi, N, roughness);
        float3 L = 2.0 * dot(V, H) * H - V;
        
        float NoL = saturate(dot(N, L));
        float NoH = max(dot(N, H), 1e-5);
        float VoH = saturate(dot(V, H));

        if (NoL > 0.0)
        {
            float V_pdf = V_SmithGGXCorrelated(NdotV, NoL, roughness) * VoH * NoL / NoH;

            float Fc = pow(1.0 - VoH, 5.0);

            A += (1.0 - Fc) * V_pdf;
            B += Fc * V_pdf;
        }
    }

    return 4.0 * float2(A, B) / SAMPLE_COUNT;
}

//float2 IntegrateBRDF(float NdotV, float roughness)
//{
//    float3 V;
//    V.x = sqrt(1.0 - NdotV * NdotV);
//    V.y = 0.0;
//    V.z = NdotV;

//    float3 N = float3(0.0, 0.0, 1.0);

//    const uint SAMPLE_COUNT = 1024u;

//    float A = 0.0;
//    float B = 0.0;

//    for (uint i = 0u; i < SAMPLE_COUNT; ++i)
//    {
//        float2 Xi = Hammersley(i, SAMPLE_COUNT);
//        float3 H = ImportanceSampleGGX(Xi, N, roughness);
//        float3 L = normalize(2.0 * dot(V, H) * H - V);

//        float NoL = saturate(L.z);
//        float NoH = saturate(H.z);
//        float VoH = saturate(dot(V, H));

//        if (NoL > 0.0)
//        {
//            float G = GeometrySmith(N, V, L, roughness);

//            float G_Vis = (G * VoH) / max(NoH * NdotV, 1e-5);

//            float Fc = pow(1.0 - VoH, 5.0);

//            A += (1.0 - Fc) * G_Vis;
//            B += Fc * G_Vis;
//        }
//    }

//    return float2(A, B) / SAMPLE_COUNT;
//}

float4 PSMain(PSInput input) : SV_Target
{

    float2 uv = float2(input.UV0.x, 1.0 - input.UV0.y);

    float NdotV = max(uv.x, 1e-4);
    float roughness = uv.y;
    roughness = max(roughness, 0.045);
    
    float2 brdf = IntegrateBRDF(NdotV, roughness);

    return float4(brdf, 0.0, 1.0);
}