using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace UNIHper {

    public class WinAPI {

        // 调用命令
        public static string CALLCMD (string InParameter) {
            System.Diagnostics.Process _process = new System.Diagnostics.Process ();
            System.Diagnostics.ProcessStartInfo _startInfo = new System.Diagnostics.ProcessStartInfo ();

            _startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            _startInfo.FileName = "cmd.exe";
            _startInfo.Arguments = InParameter;
            _startInfo.CreateNoWindow = true;
            _startInfo.UseShellExecute = false;
            _startInfo.StandardOutputEncoding = Encoding.Default;
            _startInfo.RedirectStandardOutput = true;
            _process.StartInfo = _startInfo;
            _process.Start ();
            using (StreamReader _reader = _process.StandardOutput) {
                StreamReader s = _process.StandardOutput;
                _process.WaitForExit ();
                return s.ReadToEnd ().Trim ();
            }
        }

        public static Process FindProcess (string ProcessName) {
            return Process.GetProcesses ().ToList ().Where (_process => _process.ProcessName == ProcessName).FirstOrDefault ();
        }

        public static void KeepTopWindow (string ProcessName) {
            var _process = FindProcess (ProcessName);
            if (_process == default (Process)) return;
            if (_process.MainWindowHandle != WinAPI.GetForegroundWindow ()) {
                var Hwnd = _process.MainWindowHandle;
                WinAPI.SetWindowPos ((IntPtr) Hwnd, (int) HWndInsertAfter.HWND_TOPMOST,
                    0, 0, 0, 0,
                    SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_FRAMECHANGED);
                WinAPI.ShowWindow (Hwnd, 3);
                WinAPI.SetForegroundWindow (Hwnd);
            }
        }

        public static void OpenProcess (string Path, string Args = "") {
            Process.Start (Path, Args);
        }

        public static void OpenProcessIfNotOpend (string Path, string Args = "") {
            string _processName = System.IO.Path.GetFileNameWithoutExtension (Path);
            if (WinAPI.FindProcess (_processName) != default (Process)) return;

            OpenProcess (Path, Args);
        }

        // 守护进程
        public static void DaemonProcess (string Path, string Args = "") {
            string _processName = Path;
            if (System.IO.Path.IsPathRooted (Path)) {
                // 如果进程未打开则打开该程序
                OpenProcessIfNotOpend (Path, Args);
                _processName = System.IO.Path.GetFileNameWithoutExtension (Path);
            }

            var _process = WinAPI.FindProcess (_processName);
            if (_process == default (Process)) return;

            // 如果程序挂起 则关闭进程
            if (WinAPI.IsHungAppWindow (_process.MainWindowHandle)) {
                _process.Kill ();
                return;
            }

            // 保持窗口置顶
            KeepTopWindow (_processName);
        }

        // 窗口是否最大化
        [DllImport ("user32.dll")]
        [
            return :MarshalAs (UnmanagedType.Bool)
        ]
        static extern bool IsIconic (IntPtr hWnd);

        // 窗口是否最小化

        [DllImport ("user32.dll")]
        [
            return :MarshalAs (UnmanagedType.Bool)
        ]
        static extern bool IsZoomed (IntPtr hWnd);

        // 操作窗口
        [DllImport ("user32.dll")]
        [
            return :MarshalAs (UnmanagedType.Bool)
        ]
        public static extern bool ShowWindow (IntPtr hwnd, int nCmdShow);

        // 获取当前激活窗口

        [DllImport ("user32.dll")]
        public static extern IntPtr GetForegroundWindow ();

        //激活指定窗口
        [DllImport ("user32.dll")]
        public static extern bool SetForegroundWindow (IntPtr hWnd);

        [DllImport ("user32.dll")]
        public static extern IntPtr FindWindow (string lpClassName, string lpWindowName);

        [DllImport ("kernel32.dll")]
        public static extern uint GetLastError ();

        [DllImport ("user32.dll")]
        [
            return :MarshalAs (UnmanagedType.Bool)
        ]
        public static extern bool IsHungAppWindow (IntPtr hWnd);

        // 鼠标时间模拟
        [DllImport ("user32.dll", EntryPoint = "mouse_event")]
        public static extern void mouse_event (

            int dwFlags,

            int dx,

            int dy,

            int cButtons,

            int dwExtraInfo
        );

        [DllImport ("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern int GetWindowLong (IntPtr hWnd, int nIndex);

        [DllImport ("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern int SetWindowLong (IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport ("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowPos (IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, SetWindowPosFlags flags);

        public static IntPtr CurrentWindow () {
            string _process = Process.GetCurrentProcess ().ProcessName;
            return FindWindow (null, _process);
        }

        public const int GWL_STYLE = (-16);
        public const int WS_CAPTION = 0xC00000;
        public const int WS_VISIBLE = 0x10000000;
        public const int WS_HSCROLL = 0x00100000;
        public const int WS_BORDER = 0x00800000;

        [DllImport ("User32.dll")]
        private static extern bool ShowWindowAsync (IntPtr hWnd, int cmdShow);

        [DllImport ("User32")]
        public extern static void SetCursorPos (int x, int y);

        [System.Runtime.InteropServices.DllImport ("user32.dll", EntryPoint = "ShowCursor")]
        public extern static bool ShowCursor (bool bShow);
        // 常量
        public const int MOUSEEVENTF_LEFTDOWN = 0x2;
        public const int MOUSEEVENTF_LEFTUP = 0x4;

        public const int MOUSEEVENTF_MIDDLEDOWN = 0x20;

        public const int MOUSEEVENTF_MIDDLEUP = 0x40;

        public const int MOUSEEVENTF_MOVE = 0x1;

        public const int MOUSEEVENTF_RIGHTDOWN = 0x8;

        public const int MOUSEEVENTF_RIGHTUP = 0x10;
    }

    public enum WindowType {
        SW_HIDE = 0,
        SW_SHOWRESTORE = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3, //最大化  
    }

    public enum HWndInsertAfter {
        HWND_TOPMOST = -1,
        HWND_NOTOPMOST = -2,
        HWND_BOTTOM = 1,
        HWND_TOP = 0
    }

    public enum UFullScreenMode {
        //
        // 摘要:
        //     Exclusive Mode.
        ExclusiveFullScreen = 0,
        //
        // 摘要:
        //     Fullscreen window.
        FullScreenWindow = 1,
        //
        // 摘要:
        //     Maximized window.
        MaximizedWindow = 2,
        //
        // 摘要:
        //     Windowed.
        Windowed = 3,

        // Not Unity buildin value below
        MinimizedWindow = 11,
    }

    [Flags]
    public enum SetWindowPosFlags : uint {
        // ReSharper disable InconsistentNaming

        /// <summary>
        ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
        /// </summary>
        SWP_ASYNCWINDOWPOS = 0x4000,

        /// <summary>
        ///     Prevents generation of the WM_SYNCPAINT message.
        /// </summary>
        SWP_DEFERERASE = 0x2000,

        /// <summary>
        ///     Draws a frame (defined in the window's class description) around the window.
        /// </summary>
        SWP_DRAWFRAME = 0x0020,

        /// <summary>
        ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
        /// </summary>
        SWP_FRAMECHANGED = 0x0020,

        /// <summary>
        ///     Hides the window.
        /// </summary>
        SWP_HIDEWINDOW = 0x0080,

        /// <summary>
        ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
        /// </summary>
        SWP_NOACTIVATE = 0x0010,

        /// <summary>
        ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
        /// </summary>
        SWP_NOCOPYBITS = 0x0100,

        /// <summary>
        ///     Retains the current position (ignores X and Y parameters).
        /// </summary>
        SWP_NOMOVE = 0x0002,

        /// <summary>
        ///     Does not change the owner window's position in the Z order.
        /// </summary>
        SWP_NOOWNERZORDER = 0x0200,

        /// <summary>
        ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
        /// </summary>
        SWP_NOREDRAW = 0x0008,

        /// <summary>
        ///     Same as the SWP_NOOWNERZORDER flag.
        /// </summary>
        SWP_NOREPOSITION = 0x0200,

        /// <summary>
        ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        SWP_NOSENDCHANGING = 0x0400,

        /// <summary>
        ///     Retains the current size (ignores the cx and cy parameters).
        /// </summary>
        SWP_NOSIZE = 0x0001,

        /// <summary>
        ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
        /// </summary>
        SWP_NOZORDER = 0x0004,

        /// <summary>
        ///     Displays the window.
        /// </summary>
        SWP_SHOWWINDOW = 0x0040,

        // ReSharper restore InconsistentNaming
    }

}