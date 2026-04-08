struct PSInput
{
    float4 Position : SV_POSITION;
};

cbuffer ClearColor : register(b0)
{
    float4 COLOR;
}


float4 PSMain(PSInput input) : SV_Target
{
    return COLOR;
}