#ifndef SH9_INCLUDED
#define SH9_INCLUDED

struct SH9
{
    float4 C[9];
};

struct DiffuseProbe
{
    float Weight;
    float3 Padding;
    SH9 Coefficients;
};

void EvaluateSHBasis(float3 d, out float sh[9])
{
    float x = d.x;
    float y = d.y;
    float z = d.z;

    sh[0] = 0.282095f;

    sh[1] = 0.488603f * y;
    sh[2] = 0.488603f * z;
    sh[3] = 0.488603f * x;

    sh[4] = 1.092548f * x * y;
    sh[5] = 1.092548f * y * z;
    sh[6] = 0.315392f * (3.0f * z * z - 1.0f);
    sh[7] = 1.092548f * x * z;
    sh[8] = 0.546274f * (x * x - y * y);
}

float3 EvaluateSH(SH9 sh, float3 normal)
{
    float basis[9];

    EvaluateSHBasis(normal, basis);

    float3 result = 0.0;

    [unroll]
    for (uint i = 0; i < 9; i++)
    {
        result += sh.C[i].rgb * basis[i];
    }

    return max(result, 0.0);
}

float3 EvaluateDiffuseProbe(DiffuseProbe probe, float3 normal)
{
    return EvaluateSH(probe.Coefficients, normal);
}


#endif