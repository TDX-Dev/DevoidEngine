struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

#include "./Common/RenderConstants.hlsl"
#include "./Common/MathConstants.hlsl"

Texture2D<float> DepthTexture : register(t0);
Texture2D<float4> NormalTexture : register(t1);
Texture2D<float2> BlueNoiseTexture : register(t2);

SamplerState PointSampler
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

#define DirectionCount 1
#define SampleCount    8
#define SectorCount    32

static const float VBAOThickness = 0.25;
static const float VBAORadius = 1;
static const float RadiusSq = VBAORadius * VBAORadius;

#define ACOS_QUALITY_MODE 1

float ACosPoly(float x)
{
#if ACOS_QUALITY_MODE == 1
    return 1.5707963267948966
         - 0.1565827644218014 * x;
#else
    return 1.5707963267948966
         + (-0.20491203466059038
         + 0.04832927023878897 * x) * x;
#endif
}

float ACos01_Approx(float x)
{
    x = saturate(x);
    return ACosPoly(x) * sqrt(1.0 - x);
}

float ACos_Approx(float x)
{
    x = clamp(x, -1.0, 1.0);

    float ax = abs(x);
    float u = ACosPoly(ax) * sqrt(1.0 - ax);

    return x >= 0.0 ? u : PI - u;
}

float3 ReconstructViewPosition(float2 uv, float depth)
{
    float4 ndc;
    ndc.x = uv.x * 2.0 - 1.0;
    ndc.y = 1.0 - uv.y * 2.0;
    ndc.z = depth;
    ndc.w = 1.0;

    float4 view = mul(InverseProjection, ndc);
    return view.xyz / view.w;
}

float2 ViewToUV(float3 positionVS)
{
    float4 clip = mul(Projection, float4(positionVS, 1.0));
    float2 ndc = clip.xy / clip.w;
    return float2(
        ndc.x * 0.5 + 0.5,
        0.5 - ndc.y * 0.5
    );
}

uint CountBits(uint v)
{
    v = v - ((v >> 1u) & 0x55555555u);
    v = (v & 0x33333333u) + ((v >> 2u) & 0x33333333u);
    return ((v + (v >> 4u)) & 0x0F0F0F0Fu) * 0x01010101u >> 24u;
}

float2 SampleBlueNoise(uint2 pixel, uint frame)
{
    uint2 noisePixel = pixel & 63;

    float2 noise =
        BlueNoiseTexture.Load(uint3(noisePixel, 0)).rg;

    float frameOffset =
        frac((float) frame * 0.61803398875);

    noise.x = frac(noise.x + frameOffset);
    noise.y = frac(noise.y + frameOffset * 0.75487766);

    return noise;
}

uint AddOcclusionInterval(
    float minHorizon,
    float maxHorizon,
    uint globalOccludedBitfield)
{
    minHorizon = saturate(minHorizon);
    maxHorizon = saturate(maxHorizon);

    if (maxHorizon <= minHorizon)
        return globalOccludedBitfield;

    uint startHorizonInt =
        (uint) (minHorizon * SectorCount);

    uint angleHorizonInt =
        (uint) ceil(
            (maxHorizon - minHorizon) *
            SectorCount
        );

    if (angleHorizonInt == 0)
        return globalOccludedBitfield;

    uint angleHorizonBitfield =
        angleHorizonInt >= SectorCount
            ? 0xFFFFFFFFu
            : (0xFFFFFFFFu >>
               (SectorCount - angleHorizonInt));

    uint currentOccludedBitfield =
        angleHorizonBitfield <<
        startHorizonInt;

    return globalOccludedBitfield |
           currentOccludedBitfield;
}

float3 GetSliceViewDirection(float2 screenDir, float3 V)
{
    float3 up = (abs(V.y) < 0.99) ? float3(0, 1, 0) : float3(1, 0, 0);
    float3 right = normalize(cross(up, V));
    float3 tangentUp = normalize(cross(V, right));

    return normalize(right * screenDir.x + tangentUp * screenDir.y);
}

