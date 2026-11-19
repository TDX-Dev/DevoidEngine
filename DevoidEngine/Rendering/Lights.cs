using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering
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
        public GpuDirectionalLight InternalLight;
        public bool NeedsUpdate;

        public Vector3 Direction
        {
            get => new(InternalLight.Direction.X, InternalLight.Direction.Y, InternalLight.Direction.Z);
            set
            {
                InternalLight.Direction.X = value.X;
                InternalLight.Direction.Y = value.Y;
                InternalLight.Direction.Z = value.Z;
                NeedsUpdate = true;
            }
        }

        public bool Enabled
        {
            get => InternalLight.Direction.W == 1f;
            set
            {
                InternalLight.Direction.W = value ? 1f : 0f;
                NeedsUpdate = true;
            }
        }

        public Vector3 Color
        {
            get => new(InternalLight.Color.X, InternalLight.Color.Y, InternalLight.Color.Z);
            set
            {
                InternalLight.Color.X = value.X;
                InternalLight.Color.Y = value.Y;
                InternalLight.Color.Z = value.Z;
                NeedsUpdate = true;
            }
        }

        public float Intensity
        {
            get => InternalLight.Color.W;
            set
            {
                InternalLight.Color.W = value;
                NeedsUpdate = true;
            }
        }
    }

    public class PointLight
    {
        public GpuPointLight InternalLight;
        public bool NeedsUpdate;

        public Vector3 Position
        {
            get => new(InternalLight.position.X, InternalLight.position.Y, InternalLight.position.Z);
            set
            {
                InternalLight.position.X = value.X;
                InternalLight.position.Y = value.Y;
                InternalLight.position.Z = value.Z;
                NeedsUpdate = true;
            }
        }

        public Vector3 Color
        {
            get => new(InternalLight.color.X, InternalLight.color.Y, InternalLight.color.Z);
            set
            {
                InternalLight.color.X = value.X;
                InternalLight.color.Y = value.Y;
                InternalLight.color.Z = value.Z;
                NeedsUpdate = true;
            }
        }

        public bool Enabled
        {
            get => InternalLight.position.W == 1f;
            set
            {
                InternalLight.position.W = value ? 1f : 0f;
                NeedsUpdate = true;
            }
        }

        public float Intensity
        {
            get => InternalLight.color.W;
            set
            {
                InternalLight.color.W = value;
                NeedsUpdate = true;
            }
        }

        public float Range
        {
            get => InternalLight.range.X;
            set
            {
                InternalLight.range.X = value;
                NeedsUpdate = true;
            }
        }

        public int AttenuationType
        {
            get => (int)InternalLight.range.Y;
            set
            {
                InternalLight.range.Y = value;
                NeedsUpdate = true;
            }
        }

        public Vector2 AttenuationParameters
        {
            get => new(InternalLight.range.Z, InternalLight.range.W);
            set
            {
                InternalLight.range.Z = value.X;
                InternalLight.range.W = value.Y;
                NeedsUpdate = true;
            }
        }
    }

    public class SpotLight
    {
        public GpuSpotLight InternalLight;
        public bool NeedsUpdate;

        public Vector3 Position
        {
            get => new(InternalLight.position.X, InternalLight.position.Y, InternalLight.position.Z);
            set
            {
                InternalLight.position.X = value.X;
                InternalLight.position.Y = value.Y;
                InternalLight.position.Z = value.Z;
                NeedsUpdate = true;
            }
        }

        public Vector3 Color
        {
            get => new(InternalLight.color.X, InternalLight.color.Y, InternalLight.color.Z);
            set
            {
                InternalLight.color.X = value.X;
                InternalLight.color.Y = value.Y;
                InternalLight.color.Z = value.Z;
                NeedsUpdate = true;
            }
        }

        public bool Enabled
        {
            get => InternalLight.position.W == 1f;
            set
            {
                InternalLight.position.W = value ? 1f : 0f;
                NeedsUpdate = true;
            }
        }

        public float Intensity
        {
            get => InternalLight.color.W;
            set
            {
                InternalLight.color.W = value;
                NeedsUpdate = true;
            }
        }

        public float Range
        {
            get => InternalLight.direction.W;
            set
            {
                InternalLight.direction.W = value;
                NeedsUpdate = true;
            }
        }

        public Vector3 Direction
        {
            get => new(InternalLight.direction.X, InternalLight.direction.Y, InternalLight.direction.Z);
            set
            {
                InternalLight.direction.X = value.X;
                InternalLight.direction.Y = value.Y;
                InternalLight.direction.Z = value.Z;
                NeedsUpdate = true;
            }
        }

        public float InnerCutoff
        {
            get => InternalLight.innerCutoff;
            set
            {
                InternalLight.innerCutoff = value;
                NeedsUpdate = true;
            }
        }

        public float OuterCutoff
        {
            get => InternalLight.outerCutoff;
            set
            {
                InternalLight.outerCutoff = value;
                NeedsUpdate = true;
            }
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct GpuDirectionalLight
    {
        public Vector4 Direction; // Normalized + w for enabled/disabled
        public Vector4 Color; // Color + w for intensity
    }


    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public unsafe struct GpuPointLight
    {
        public Vector4 position; // 3 floats for position 1 float enabled/disabled
        public Vector4 color; // 3 floats for color 1 float for intensity
        public Vector4 range; // 1 float range, 1 float attenuation type, 2 floats attenuation parameters (Linear Factor and QuadraticFactor)

        //private fixed byte _padding[12];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GpuSpotLight
    {
        public Vector4 position; // W component is enabled/disabled
        public Vector4 color; // W component is intensity
        public Vector4 direction; // W component is radius

        public float innerCutoff;
        public float outerCutoff;

        public int shadowIndex;


        public float padding; // must match shader
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
