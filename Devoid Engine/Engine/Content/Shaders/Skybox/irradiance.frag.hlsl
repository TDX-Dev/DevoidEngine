
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

    irradiance = PI * irradiance / sampleCount;
    return float4(irradiance, 1.0);
}