using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Utilities
{
    class WindowUtil
    {
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        private enum PreferredAppMode
        {
            Default,
            AllowDark,
            ForceDark,
            ForceLight,
            Max
        }

        [DllImport("dwmapi.dll", SetLastError = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref bool attrValue, int attrSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryExW(string lpLibFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        private delegate PreferredAppMode SetPreferredAppModeDelegate(PreferredAppMode appMode);

        public static unsafe void EnableDarkMode(IntPtr hwnd)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) != true)
            {
                Console.WriteLine("Editor not running on windows, skipping dark titlebar.");
                return;
            }

            IntPtr windowHandle = hwnd;

            bool darkModeEnabled = true;
            DwmSetWindowAttribute(windowHandle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkModeEnabled, Marshal.SizeOf(darkModeEnabled));
            //ShowWindow(windowHandle, 2); // SW_MINIMIZE
            //ShowWindow(windowHandle, 9); // SW_RESTORE

            IntPtr hUxtheme = LoadLibraryExW("uxtheme.dll", IntPtr.Zero, 0x00001000); // LOAD_LIBRARY_SEARCH_SYSTEM32

            if (hUxtheme != IntPtr.Zero)
            {
                IntPtr procAddress = GetProcAddress(hUxtheme, "#135");
                if (procAddress != IntPtr.Zero)
                {
                    SetPreferredAppModeDelegate SetPreferredAppMode = Marshal.GetDelegateForFunctionPointer<SetPreferredAppModeDelegate>(procAddress);
                    SetPreferredAppMode(PreferredAppMode.ForceDark);
                }
            }
        }

        public static unsafe IntPtr GetWin32Window(GameWindow window)
        {
            unsafe
            {
                var glfwHandle = window.WindowPtr;
                IntPtr hwnd = (IntPtr)OpenTK.Windowing.GraphicsLibraryFramework.GLFW.GetWin32Window(glfwHandle);
                return hwnd;

            }

        }
    }
}
