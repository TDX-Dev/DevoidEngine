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

#define SliceCount 3
#define StepsPerSlice 3

static const float GTAORadius = 1.0;
static const float GTAOFalloffRange = 0.5;
static const float GTAOSampleDistributionPower = 2.0;
static const float GTAOThinOccluderCompensation = 0.0;

static const float PixelTooCloseThreshold = 1.3;

static const float RadiusSq = GTAORadius * GTAORadius;

float ACosPoly(float x)
{
    return 1.5707963267948966
         - 0.1565827644218014 * x;
}

float ACos_Approx(float x)
{
    x = clamp(x, -1.0, 1.0);

    float ax = abs(x);
    float u = ACosPoly(ax) * sqrt(1.0 - ax);

    return x >= 0.0 ? u : PI - u;
}

/*
    DepthTexture contains positive linear view-space depth:

        viewPos.z = -3
        viewDepth = 3

    Therefore the reconstructed view-space position is:

        x = ndc.x * depth / P00
        y = ndc.y * depth / P11
        z = -depth
*/
float3 ReconstructViewPosition(float2 uv, float viewDepth)
{
    float2 ndc = uv * 2.0 - 1.0;

    ndc.y = -ndc.y;

    return float3(
        ndc.x * viewDepth / Projection[0][0],
        ndc.y * viewDepth / Projection[1][1],
        viewDepth
    );
}

float2 ViewToUV(float3 positionVS)
{
    float4 clip =
        mul(
            Projection,
            float4(positionVS, 1.0));

    float2 ndc =
        clip.xy / clip.w;

    return float2(
        ndc.x * 0.5 + 0.5,
        0.5 - ndc.y * 0.5
    );
}

float2 SampleBlueNoise(uint2 pixel, uint frame)
{
    uint2 noisePixel =
        pixel & 63;

    float2 noise =
        BlueNoiseTexture.Load(
            uint3(noisePixel, 0)).rg;

    float frameOffset =
        frac(
            (float) frame *
            0.61803398875);

    noise.x =
        frac(
            noise.x +
            frameOffset);

    noise.y =
        frac(
            noise.y +
            frameOffset *
            0.75487766);

    return noise;
}

