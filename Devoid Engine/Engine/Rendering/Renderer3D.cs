﻿using DevoidEngine.Engine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public class DrawCommand
    {
        public Mesh Mesh;
        public int MaterialHandle;
        public Matrix4x4 WorldMatrix;
    }


    public static class Renderer3D
    {
        public static IRenderPipeline ActiveRenderingPipeline;

        public static List<DrawCommand> DrawCommandList;

        public static void Initialize(int width, int height)
        {
            DrawCommandList = new List<DrawCommand>();

            ActiveRenderingPipeline = new ClusteredRenderer();

            ActiveRenderingPipeline.Initialize(width, height);
        }

        public static DrawCommand SubmitMesh(Mesh mesh, int materialHandle, Matrix4x4 worldMatrix)
        {
            DrawCommandList.Add(new DrawCommand()
            {
                Mesh = mesh,
                MaterialHandle = materialHandle,
                WorldMatrix = worldMatrix
            });

            return DrawCommandList.Last();
        }


        public static void BeginRender(Camera camera)
        {
            ActiveRenderingPipeline.BeginRender(camera);
        }

        public static void Render()
        {
            ActiveRenderingPipeline.Render();
        }

        public static void EndRender()
        {
            ActiveRenderingPipeline.EndRender();
        }

        public static IInputLayout GetInputLayout(Mesh mesh, Shader shader)
        {
            var key = (mesh.VertexBuffer.Layout, shader.vShader);
            if (!InputLayoutManager.inputLayoutCache.TryGetValue(key, out var layout))
            {
                layout = Renderer.graphicsDevice.CreateInputLayout(mesh.VertexBuffer.Layout, shader.vShader);
                InputLayoutManager.inputLayoutCache[key] = layout;
            }
            return layout;
        }

        public static void Resize(int width, int height)
        {
            ActiveRenderingPipeline.Resize(width, height);
        }
    }
}
