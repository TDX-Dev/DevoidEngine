#ifndef RENDER_CONSTANTS
#define RENDER_CONSTANTS

#define MAX_SPOT_LIGHTS 10
#define MAX_POINT_LIGHTS 100
#define MAX_DIR_LIGHTS 1


cbuffer CameraData : register(b0)
{
    float4x4 View;
    float4x4 Projection;
    float4x4 InverseProjection;
    float4x4 InverseView;
    float4x4 InverseViewProjection;
    float3 CameraPosition;
    float NearClip;
    float FarClip;
    float2 ScreenSize;
    float _padding0;
};

cbuffer PerObject : register(b1)
{
    float4x4 Model;
    float4x4 invModel;
    float UniqueIdentifier;
    float3 _padding02;
};

struct ProbeGIGlobals
{
    float3 ProbeCount;
    float ProbeColorResolution;

    float3 ProbeCountLog2;
    float ProbeVisibilityResolution;

    float3 ProbeCountRCP;
    float IrradianceBorderWidth;

    float IrradianceBorderWidthRCP;
    float VisibilityBorderWidth;
    float VisibilityBorderWidthRCP;
    float MaxVisibilityDistance;

    float3 VolumeMin;
    uint ProbeTraceResolution;

    float3 VolumeMax;
    float _padding1;

    float3 VolumeSize;
    float _padding2;

    float3 ProbeSpacing;
    float _padding3;

    float3 ProbeSpacingRCP;
    float _padding4;

    float3 ProbeGridOrigin;
    float _padding5;
};


cbuffer PerFrame : register(b2)
{
    int FrameIndex;
    float3 _padding01;
    ProbeGIGlobals ProbeGISettings;
}

#endif