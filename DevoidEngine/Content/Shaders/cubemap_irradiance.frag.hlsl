struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

TextureCube MAT_Skybox : register(t0);
SamplerState MAT_SkyboxSampler : register(s0);

#include "./Common/RenderConstants.hlsl"
#include "./Common/MathConstants.hlsl"

static const uint SAMPLE_COUNT = 512;

float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16) | (bits >> 16);
    bits = ((bits & 0x55555555u) << 1) | ((bits & 0xAAAAAAAAu) >> 1);
    bits = ((bits & 0x33333333u) << 2) | ((bits & 0xCCCCCCCCu) >> 2);
    bits = ((bits & 0x0F0F0F0Fu) << 4) | ((bits & 0xF0F0F0F0u) >> 4);
    bits = ((bits & 0x00FF00FFu) << 8) | ((bits & 0xFF00FF00u) >> 8);

    return float(bits) * 2.3283064365386963e-10;
}

float2 Hammersley(uint i, uint N)
{
    return float2(float(i) / float(N), RadicalInverse_VdC(i));
}

float3 UniformSampleHemisphere(float2 Xi)
{
    float phi = 2.0 * PI * Xi.x;

    float cosTheta = Xi.y;
    float sinTheta = sqrt(1.0 - cosTheta * cosTheta);

    return float3(
        cos(phi) * sinTheta,
        sin(phi) * sinTheta,
        cosTheta
    );
}

float4 PSMain(PSInput input) : SV_Target
{
    float3 N = normalize(input.WorldspacePosition);

    // tangent space basis (build hemisphere around normal)
    float3 up = abs(N.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
    float3 right = normalize(cross(up, N));
    float3 forward = cross(N, right);
    
    float3 irradiance = 0;
    
        for (uint i = 0; i < SAMPLE_COUNT; i++)
        {
            float2 Xi = Hammersley(i, SAMPLE_COUNT);

            float3 tangentSample = UniformSampleHemisphere(Xi);

            float3 sampleVec =
              tangentSample.x * right
            + tangentSample.y * forward
            + tangentSample.z * N;

            sampleVec.y = -sampleVec.y;

            float NdotL = tangentSample.z;

                irradiance +=
            MAT_Skybox.SampleLevel(
                MAT_SkyboxSampler,
                sampleVec,
                3
            ).rgb * NdotL;
    }

    irradiance *= (2.0 * PI) / SAMPLE_COUNT;
    return float4(irradiance, 1.0);
}