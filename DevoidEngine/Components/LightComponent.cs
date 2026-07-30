using DevoidEngine.Core;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class LightComponent : Component
    {
        public override string Type => nameof(LightComponent);

        private RID lightRID;
        private bool dirty = true;

        internal bool enabled = true;
        internal Vector3 color = Vector3.One;
        internal float intensity = 10f;
        internal float radius = 30f;

        internal bool castShadows = false;

        internal float outerCutoff = MathHelper.DegToRad(71);
        internal float innerCutoff = MathHelper.DegToRad(52);

        internal LightType lightType = LightType.PointLight;
        internal LightAttenuationType attenuationType = LightAttenuationType.Custom;

        internal float linearFactor = 0.7f;
        internal float quadraticFactor = 1.8f;

        #region Properties

        public LightType LightType
        {
            get => lightType;
            set
            {
                if (lightType == value)
                    return;

                DestroyRenderLight();

                lightType = value;

                CreateRenderLight();
            }
        }

        public bool CastShadows
        {
            get => castShadows;
            set
            {
                if (castShadows != value)
                {
                    castShadows = value;
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
            get => new(color, 1f);
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
            set
            {
                float deg = Math.Clamp(value, 0.1f, OuterCutoff - 0.1f);
                innerCutoff = MathHelper.DegToRad(deg);
                dirty = true;
            }
        }

        public float OuterCutoff
        {
            get => MathHelper.RadToDeg(outerCutoff);
            set
            {
                float deg = Math.Clamp(value, InnerCutoff + 0.1f, 89.0f);
                outerCutoff = MathHelper.DegToRad(deg);
                dirty = true;
            }
        }

        #endregion

        public override void OnAttach()
        {
            CreateRenderLight();
            Console.WriteLine("Light OnAttach");

        }

        public override void OnDestroy()
        {
            DestroyRenderLight();
        }

        private void CreateRenderLight()
        {
            RenderWorld world = Engine.Renderer.World;

            switch (lightType)
            {
                case LightType.PointLight:
                    lightRID = world.CreatePointLight();
                    break;

                case LightType.SpotLight:
                    lightRID = world.CreateSpotLight();
                    break;

                case LightType.DirectionalLight:
                    lightRID = world.CreateDirectionalLight();
                    break;
            }

            dirty = true;
        }

        private void DestroyRenderLight()
        {
            RenderWorld world = Engine.Renderer.World;

            switch (lightType)
            {
                case LightType.PointLight:
                    world.FreePointLight(lightRID);
                    break;

                case LightType.SpotLight:
                    world.FreeSpotLight(lightRID);
                    break;

                case LightType.DirectionalLight:
                    world.FreeDirectionalLight(lightRID);
                    break;
            }
        }

        public override void OnUpdate(float dt)
        {
            if (dirty)
                RebuildGPUData();
        }

        private void RebuildGPUData()
        {
            //Console.WriteLine("Rebuilt light data");
            RenderWorld world = Engine.Renderer.World;

            Vector3 position = gameObject.Transform.Position;
            Vector3 forward = gameObject.Transform.Forward;

            switch (lightType)
            {
                case LightType.PointLight:
                    {
                        world.PointLightSetPosition(lightRID, position);
                        world.PointLightSetEnabled(lightRID, enabled);
                        world.PointLightSetColor(lightRID, color);
                        world.PointLightSetIntensity(lightRID, intensity);
                        world.PointLightSetRange(lightRID, radius);

                        world.PointLightSetAttenuation(
                            lightRID,
                            attenuationType,
                            linearFactor,
                            quadraticFactor);

                        break;
                    }

                case LightType.SpotLight:
                    {
                        world.SpotLightSetPosition(lightRID, position);
                        world.SpotLightSetEnabled(lightRID, enabled);
                        world.SpotLightSetDirection(lightRID, forward);
                        world.SpotLightSetColor(lightRID, color);
                        world.SpotLightSetIntensity(lightRID, intensity);
                        world.SpotLightSetRadius(lightRID, radius);

                        world.SpotLightSetCutoffs(
                            lightRID,
                            innerCutoff,
                            outerCutoff);

                        world.SpotLightSetShadowIndex(
                            lightRID,
                            castShadows ? 0 : -1);

                        break;
                    }

                case LightType.DirectionalLight:
                    {
                        world.DirectionalLightSetDirection(
                            lightRID,
                            forward);

                        world.DirectionalLightSetColor(
                            lightRID,
                            color);

                        world.DirectionalLightSetIntensity(
                            lightRID,
                            intensity);

                        world.DirectionalLightSetEnabled(
                            lightRID,
                            enabled);

                        break;
                    }
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
    }
}
