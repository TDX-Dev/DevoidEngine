struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz + handedness
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 WorldspacePosition : TEXCOORD3;
};

cbuffer Material : register(b3)
{
    int Face;
};

Texture2D MAT_PANORAMA_TEX : register(t0);
SamplerState MAT_PANORAMA_TEX_SAMPLER : register(s0);

static const float PI = 3.14159265359;

float3 GetDirection(int face, float2 uv)
{
    float2 xy = uv * 2.0f - 1.0f;

    float3 dir;

    if (face == 0)
        dir = float3(1.0, -xy.y, -xy.x); // +X
    else if (face == 1)
        dir = float3(-1.0, -xy.y, xy.x); // -X
    else if (face == 2)
        dir = float3(xy.x, 1.0, xy.y); // +Y
    else if (face == 3)
        dir = float3(xy.x, -1.0, -xy.y); // -Y
    else if (face == 4)
        dir = float3(xy.x, -xy.y, 1.0); // +Z
    else
        dir = float3(-xy.x, -xy.y, -1.0); // -Z

    return normalize(dir);
}

float2 DirToEquirectUV(float3 dir)
{
    float2 uv;

    uv.x = atan2(dir.z, dir.x) / (2.0 * PI) + 0.5;
    uv.y = asin(dir.y) / PI + 0.5;

    return uv;
}

float4 PSMain(PSInput input) : SV_Target
{
    
    float3 dir = normalize(input.WorldspacePosition);

    float2 uv;
    uv.x = atan2(dir.z, dir.x) / (2 * PI) + 0.5;
    uv.y = asin(dir.y) / PI + 0.5;

    float3 color = MAT_PANORAMA_TEX.Sample(MAT_PANORAMA_TEX_SAMPLER, uv).rgb;
    return float4(color, 1);
}