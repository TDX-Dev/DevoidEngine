﻿using OpenTK.Windowing.Desktop;
using System.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DevoidEngine.Engine.Core
{
    public struct WindowSpecification
    {
        public Vector2 WindowLocation;
        public Vector2 WindowSize;
        public Vector2 WindowMinimumSize;
        public Vector2 WindowMaximumSize;
        
        public float EventTimeout;

        public double TargetUpdateRate;
        public double TargetRenderRate;

        public WindowState WindowState;

        public VSyncMode VSync;
        public bool StartVisible;
        public bool StartFocused;

        public string WindowTitle;
    }

    public enum WindowState
    {
        Normal = 0,
        Minimized,
        Maximized,
        Fullscreen
    }

    public enum VSyncMode
    {
        On,
        Off,
        Adaptive
    }

    public class Window : NativeWindow
    {
        public event Action? OnLoad;
        public event Action? OnUnload;

        public event Action<double>? OnFixedUpdate;
        public event Action<double>? OnUpdateFrame;
        public event Action? OnRenderFrame;

        public WindowSpecification WindowSpecification;

        public Window(WindowSpecification windowSpec) : base(new NativeWindowSettings()
        {
            API = OpenTK.Windowing.Common.ContextAPI.NoAPI,
            StartVisible = windowSpec.StartVisible,
            StartFocused = windowSpec.StartFocused,
            WindowState = (OpenTK.Windowing.Common.WindowState)windowSpec.WindowState,
            AutoLoadBindings = false,

            Vsync = (OpenTK.Windowing.Common.VSyncMode)windowSpec.VSync,

            Location = new OpenTK.Mathematics.Vector2i((int)windowSpec.WindowLocation.X, (int)windowSpec.WindowLocation.Y),
            ClientSize = new OpenTK.Mathematics.Vector2i((int)windowSpec.WindowSize.X, (int)windowSpec.WindowSize.Y),
            //MaximumClientSize = new OpenTK.Mathematics.Vector2i((int)windowSpec.WindowMaximumSize.X, (int)windowSpec.WindowMaximumSize.Y),
            //MinimumClientSize = new OpenTK.Mathematics.Vector2i((int)windowSpec.WindowMinimumSize.X, (int)windowSpec.WindowMinimumSize.Y)
            
            Title = windowSpec.WindowTitle
        })
        {
            WindowSpecification = windowSpec;
        }

        public void Load()
        {
            OnLoad?.Invoke();
            this.IsVisible = true;
        }

        public void Update(double deltaTime)
        {
            OnUpdateFrame?.Invoke(deltaTime);
        }

        public void FixedUpdate(double deltaTime)
        {
            OnFixedUpdate?.Invoke(deltaTime);
        }

        public void Render()
        {
            OnRenderFrame?.Invoke();
        }

        public void Close()
        {
            OnUnload?.Invoke();
            base.Close();
            base.Dispose();
        }


        public void ProcessEvents()
        {
            ProcessEvents(0);
        }

        public unsafe nint GetWindowPtr()
        {
            unsafe
            {
                var glfwHandle = WindowPtr;
                nint hwnd = OpenTK.Windowing.GraphicsLibraryFramework.GLFW.GetWin32Window(glfwHandle);
                return hwnd;

            }

        }


    }

}
