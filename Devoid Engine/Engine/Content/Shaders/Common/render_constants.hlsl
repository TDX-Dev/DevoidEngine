cbuffer CameraData : register(b0)
{
    float4x4 View;
    float4x4 Projection;
    float4x4 InverseProjection;
    float4x4 InverseView;
    float4x4 InverseViewProjection;
    float3 Position;
    float NearClip;
    float FarClip;
    float2 ScreenSize;
    float _padding0;
};

cbuffer PerObject : register(b1)
{
    float4x4 Model;
    float4x4 invModel;
};