using System;
using System.Runtime.InteropServices;

namespace Flatsch.Helper
{
    /// <summary>
    /// <see href="https://social.msdn.microsoft.com/Forums/vstudio/en-US/a3cb7db6-5014-430f-a5c2-c9746b077d4f/click-through-windows-and-child-image-issue?forum=wpf">Click through</see>
    /// <see href="https://www.c-sharpcorner.com/uploadfile/kirtan007/make-form-stay-always-on-top-of-every-window/">Topmost</see>
    /// </summary>
    public static class WindowHelper
    {
        public const int WS_EX_TRANSPARENT = 0x00000020; /* click through */
        public const int WS_EX_TOOLWINDOW = 0x00000080; /* hide from program switcher */
        public const int GWL_EXSTYLE = (-20);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static void PrepareWindow(IntPtr hwnd)
        {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }
    }
}
