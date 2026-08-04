struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

cbuffer Material : register(b2)
{
    float exposure;
    float bloomIntensity;
    int tonemapMode;
    int _pad;
};

#define AGX_LOOK 2

static const float3x3 ACESInputMat =
{
    { 0.59719, 0.35458, 0.04823 },
    { 0.07600, 0.90834, 0.01566 },
    { 0.02840, 0.13383, 0.83777 }
};

// ODT_SAT => XYZ => D60_2_D65 => sRGB
static const float3x3 ACESOutputMat =
{
    { 1.60475, -0.53108, -0.07367 },
    { -0.10208, 1.10813, -0.00605 },
    { -0.00327, -0.07276, 1.07602 }
};

float3 RRTAndODTFit(float3 v)
{
    float3 a = v * (v + 0.0245786f) - 0.000090537f;
    float3 b = v * (0.983729f * v + 0.4329510f) + 0.238081f;
    return a / b;
}

float3 ACESFitted(float3 color)
{
    color = mul(ACESInputMat, color);

    // Apply RRT and ODT
    color = RRTAndODTFit(color);

    color = mul(ACESOutputMat, color);

    // Clamp to [0, 1]
    color = saturate(color);

    return color;
}

float3 AgXDefaultContrastApprox(float3 x)
{
    float3 x2 = x * x;
    float3 x4 = x2 * x2;

    return 15.5 * x4 * x2
          - 40.14 * x4 * x
          + 31.96 * x4
          - 6.868 * x2 * x
          + 0.4298 * x2
          + 0.1191 * x
          - 0.00232;
}

float3 AgX(float3 color)
{
    static const float3x3 AgXMatrix =
    {
        0.842479062253094, 0.0423282422610123, 0.0423756549057051,
        0.0784335999999992, 0.878468636469772, 0.0784336,
        0.0792237451477643, 0.0791661274605434, 0.879142973793104
    };

    static const float MinEV = -12.47393;
    static const float MaxEV = 4.026069;

    // Input transform
    color = mul(AgXMatrix, color);

    // Avoid log2(0)
    color = max(color, 1e-10);

    // Log2 encoding
    color = clamp(log2(color), MinEV, MaxEV);
    color = (color - MinEV) / (MaxEV - MinEV);

    // Contrast approximation
    return AgXDefaultContrastApprox(color);
}

float3 AgXEOTF(float3 color)
{
    static const float3x3 AgXInverseMatrix =
    {
        1.19687900512017, -0.0528968517574562, -0.0529716355144438,
        -0.0980208811401368, 1.15190312990417, -0.0980434501171241,
        -0.0990297440797205, -0.0989611768448433, 1.15107367264116
    };
    
    return mul(AgXInverseMatrix, color);

    //// Output transform
    //color = mul(AgXInverseMatrix, color);

    //// Linear output (for sRGB backbuffer)
    //color = pow(color, 2.2);

    //return color;
}

float3 AgXLook(float3 color)
{
    float3 offset = 0.0;
    float3 slope = 1.0;
    float3 power = 1.0;
    float saturation = 1.0;

#if AGX_LOOK == 1
    // Golden
    slope = float3(1.0, 0.9, 0.5);
    power = 0.8;
    saturation = 0.8;

#elif AGX_LOOK == 2
    // Punchy
    slope = 1.0;
    power = 1.35;
    saturation = 1.4;
#endif

    // ASC CDL
    color = pow(color * slope + offset, power);

    const float3 LumaWeights = float3(0.2126, 0.7152, 0.0722);
    float luma = dot(color, LumaWeights);

    return luma.xxx + saturation * (color - luma.xxx);
}

float3 ProcessAgX(float3 color)
{
    static const float3x3 AgXMatrix =
    {
        { 0.842479062253094, 0.0423282422610123, 0.0423756549057051 },
        { 0.0784335999999992, 0.878468636469772, 0.0784336 },
        { 0.0792237451477643, 0.0791661274605434, 0.879142973793104 }
    };

    static const float3x3 AgXInverseMatrix =
    {
        { 1.19687900512017, -0.0528968517574562, -0.0529716355144438 },
        { -0.0980208811401368, 1.15190312990417, -0.0980434501171241 },
        { -0.0990297440797205, -0.0989611768448433, 1.15107367264116 }
    };

    static const float MinEV = -12.47393;
    static const float MaxEV = 4.026069;

    // 1. Input transform (Scene-referred linear to AgX working space)
    color = mul(AgXMatrix, color);

    // 2. Log2 encoding and normalization
    color = max(color, 1e-10);
    color = clamp(log2(color), MinEV, MaxEV);
    color = (color - MinEV) / (MaxEV - MinEV);

    // 3. Apply Look (Contrast, Saturation, CDL)
    color = AgXLook(color);

    // 4. Contrast approximation curve
    color = AgXDefaultContrastApprox(color);

    // 5. Output transform (Back to linear working space)
    color = mul(AgXInverseMatrix, color);

    // Ensure bounds are clean before final display encoding
    return saturate(color);
}

// Evaluates the filmic S-curve for a given color vector
float3 FilmicCurve(float3 x)
{
    float A = 0.15; // Shoulder Strength
    float B = 0.50; // Linear Strength
    float C = 0.10; // Linear Angle
    float D = 0.20; // Toe Strength
    float E = 0.02; // Toe Numerator
    float F = 0.30; // Toe Denominator

    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - (E / F);
}

float3 TonemapFilmic(float3 color, float exposure = 2.0, float whitePoint = 11.2)
{
    // Apply exposure bias
    color *= exposure;

    // Apply filmic curve
    float3 curr = FilmicCurve(color);

    // Normalize against the white point so bright whites map to 1.0
    float3 whiteScale = 1.0 / FilmicCurve(float3(whitePoint, whitePoint, whitePoint));
    
    return saturate(curr * whiteScale);
}

// Accurate Linear to sRGB conversion
float3 LinearToSRGB(float3 linearColor)
{
    float3 sRGBLo = linearColor * 12.92;
    float3 sRGBHi = (pow(abs(linearColor), 1.0 / 2.4) * 1.055) - 0.055;
    
    // Branchless select
    float3 isHi = step(0.0031308, linearColor);
    return lerp(sRGBLo, sRGBHi, isHi);
}

Texture2D MAT_SceneColor : register(t0);
SamplerState MAT_SceneColorSampler : register(s0);

float4 PSMain(PSInput input) : SV_Target0
{
    float3 hdr = MAT_SceneColor.Sample(MAT_SceneColorSampler, input.UV).rgb;
    
    hdr *= exposure;
    
    //float3 ldr = ProcessAgX(hdr);
    float3 ldr = TonemapFilmic(hdr);
    ldr = LinearToSRGB(ldr);
    
    return float4(ldr, 1.0);
}