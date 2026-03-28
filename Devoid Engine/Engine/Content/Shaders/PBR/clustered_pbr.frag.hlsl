struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float3 BiTangent : BINORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 FragPos : TEXCOORD2;
    float3 WorldPos : TEXCOORD3;
};

cbuffer Material : register(b2)
{
    float4 Albedo;
    float4 Emission;
    float EmissionStrength;
    float Metallic;
    float Roughness;
    int useDiffuseMap;
    int useNormalMap;
    int useRoughnessMap;
    int useEmissionMap;
    int useParallaxMap;
};

Texture2D DiffuseTexture : register(t10);
Texture2D NormalTexture : register(t11);
Texture2D RoughnessTexture : register(t12);
Texture2D EmissionTexture : register(t13);

SamplerState DiffuseSampler : register(s0);
SamplerState NormalSampler : register(s1);
SamplerState RoughnessSampler : register(s2);
SamplerState EmissionSampler : register(s3);


#define LIGHT_TYPE_MASK 0xC0000000u // top 2 bits
#define LIGHT_INDEX_MASK 0x3FFFFFFFu // lower 30 bits

#define TYPE_POINT 0u
#define TYPE_SPOT  1u
#define TYPE_DIRECTIONAL 2u

static const float PI = 3.14159265359;

struct DirectionalLight
{
    float4 direction; // xyz direction w enabled/disabled
    float4 color; // xyz color w intensity
};

struct PointLight
{
    float4 position;
    float4 color;
    float4 range;
};

struct SpotLight
{
    float4 position; // xyz = position, w = enabled
    float4 color; // rgb = color, w = intensity
    float4 direction; // xyz = direction, w = range
    float innerCutoff; // degrees
    float outerCutoff; // degrees
};

struct ScreenViewData
{
    float4x4 inverseProjectionMatrix;

    // tile info
    uint tileCountX;
    uint tileCountY;
    uint sliceCountZ;
    uint tilePixelWidth;

    uint screenWidth;
    uint screenHeight;
    uint tilePixelHeight;

    float sliceScaling;
    float sliceBias;
    float3 _padding;
};

struct LightGrid
{
    uint offset;
    uint count;
};

cbuffer CameraData : register(b0)
{
    float4x4 View;
    float4x4 Projection;
    float3 Position;
    float _padding;
    float zNear;
    float zFar;
    float2 _padding1;
};

cbuffer LightCounts : register(b1)
{
    uint numPointLights;
    uint numSpotLights;
    uint numDirectionalLights;
    uint _padding2;
};

StructuredBuffer<PointLight> pointLights : register(t0);
StructuredBuffer<SpotLight> spotLights : register(t1);

StructuredBuffer<ScreenViewData> screenViewData : register(t7); // binding = 7
StructuredBuffer<LightGrid> lightGrids : register(t3);
StructuredBuffer<uint> globalLightIndexList : register(t6);


float DistributionGGX(float3 N, float3 H, float roughness);
float GeometrySchlickGGX(float NdotV, float roughness);
float GeometrySmith(float3 N, float3 V, float3 L, float roughness);
float3 fresnelSchlick(float cosTheta, float3 F0);
float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness);
float3 CalcDirectionalLight(float3 normal, float3 fragPos, float3 viewDir, float3 albedo, float rough, float metal, float3 F0);
float3 CalcPointLight(uint index, float3 normal, float3 fragPos, float3 viewDir, float3 albedo, float rough, float metal, float3 F0, float viewDistance);
float3 CalcSpotLight(uint index, float3 normal, float3 fragPos, float3 viewDir, float3 albedo, float rough, float metal, float3 F0, float viewDistance);
float3 CalcIBL(float3 N, float3 V, float3 F0, float3 albedo, float3 R, float roughness, float metallic);

float linearDepth(float depthSample);
float3 GammaCorrect(float3 value);
float4 GetDiffuse(float2 texCoords);
float GetRoughness(float3 texCoords);
float GetMetallic(float3 texCoords);
float3 GetNormal(float3 N, float4 Tangent, float2 texCoords);
float3 GetEmission(float3 texCoords);
float ComputeAttenuation(PointLight light, float dist);


