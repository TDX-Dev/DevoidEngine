struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 NormalViewSpace : TEXCOORD2;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

float4 PSMain(PSInput input) : SV_Target0
{
    float2 uv = input.UV * 8.0; // checker size

    int checker = ((int) floor(uv.x) + (int) floor(uv.y)) & 1;

    float3 magenta = float3(1.0, 0.0, 1.0);
    float3 black = float3(0.0, 0.0, 0.0);

    float3 color = checker ? magenta : black;

    return float4(color, 1.0);
}