cbuffer VertexConstants : register(b0)
{
    float4x4 projection_matrix;
    float dpi_scaling; // not used yet, but available
    float3 padding; // pad to 16-byte boundary
};

struct VSInput
{
    float2 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct PSInput
{
    float4 Position : SV_Position; // needed by rasterizer
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};


PSInput VSMain(VSInput input)
{
    PSInput output;
    output.Position = mul(projection_matrix, float4(input.Position, 0.0f, 1.0f));
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}