float ComputeAttenuation(PointLight light, float dist)
{
    float r = light.range.x;
    float t = saturate(dist / r);
    float atten = 1.0 - t; // simplest fade out
    atten *= step(dist, r); // clamp outside radius
    return atten;
}


//float GetClusterZNear(uint zSlice)
//{
//    // EQ: Z = NearZ (FarZ / NearZ) ^ (slice / numSlices)
//    // From: http://www.aortiz.me/2018/12/21/CG.html#part-2 and DOOM 2016 SIGGRAPH
    
//    // _ProjectionParams: Y = near, Z = far
//    // _ZBufferParams: Y = far / near
//    return zNear * pow(_ZBufferParams.y, (float) zSlice * INV_DEPTH_SLICES);
//}


float4 PSMain(PSInput input) : SV_TARGET
{
     // normalize normal
    float3 N = GetNormal(input.Normal, input.Tangent, input.UV0);

    // view dir
    float3 V = normalize(Position - input.WorldPos);
    
    float VDist = length(Position - input.FragPos);

    // ambient
    float3 radianceOut = float3(0.1, 0.1, 0.1);
    
    float3 albedo = GetDiffuse(input.UV0).rgb;


    float ndcDepth = input.Position.z;
    
    float linDepth = linearDepth(ndcDepth);


    uint zTile = (uint) max(log2(linDepth) * screenViewData[0].sliceScaling + screenViewData[0].sliceBias, 0.0f);


    
    // x,y tile indices:
    uint2 tileXY = uint2(floor(input.Position.xy / float2(screenViewData[0].tilePixelWidth, screenViewData[0].tilePixelHeight)));

    tileXY.x = min(tileXY.x, screenViewData[0].tileCountX - 1u);
    tileXY.y = min(tileXY.y, screenViewData[0].tileCountY - 1u);

    uint tileIndex = tileXY.x + screenViewData[0].tileCountX * tileXY.y + screenViewData[0].tileCountX * screenViewData[0].tileCountY * zTile;



    // get the light range for this tile
    uint lightCount = lightGrids[tileIndex].count;
    uint offset = lightGrids[tileIndex].offset;
    
    
    
    float3 F0 = float3(0.04, 0.04, 0.04);
    F0 = lerp(F0, albedo, Metallic);
    

    // loop only over point lights
    [loop] // dynamic loop
    for (uint i = 0; i < lightCount; i++)
    {
        uint packedIndex = globalLightIndexList[offset + i];
        uint lightType = (packedIndex & 0xC0000000u) >> 30;
        uint lightIndex = (packedIndex & 0x3FFFFFFFu);

        if (lightType == 0u) // TYPE_POINT
        {
            PointLight light = pointLights[lightIndex];
            
            radianceOut += CalcPointLight(lightIndex, N, input.FragPos, V, albedo, Roughness, Metallic, F0, VDist);
            
        }
    }
    return float4(radianceOut, 1.0f);
}

float3 CalcPointLight(uint index, float3 normal, float3 fragPos,
                      float3 viewDir, float3 albedo, float rough,
                      float metal, float3 F0, float viewDistance)
{
    if (pointLights[index].position.w == 0.0f)
        return float3(0.0f, 0.0f, 0.0f);

    // Point light basics
    float3 position = pointLights[index].position.xyz;
    float3 color = 10.0f * pointLights[index].color.rgb * pointLights[index].color.w;
    float radius = pointLights[index].range.x;

    // Common BRDF terms
    float3 lightDir = normalize(position - fragPos);
    float3 halfway = normalize(lightDir + viewDir);
    float nDotV = max(dot(normal, viewDir), 0.0f);
    float nDotL = max(dot(normal, lightDir), 0.0f);

    // Attenuation
    float distance = length(position - fragPos);
    float attenuation = ComputeAttenuation(pointLights[index], distance);
    float3 radianceIn = color * attenuation;

    // Cook-Torrance BRDF
    float NDF = DistributionGGX(normal, halfway, rough);
    float G = GeometrySmith(normal, viewDir, lightDir, rough);
    float3 F = fresnelSchlick(max(dot(halfway, viewDir), 0.0f), F0);

    // Specular & diffuse
    float3 kS = F;
    float3 kD = float3(1.0f, 1.0f, 1.0f) - kS;
    kD *= 1.0f - metal;

    float3 numerator = NDF * G * F;
    float denominator = 4.0f * nDotV * nDotL;
    float3 specular = numerator / max(denominator, 1e-7f);

    float3 radiance = (kD * (albedo / PI) + specular) * radianceIn * nDotL;

    // Shadow code (commented out in GLSL)
    // float3 fragToLight = fragPos - position;
    // float shadow = calcPointLightShadows(depthMaps[index], fragToLight, viewDistance);
    // radiance *= (1.0f - shadow);

    return radiance;
}

