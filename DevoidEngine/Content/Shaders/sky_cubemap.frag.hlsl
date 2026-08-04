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

#include "./Common/MathConstants.hlsl"

float4 PSMain(PSInput input) : SV_Target
{
    
    float3 dir = normalize(input.WorldspacePosition);

    float3 color = MAT_Skybox.Sample(MAT_Skybox_Sampler, dir).rgb;
    return float4(color, 1);
}

//float4 PSMain(PSInput input) : SV_Target
//{
//    float3 dir = normalize(input.WorldspacePosition);
//    float3 absDir = abs(dir);
//    float3 col = float3(0, 0, 0);

//    // Find the major axis (|X|, |Y|, or |Z|)
//    if (absDir.x >= absDir.y && absDir.x >= absDir.z)
//    {
//        // X-axis: +X = Red, -X = Cyan
//        col = (dir.x > 0.0) ? float3(1, 0, 0) : float3(0, 1, 1);
//    }
//    else if (absDir.y >= absDir.x && absDir.y >= absDir.z)
//    {
//        // Y-axis: +Y = Green, -Y = Magenta
//        col = (dir.y > 0.0) ? float3(0, 1, 0) : float3(1, 0, 1);
//    }
//    else
//    {
//        // Z-axis: +Z = Blue, -Z = Yellow
//        col = (dir.z > 0.0) ? float3(0, 0, 1) : float3(1, 1, 0);
//    }

//    return float4(col, 1.0);
//}