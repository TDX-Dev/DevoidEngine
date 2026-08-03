struct SH9
{
    float4 C[9];
};

StructuredBuffer<SH9> InputSH : register(t0);
RWStructuredBuffer<SH9> OutputSH : register(u0);

#include "./Common/MathConstants.hlsl"

cbuffer ReduceData : register(b0)
{
    uint InputCount;
    uint FinalPass;
    uint Padding0;
    uint Padding1;
}

[numthreads(64, 1, 1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
    uint id = DTid.x;

    uint a = id * 2;
    uint b = a + 1;

    if (a >= InputCount)
        return;

    SH9 result = InputSH[a];

    if (b < InputCount)
    {
        [unroll]
        for (uint i = 0; i < 9; i++)
        {
            result.C[i] += InputSH[b].C[i];
        }
    }

    if (FinalPass != 0)
    {
        const float invOmega = 1.0 / (4.0 * PI);

        result.C[0] *= invOmega * PI;

        result.C[1] *= invOmega * (2.0 * PI / 3.0);
        result.C[2] *= invOmega * (2.0 * PI / 3.0);
        result.C[3] *= invOmega * (2.0 * PI / 3.0);

        result.C[4] *= invOmega * (PI / 4.0);
        result.C[5] *= invOmega * (PI / 4.0);
        result.C[6] *= invOmega * (PI / 4.0);
        result.C[7] *= invOmega * (PI / 4.0);
        result.C[8] *= invOmega * (PI / 4.0);
    }

    OutputSH[id] = result;
}