float ComputeVBAO(
    float3 positionVS,
    float3 normalVS,
    uint2 pixel
)
{
    
    float2 halfScreen = ScreenSize / 2;
    // View vector pointing from pixel towards camera
    float3 V = normalize(-positionVS);

    float2 uv0 = ViewToUV(positionVS);
    float projectedRadius = (0.5 * Projection[0][0] * halfScreen.x * VBAORadius) / max(abs(positionVS.z), 0.001);
    float rayStep = projectedRadius / (SampleCount + 1.0);

    if (projectedRadius < 1.0)
        return 1.0;

    float totalAO = 0.0;
    
    uint2 fullResPixel = pixel * 2;
    float2 noise = SampleBlueNoise(fullResPixel, FrameIndex);
   

    // Loop over multiple slice directions
    [unroll]
    for (uint d = 0; d < DirectionCount; ++d)
    {
        // 1. Generate stratified 2D screen direction for slice d
        float angle = (noise.x + (float) d) * (PI / (float) DirectionCount);
        float2 rayDir = float2(cos(angle), sin(angle));

        float3 sliceDir = float3(rayDir, 0.0);

        float3 sliceN = normalize(cross(sliceDir, V));

        float3 projN = normalVS - sliceN * dot(normalVS, sliceN);

        float projNSqrLen = dot(projN, projN);

        if (projNSqrLen < 1e-6)
        {
            totalAO += 1.0;
            continue;
        }

        float3 projNNormalized =
    projN * rsqrt(projNSqrLen);

        float cosN = clamp(dot(projNNormalized, V), -1.0, 1.0);

        float3 T =
    cross(V, sliceN);

        float N_angle = -sign(dot(projN, T)) * ACos_Approx(cosN);

        float rayOffset = frac(noise.y + (float) d * 0.6180339887498948482);
        
        uint globalOccludedBitfield = 0u;

        // 4. Trace along positive (+rayDir) and negative (-rayDir) directions
        for (int side = -1; side <= 1; side += 2)
        {
            float samplingDirection = (float) side;
            float t = (rayOffset + 1.0) * rayStep;

            for (uint i = 0; i < SampleCount; ++i)
            {

                float2 sampleOffset = rayDir * samplingDirection * t;

                float2 samplePixel = uv0 * halfScreen + sampleOffset;

                if (samplePixel.x < 0.0 || samplePixel.y < 0.0 ||
                    samplePixel.x >= halfScreen.x || samplePixel.y >= halfScreen.y)
                {
                    break;
                }

                float2 sampleUV = samplePixel / halfScreen;
                float sampleDepth = DepthTexture.SampleLevel(PointSampler, sampleUV, 0);

                if (sampleDepth >= 0.999999)
                    continue;

                float3 samplePosVS = ReconstructViewPosition(sampleUV, sampleDepth);
                float3 deltaPos = samplePosVS - positionVS;

                float deltaLenSq = dot(deltaPos, deltaPos);

                if (deltaLenSq > RadiusSq)
                    continue;

                float frontCos =
    dot(deltaPos, V) *
    rsqrt(max(deltaLenSq, 1e-8));

                float3 backDelta =
    deltaPos - V * VBAOThickness;

                float backLenSq =
    dot(backDelta, backDelta);

                float backCos =
    dot(backDelta, V) *
    rsqrt(max(backLenSq, 1e-8));

                float frontAngle =
    ACos01_Approx(saturate(frontCos));

                float backAngle =
    ACos01_Approx(saturate(backCos));

                float2 frontBackHorizon =
    float2(frontAngle, backAngle);

// Shift from V to projected normal.
// Map [-PI/2, +PI/2] to [0, 1].
                frontBackHorizon =
    saturate(
        ((samplingDirection * -frontBackHorizon)
        - N_angle
        + HALF_PI) / PI
    );

// Sampling direction reverses min/max.
                if (samplingDirection >= 0.0)
                {
                    frontBackHorizon = frontBackHorizon.yx;
                }

                globalOccludedBitfield =
    AddOcclusionInterval(
        frontBackHorizon.x,
        frontBackHorizon.y,
        globalOccludedBitfield
    );
                t += rayStep;
            }
        }

        uint occludedBits = CountBits(globalOccludedBitfield);
        float sliceAO = 1.0 - ((float) occludedBits / (float) SectorCount);
        totalAO += sliceAO;
    }

    return totalAO / (float) DirectionCount;
}

float PSMain(PSInput input) : SV_TARGET
{
    float depth = DepthTexture.SampleLevel(PointSampler, input.UV, 0);

    if (depth >= 0.999999)
        return float4(1, 1, 1, 1);

    float3 positionVS = ReconstructViewPosition(input.UV, depth);

    // Unpack normal if your G-Buffer stores normals in [0, 1] UNORM format
    float3 normalSample = NormalTexture.SampleLevel(PointSampler, input.UV, 0).xyz;
    float3 normalVS = normalize(normalSample);
    
    float3 biasedPositionVS = positionVS + normalVS * 0.002;

    uint2 pixel = uint2(input.Position.xy);

    float ao = ComputeVBAO(biasedPositionVS, normalVS, pixel);

    return ao;
}