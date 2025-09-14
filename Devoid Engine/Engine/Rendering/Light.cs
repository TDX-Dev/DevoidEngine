﻿using System.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public enum LightType
    {
        PointLight,
        SpotLight,
        DirectionalLight
    }

    public enum LightAttenuationType
    {
        Custom = 0,
        Constant = 1,
        Linear = 2,
        Quadratic = 3,
    }

    public class DirectionalLight
    {
        public GPUDirectionalLight internalLight;
        public bool needsUpdate;

        public Vector3 direction
        {
            get => new Vector3(internalLight.Direction.X, internalLight.Direction.Y, internalLight.Direction.Z);
            set
            {
                internalLight.Direction.X = value.X;
                internalLight.Direction.Y = value.Y;
                internalLight.Direction.Z = value.Z;
                needsUpdate = true;
            }
        }

        public bool enabled
        {
            get => internalLight.Direction.W == 1f;
            set
            {
                internalLight.Direction.W = value ? 1f : 0f;
                needsUpdate = true;
            }
        }

        public Vector3 color
        {
            get => new Vector3(internalLight.Color.X, internalLight.Color.Y, internalLight.Color.Z);
            set
            {
                internalLight.Color.X = value.X;
                internalLight.Color.Y = value.Y;
                internalLight.Color.Z = value.Z;
                needsUpdate = true;
            }
        }

        public float intensity
        {
            get => internalLight.Color.W;
            set
            {
                internalLight.Color.W = value;
                needsUpdate = true;
            }
        }
    }

    public class PointLight
    {
        public GPUPointLight internalLight;
        public bool needsUpdate;

        public Vector3 position
        {
            get => new Vector3(internalLight.position.X, internalLight.position.Y, internalLight.position.Z);
            set
            {
                internalLight.position.X = value.X;
                internalLight.position.Y = value.Y;
                internalLight.position.Z = value.Z;
                needsUpdate = true;
            }
        }

        public Vector3 color
        {
            get => new Vector3(internalLight.color.X, internalLight.color.Y, internalLight.color.Z);
            set
            {
                internalLight.color.X = value.X;
                internalLight.color.Y = value.Y;
                internalLight.color.Z = value.Z;
                needsUpdate = true;
            }
        }

        public bool enabled
        {
            get => internalLight.position.W == 1f;
            set
            {
                internalLight.position.W = value ? 1f : 0f;
                needsUpdate = true;
            }
        }

        public float intensity
        {
            get => internalLight.color.W;
            set
            {
                internalLight.color.W = value;
                needsUpdate = true;
            }
        }

        public float range
        {
            get => internalLight.range.X;
            set
            {
                internalLight.range.X = value;
                needsUpdate = true;
            }
        }

        public int attenuationType
        {
            get => (int)internalLight.range.Y;
            set
            {
                internalLight.range.Y = value;
                needsUpdate = true;
            }
        }

        public Vector2 attenuationParameters
        {
            get => new Vector2(internalLight.range.Z, internalLight.range.W);
            set
            {
                internalLight.range.Z = value.X;
                internalLight.range.W = value.Y;
                needsUpdate = true;
            }
        }
    }

    public class SpotLight
    {
        public GPUSpotLight internalLight;
        public bool needsUpdate;

        public Vector3 position
        {
            get => new Vector3(internalLight.position.X, internalLight.position.Y, internalLight.position.Z);
            set
            {
                internalLight.position.X = value.X;
                internalLight.position.Y = value.Y;
                internalLight.position.Z = value.Z;
                needsUpdate = true;
            }
        }

        public Vector3 color
        {
            get => new Vector3(internalLight.color.X, internalLight.color.Y, internalLight.color.Z);
            set
            {
                internalLight.color.X = value.X;
                internalLight.color.Y = value.Y;
                internalLight.color.Z = value.Z;
                needsUpdate = true;
            }
        }

        public bool enabled
        {
            get => internalLight.position.W == 1f;
            set
            {
                internalLight.position.W = value ? 1f : 0f;
                needsUpdate = true;
            }
        }

        public float intensity
        {
            get => internalLight.color.W;
            set
            {
                internalLight.color.W = value;
                needsUpdate = true;
            }
        }

        public float range
        {
            get => internalLight.direction.W;
            set
            {
                internalLight.direction.W = value;
                needsUpdate = true;
            }
        }

        public Vector3 direction
        {
            get => new Vector3(internalLight.direction.X, internalLight.direction.Y, internalLight.direction.Z);
            set
            {
                internalLight.direction.X = value.X;
                internalLight.direction.Y = value.Y;
                internalLight.direction.Z = value.Z;
                needsUpdate = true;
            }
        }

        public float innerCutoff
        {
            get => internalLight.innerCutoff;
            set
            {
                internalLight.innerCutoff = value;
                needsUpdate = true;
            }
        }

        public float outerCutoff
        {
            get => internalLight.outerCutoff;
            set
            {
                internalLight.outerCutoff = value;
                needsUpdate = true;
            }
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct GPULightData
    {
        public uint numPointLights;
        public uint numSpotLights;
        public uint numDirectionalLights;
        public uint _padding; // Ensure 16-byte alignment

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096)]
        public GPUPointLight[] pointLights;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public GPUSpotLight[] spotLights;

        public GPUDirectionalLight directionalLights;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct GPUDirectionalLight
    {
        public Vector4 Direction; // Normalized + w for enabled/disabled
        public Vector4 Color; // Color + w for intensity
    }


    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct GPUPointLight
    {
        public Vector4 position; // 3 floats for position 1 float enabled/disabled
        public Vector4 color; // 3 floats for color 1 float for intensity
        public Vector4 range; // 1 float range, 1 float attenuation type, 2 floats attenuation parameters (Linear Factor and QuadraticFactor)

        //private fixed byte _padding[12];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct GPUSpotLight
    {
        public Vector4 position; // xyz position w enabled
        public Vector4 color; // xyz color w intensity
        public Vector4 direction; // xyz direction w range
        public float innerCutoff;
        public float outerCutoff;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct LightData
    {
        public uint pointLightCount;
        public uint directionalLightCount;
        public uint spotLightCount;
        private fixed byte _padding[4];
    }
}
