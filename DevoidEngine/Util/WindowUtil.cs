using System.Runtime.InteropServices;

namespace DevoidEngine.Util
{
    internal static partial class WindowUtil
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

        [LibraryImport("dwmapi.dll", SetLastError = true)]
        private static partial int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            [MarshalAs(UnmanagedType.Bool)] ref bool attrValue,
            int attrSize
        );

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
        private static partial IntPtr LoadLibraryExW(
            string lpLibFileName,
            IntPtr hFile,
            uint dwFlags
        );

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr GetProcAddress(
            IntPtr hModule,
            [MarshalAs(UnmanagedType.LPStr)] string procName
        );

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
            int hr = DwmSetWindowAttribute(
                windowHandle,
                DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref darkModeEnabled,
                Marshal.SizeOf<bool>()
            );

            if (hr < 0)
            {
                Console.WriteLine($"DwmSetWindowAttribute failed: 0x{hr:X}");
                return;
            }
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
    }
}