// ----------------------------------------------------------------------------
float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0f);
    float NdotH2 = NdotH * NdotH;

    float nom = a2;
    float denom = (NdotH2 * (a2 - 1.0f) + 1.0f);
    denom = PI * denom * denom;

    return nom / denom;
}

// ----------------------------------------------------------------------------
float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = roughness + 1.0f;
    float k = (r * r) / 8.0f;

    float nom = NdotV;
    float denom = NdotV * (1.0f - k) + k;

    return nom / denom;
}

// ----------------------------------------------------------------------------
float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0f);
    float NdotL = max(dot(N, L), 0.0f);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);

    return ggx1 * ggx2;
}

// ----------------------------------------------------------------------------
float3 fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0f - F0) * pow(clamp(1.0f - cosTheta, 0.0f, 1.0f), 5.0f);
}

// ----------------------------------------------------------------------------
float3 fresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    return F0 + (max(float3(1.0f - roughness, 1.0f - roughness, 1.0f - roughness), F0) - F0) *
                pow(clamp(1.0f - cosTheta, 0.0f, 1.0f), 5.0f);
}


float linearDepth(float depthSample)
{
    float depthRange = 2.0f * depthSample - 1.0f;
    float linearD  = 2.0f * zNear * zFar / (zFar + zNear - depthRange * (zFar - zNear));
    return linearD;
}

// ----------------------------------------------------------------------------
float4 GetDiffuse(float2 texCoords)
{
    float4 texColor = DiffuseTexture.Sample(DiffuseSampler, texCoords);
    float4 result = lerp(Albedo, texColor, useDiffuseMap ? 1.0f : 0.0f);
    return result;
}

// ----------------------------------------------------------------------------
float GetRoughness(float2 texCoords)
{
    float texValue = RoughnessTexture.Sample(RoughnessSampler, texCoords).g;
    float result = lerp(Roughness, texValue, useRoughnessMap ? 1.0f : 0.0f);
    return result;
}

// ----------------------------------------------------------------------------
float GetMetallic(float2 texCoords)
{
    float texValue = RoughnessTexture.Sample(RoughnessSampler, texCoords).b;
    // If you really mean metallic, you probably want a metallic texture or metallic field
    float result = lerp(Metallic, texValue, useRoughnessMap ? 1.0f : 0.0f);
    return result;
}

// ----------------------------------------------------------------------------
float3 GetEmission(float2 texCoords)
{
    float3 texValue = EmissionTexture.Sample(EmissionSampler, texCoords).rgb;
    float3 result = lerp(Emission.xyz, texValue, useEmissionMap ? 1.0f : 0.0f);
    return result;
}

// ----------------------------------------------------------------------------
float3 GetNormal(float3 Normal, float4 Tangent, float2 texCoords)
{
    float3 N = normalize(Normal);
    float3 T = normalize(Tangent.xyz);
    float3 B = normalize(cross(N, T) * Tangent.w);
    float3x3 TBN = float3x3(T, B, N);

    float3 normalMap = NormalTexture.Sample(NormalSampler, texCoords).rgb;
    //normalMap.g = 1.0f - normalMap.g; // Flip Y if needed
    normalMap = normalMap * 2.0f - 1.0f;
    normalMap = normalize(mul(normalMap, TBN)); // HLSL uses row-major by default

    float3 result = lerp(N, normalMap, useNormalMap ? 1.0f : 0.0f);
    return result;
}