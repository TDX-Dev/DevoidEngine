struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 NormalViewSpace : TEXCOORD2;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
    uint ProbeIndex : TEXCOORD3;
};

#include "./Common/RenderConstants.hlsl"
#include "./Common/ProbeGI.hlsl"


PSInput VSMain(VSInput input, uint instanceID : SV_InstanceID)
{
    PSInput output;

    uint probeX = instanceID % (uint) ProbeGISettings.ProbeCount.x;

    uint probeY = (instanceID / (uint) ProbeGISettings.ProbeCount.x) % (uint) ProbeGISettings.ProbeCount.y;

    uint probeZ = instanceID / ((uint) ProbeGISettings.ProbeCount.x * (uint) ProbeGISettings.ProbeCount.y);

    uint3 probeIndex = uint3(probeX, probeY, probeZ);

    float3 gridPosition = ProbeGISettings.ProbeGridOrigin + float3(probeIndex) * ProbeGISettings.ProbeSpacing;

    // Texture2DArray:
    // X = probe X
    // Y = probe Y
    // Z = probe Z / array slice
    float4 probeInfo = ProbeInfoImage.Load(int4(probeX, probeY, probeZ, 0));

    float3 probePosition = gridPosition + probeInfo.xyz * ProbeGISettings.ProbeSpacing;

    // Scale your visualization mesh independently.
    float3 worldPosition = probePosition + mul(Model, float4(input.Position, 1.0)).xyz;

    float4 worldPos = float4(worldPosition, 1.0);

    output.Position = mul(Projection, mul(View, worldPos));

    output.WorldspacePosition = worldPos.xyz;
    output.UV = input.UV;
    output.ProbeIndex = instanceID;

    float3x3 normalMatrix = transpose((float3x3) invModel);

    float3 N = normalize(mul(normalMatrix, input.Normal));
    float3 T = normalize(mul(normalMatrix, input.Tangent.xyz));

    output.Normal = N;
    output.Tangent = float4(T, input.Tangent.w);

    output.NormalViewSpace = normalize(mul((float3x3) View, N));

    return output;
}