float ComputeGTAO(
    float3 positionVS,
    float3 normalVS,
    uint2 pixel)
{
    float2 halfScreen =
        ScreenSize * 0.5;

    float3 viewVec =
        normalize(-positionVS);

    float2 uv =
        ViewToUV(positionVS);

    /*
        Approximate view-space pixel size.

        Projection[0][0] maps view-space X to NDC.
    */
    float pixelSizeVS =
        abs(positionVS.z) /
        max(
            Projection[0][0] *
            halfScreen.x,
            1e-5);

    float screenRadius =
        GTAORadius /
        max(
            pixelSizeVS,
            1e-5);

    if (screenRadius < PixelTooCloseThreshold)
        return 1.0;

    float minS =
        PixelTooCloseThreshold /
        screenRadius;

    float falloffDistance =
        GTAORadius *
        GTAOFalloffRange;

    float falloffFrom =
        GTAORadius *
        (1.0 - GTAOFalloffRange);

    float falloffMul =
        -1.0 /
        max(
            falloffDistance,
            1e-5);

    float falloffAdd =
        falloffFrom /
        max(
            falloffDistance,
            1e-5) +
        1.0;

    float2 noise =
        SampleBlueNoise(
            pixel,
            FrameIndex);

    float visibility = 0.0;

    [unroll]
    for (uint slice = 0;
         slice < SliceCount;
         ++slice)
    {
        float sliceK =
            ((float) slice + noise.x) /
            (float) SliceCount;

        float phi =
            sliceK * PI;

        float cosPhi =
            cos(phi);

        float sinPhi =
            sin(phi);

        float2 omega =
            float2(
                cosPhi,
                -sinPhi);

        omega *=
            screenRadius;

        float3 directionVec =
            float3(
                cosPhi,
                sinPhi,
                0.0);

        float3 orthoDirectionVec =
            directionVec -
            dot(
                directionVec,
                viewVec) *
            viewVec;

        float3 axisVec =
            normalize(
                cross(
                    orthoDirectionVec,
                    viewVec));

        float3 projectedNormalVec =
            normalVS -
            axisVec *
            dot(
                normalVS,
                axisVec);

        float projectedNormalLength =
            length(
                projectedNormalVec);

        if (projectedNormalLength < 1e-5)
        {
            visibility += 1.0;
            continue;
        }

        float signNorm =
            sign(
                dot(
                    orthoDirectionVec,
                    projectedNormalVec));

        float cosNorm =
            saturate(
                dot(
                    projectedNormalVec,
                    viewVec) /
                projectedNormalLength);

        float normalAngle =
            signNorm *
            ACos_Approx(
                cosNorm);

        float lowHorizonCos0 =
            cos(
                normalAngle +
                HALF_PI);

        float lowHorizonCos1 =
            cos(
                normalAngle -
                HALF_PI);

        float horizonCos0 =
            lowHorizonCos0;

        float horizonCos1 =
            lowHorizonCos1;

        [unroll]
        for (uint step = 0;
             step < StepsPerSlice;
             ++step)
        {
            float stepBaseNoise =
                (float) (
                    slice +
                    step * SliceCount)
                *
                0.6180339887498948482;

            float stepNoise =
                frac(
                    noise.y +
                    stepBaseNoise);

            float s =
                ((float) step +
                 stepNoise) /
                (float) StepsPerSlice;

            s =
                s * s;

            s += minS;

            float2 sampleOffset =
                s * omega;

            float2 sampleOffsetPixels =
                round(
                    sampleOffset);

            sampleOffset =
                sampleOffsetPixels /
                halfScreen;

            float2 sampleUV0 =
                saturate(
                    uv +
                    sampleOffset);

            float2 sampleUV1 =
                saturate(
                    uv -
                    sampleOffset);

            /*
                These are now LINEAR VIEW DEPTH values.

                Example:

                    sample = 3
                    sample = 8
                    sample = 16

                They are NOT [0,1] hardware depth.
            */
            float viewDepth0 =
                DepthTexture.SampleLevel(
                    PointSampler,
                    sampleUV0,
                    0);

            float viewDepth1 =
                DepthTexture.SampleLevel(
                    PointSampler,
                    sampleUV1,
                    0);

            /*
                Zero means no geometry/background.
            */
            if (viewDepth0 > 0.0)
            {
                float3 samplePos0 =
                    ReconstructViewPosition(
                        sampleUV0,
                        viewDepth0);

                float3 delta0 =
                    samplePos0 -
                    positionVS;

                float distSq0 =
                    dot(
                        delta0,
                        delta0);

                if (distSq0 <= RadiusSq &&
                    distSq0 > 1e-8)
                {
                    float dist0 =
                        sqrt(distSq0);

                    float3 horizonVec0 =
                        delta0 /
                        dist0;

                    float horizonCos0Sample =
                        dot(
                            horizonVec0,
                            viewVec);

                    float weight0 =
                        saturate(
                            dist0 *
                            falloffMul +
                            falloffAdd);

                    horizonCos0Sample =
                        lerp(
                            lowHorizonCos0,
                            horizonCos0Sample,
                            weight0);

                    horizonCos0 =
                        max(
                            horizonCos0,
                            horizonCos0Sample);
                }
            }

            if (viewDepth1 > 0.0)
            {
                float3 samplePos1 =
                    ReconstructViewPosition(
                        sampleUV1,
                        viewDepth1);

                float3 delta1 =
                    samplePos1 -
                    positionVS;

                float distSq1 =
                    dot(
                        delta1,
                        delta1);

                if (distSq1 <= RadiusSq &&
                    distSq1 > 1e-8)
                {
                    float dist1 =
                        sqrt(distSq1);

                    float3 horizonVec1 =
                        delta1 /
                        dist1;

                    float horizonCos1Sample =
                        dot(
                            horizonVec1,
                            viewVec);

                    float weight1 =
                        saturate(
                            dist1 *
                            falloffMul +
                            falloffAdd);

                    horizonCos1Sample =
                        lerp(
                            lowHorizonCos1,
                            horizonCos1Sample,
                            weight1);

                    horizonCos1 =
                        max(
                            horizonCos1,
                            horizonCos1Sample);
                }
            }
        }

        float h0 =
            -ACos_Approx(
                horizonCos1);

        float h1 =
            ACos_Approx(
                horizonCos0);

        float iarc0 =
            (
                cosNorm +
                2.0 * h0 *
                sin(normalAngle) -
                cos(
                    2.0 * h0 -
                    normalAngle)
            ) * 0.25;

        float iarc1 = (cosNorm + 2.0 * h1 * sin(normalAngle) - cos(2.0 * h1 - normalAngle)) * 0.25;

        float localVisibility = projectedNormalLength * (iarc0 + iarc1);

        visibility += localVisibility;
    }

    visibility /= (float) SliceCount;

    return saturate(visibility);
}

float PSMain(PSInput input) : SV_TARGET
{
    float viewDepth = DepthTexture.SampleLevel(PointSampler, input.UV, 0);
    if (viewDepth <= 0.0)
        return 1.0;

    float3 positionVS = ReconstructViewPosition(input.UV, viewDepth);

    float3 normalVS = normalize(NormalTexture.SampleLevel(PointSampler, input.UV, 0).xyz);

    uint2 pixel = uint2(input.Position.xy);

    float ao = ComputeGTAO(positionVS, normalVS, pixel);

    return ao;
}