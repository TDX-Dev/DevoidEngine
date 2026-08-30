#include "./Common/SH9.hlsl"

StructuredBuffer<SH9> InputSH : register(t0);
RWStructuredBuffer<DiffuseProbe> OutputProbes : register(u0);

cbuffer PackData : register(b0)
{
    uint ProbeIndex;
    float Weight;
    uint Padding0;
    uint Padding1;
}

[numthreads(1, 1, 1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
    if (DTid.x != 0)
        return;

    OutputProbes[ProbeIndex].Weight = 1;
    OutputProbes[ProbeIndex].Coefficients = InputSH[0];
}