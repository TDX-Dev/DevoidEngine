
struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz + handedness
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 WorldspacePosition : TEXCOORD3;
};

TextureCube MAT_ENVIRONMENT_MAP : register(t0);
SamplerState MAT_ENVIRONMENT_MAP_SAMPLER : register(s0);

static const float PI = 3.14159265359;

float4 PSMain(PSInput input) : SV_Target
{
    float3 N = normalize(input.WorldspacePosition);

    // tangent space basis (build hemisphere around normal)
    float3 up = abs(N.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
    float3 right = normalize(cross(up, N));
    float3 forward = cross(N, right);

    //return float4(right, 1);
    
    float sampleDelta = 0.025;
    float3 irradiance = float3(0, 0, 0);
    float sampleCount = 0.0;

    [loop]
    for (float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
    {
        [loop]
        for (float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
        {
            // spherical to cartesian (tangent space)
            float3 tangentSample = float3(
                sin(theta) * cos(phi),
                sin(theta) * sin(phi),
                cos(theta)
            );

            // transform to world space
            float3 sampleVec =
                tangentSample.x * right +
                tangentSample.y * forward +
                tangentSample.z * N;
            
            sampleVec = normalize(sampleVec);
            sampleVec.y = -sampleVec.y;

            float3 color = MAT_ENVIRONMENT_MAP.Sample(MAT_ENVIRONMENT_MAP_SAMPLER, sampleVec).rgb;
            
            if (!all(isfinite(color)))
                color = 0;
            
            irradiance += color * cos(theta) * sin(theta);
            sampleCount++;
        }
    }

    irradiance = irradiance / sampleCount * (2.0 * PI);
    return float4(irradiance, 1.0);
}

//static const float PI = 3.14159265359;

//float RadicalInverse_VdC(uint bits)
//{
//    bits = (bits << 16u) | (bits >> 16u);
//    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
//    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
//    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
//    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);

//    return float(bits) * 2.3283064365386963e-10;
//}

//float2 Hammersley(uint i, uint N)
//{
//    return float2((float) i / (float) N, RadicalInverse_VdC(i));
//}


//float3 ImportanceSampleCosine(float2 Xi, float3 N)
//{
//    float phi = 2.0 * PI * Xi.x;

//    float cosTheta = sqrt(1.0 - Xi.y);
//    float sinTheta = sqrt(Xi.y);

//    float3 H;
//    H.x = cos(phi) * sinTheta;
//    H.y = sin(phi) * sinTheta;
//    H.z = cosTheta;

//    float3 up = abs(N.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
//    float3 tangent = normalize(cross(up, N));
//    float3 bitangent = cross(N, tangent);

//    float3 sampleVec =
//        tangent * H.x +
//        bitangent * H.y +
//        N * H.z;
    
//    sampleVec.y = 1 - sampleVec.y;

//    return normalize(sampleVec);
//}

//float4 PSMain(PSInput input) : SV_Target
//{
//    float3 N = normalize(input.WorldspacePosition);

//    const uint SAMPLE_COUNT = 64;

//    float3 irradiance = float3(0, 0, 0);

//    [loop]
//    for (uint i = 0; i < SAMPLE_COUNT; ++i)
//    {
//        float2 Xi = Hammersley(i, SAMPLE_COUNT);

//        float3 L = ImportanceSampleCosine(Xi, N);

//        float NdotL = saturate(dot(N, L));

//        float3 radiance =
//            MAT_ENVIRONMENT_MAP.Sample(
//                MAT_ENVIRONMENT_MAP_SAMPLER,
//                L
//            ).rgb;

//        irradiance += radiance * NdotL;
//    }

//    irradiance = irradiance * (PI / SAMPLE_COUNT);

//    return float4(irradiance, 1.0);
//}