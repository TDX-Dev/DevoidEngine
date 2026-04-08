struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 WorldspacePosition : TEXCOORD3;
};

void PSMain(PSInput input)
{
}

//float PSMain(PSInput input) : SV_TARGET
//{
//    //PSOutput output;
//    //output.Position = float3(input.Position.xyz / input.Position.w);
//    //output.Normal = float3(input.Normal);
//    //return output;
//}