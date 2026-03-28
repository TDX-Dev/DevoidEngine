using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class LightComponent : Component, IRenderComponent
    {
        public override string Type => nameof(LightComponent);

        private GPUPointLight gpuPoint;
        private GPUSpotLight gpuSpot;
        private GPUDirectionalLight gpuDirectional;

        private bool dirty = true;

        private bool enabled = true;
        private Vector3 color = Vector3.One;
        private float intensity = 10f;
        private float radius = 30f;

        private float outerCutoff = 1f;
        private float innerCutoff = 1f;

        private LightType lightType = LightType.PointLight;
        private LightAttenuationType attenuationType = LightAttenuationType.Custom;
        private float linearFactor = 0.7f;
        private float quadraticFactor = 1.8f;

        #region Properties

        public LightType LightType
        {
            get => lightType;
            set
            {
                if (lightType != value)
                {
                    lightType = value;
                    dirty = true;
                }
            }
        }

        public bool Enabled
        {
            get => enabled;
            set { enabled = value; dirty = true; }
        }

        public Vector4 Color
        {
            get => new Vector4(color, 1f);
            set { color = new Vector3(value.X, value.Y, value.Z); dirty = true; }
        }

        public float Radius
        {
            get => radius;
            set { radius = value; dirty = true; }
        }

        public float Intensity
        {
            get => intensity;
            set { intensity = value; dirty = true; }
        }

        public LightAttenuationType AttenuationFunction
        {
            get => attenuationType;
            set { attenuationType = value; dirty = true; }
        }

        public float LinearFactor
        {
            get => linearFactor;
            set { linearFactor = value; dirty = true; }
        }

        public float QuadraticFactor
        {
            get => quadraticFactor;
            set { quadraticFactor = value; dirty = true; }
        }

        public float InnerCutoff
        {
            get => MathHelper.RadToDeg(innerCutoff);
            set { innerCutoff = MathHelper.DegToRad(value); dirty = true; }
        }

        public float OuterCutoff
        {
            get => MathHelper.RadToDeg(outerCutoff);
            set { outerCutoff = MathHelper.DegToRad(value); dirty = true; }
        }

        #endregion

        public override void OnUpdate(float dt)
        {
            if (gameObject.Transform.hasMoved)
                dirty = true;
        }

        private void RebuildGPUData()
        {
            var transform = gameObject.Transform;

            Vector3 position = transform.Position;

            // Compute forward direction from quaternion
            Vector3 forward = Vector3.Normalize(
                Vector3.Transform(-Vector3.UnitZ, transform.Rotation)
            );

            switch (lightType)
            {
                case LightType.PointLight:
                    gpuPoint.position = new Vector4(position, enabled ? 1f : 0f);
                    gpuPoint.color = new Vector4(color, intensity);
                    gpuPoint.range = new Vector4(
                        radius,
                        (float)attenuationType,
                        linearFactor,
                        quadraticFactor);
                    break;

                case LightType.SpotLight:
                    gpuSpot.position = new Vector4(position, enabled ? 1f : 0f);
                    gpuSpot.color = new Vector4(color, intensity);
                    gpuSpot.direction = new Vector4(forward, radius);
                    gpuSpot.innerCutoff = innerCutoff;
                    gpuSpot.outerCutoff = outerCutoff;
                    break;

                case LightType.DirectionalLight:
                    gpuDirectional.Direction = new Vector4(forward, enabled ? 1f : 0f);
                    gpuDirectional.Color = new Vector4(color, intensity);
                    break;
            }

            dirty = false;
        }

        //public void Collect(CameraComponent3D camera, CameraRenderContext ctx)
        //{
        //    if (!enabled)
        //        return;

        //    Vector3 pos = gameObject.transform.Position;

        //    // Fake influence radius for testing
        //    float radius = 5.0f;

        //    Vector3 min = pos - new Vector3(radius);
        //    Vector3 max = pos + new Vector3(radius);

        //    if (!camera.Camera.IntersectsAABB(min, max))
        //        return;

        //    if (dirty)
        //        RebuildGPUData();

        //    switch (lightType)
        //    {
        //        case LightType.PointLight:
        //            ctx.pointLights.Add(gpuPoint);
        //            break;

        //        case LightType.SpotLight:
        //            ctx.spotLights.Add(gpuSpot);
        //            break;

        //        case LightType.DirectionalLight:
        //            // usually never culled
        //            ctx.directionalLights.Add(gpuDirectional);
        //            break;
        //    }
        //}


        // Called during render collection phase
        public void Collect(CameraComponent3D camera, CameraRenderContext ctx)
        {
            if (!enabled)
                return;

            // Optional: frustum test for point/spot lights here

            if (dirty)
                RebuildGPUData();

            switch (lightType)
            {
                case LightType.PointLight:
                    ctx.pointLights.Add(gpuPoint);
                    break;

                case LightType.SpotLight:
                    ctx.spotLights.Add(gpuSpot);
                    break;

                case LightType.DirectionalLight:
                    ctx.directionalLights.Add(gpuDirectional);
                    break;
            }
        }
    }
}