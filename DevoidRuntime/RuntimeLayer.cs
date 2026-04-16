using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidRuntime
{
    internal class RuntimeLayer : Layer
    {
        public override void OnAttach()
        {

            Application.ApplyProjectSettings();
        }

        public override void OnUpdate(float deltaTime)
        {
            SceneManager.CurrentScene.Update(deltaTime);
        }

        public override void OnFixedUpdate(float deltaTime)
        {
            SceneManager.CurrentScene.FixedUpdate(deltaTime);
        }

        public override void OnRender()
        {
            SceneManager.CurrentScene.Render();
            Application.RenderScene();   
        }

        public override void OnPostRender()
        {
            Texture2D renderOutput = (Texture2D)SceneManager.CurrentScene?.GetDefaultCamera3D()?.Camera?.RenderTarget?.GetRenderTexture(0);
            RenderAPI.RenderToScreen(renderOutput);
        }

        public override void OnResize(int width, int height)
        {
            Screen.Size = new Vector2(width, height);
            Renderer.Resize(width, height);
            SceneManager.CurrentScene?.ResizeCameras(width,height);
        }

    }
}
