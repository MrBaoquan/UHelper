using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System;  
using System.Collections;  
using System.Collections.Generic;  
using System.Runtime.InteropServices;  
using UHelper; 

namespace UHelper
{

public enum WindowType
{
    SW_SHOWRESTORE = 1,
    SW_SHOWMINIMIZED = 2, //｛最小化, 激活}
    SW_SHOWMAXIMIZED = 3,//最大化  
}

public enum UFullScreenMode
{
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


public class WinAPI : Singleton<WinAPI>
{
    [DllImport("user32.dll")]  
    public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);  
    
    [DllImport("user32.dll")]  
    static extern IntPtr GetForegroundWindow();  
    public static bool ShowWindow(WindowType InWindowType)  
    {
        return ShowWindow(GetForegroundWindow(), (int)InWindowType);  
    }

    public static string CALLCMD(string InParameter)
    {
        System.Diagnostics.Process _process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo _startInfo = new System.Diagnostics.ProcessStartInfo();
        
        _startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        _startInfo.FileName = "cmd.exe";
        _startInfo.Arguments = InParameter;
        _startInfo.CreateNoWindow = true;
        _startInfo.UseShellExecute = false;
        _startInfo.StandardOutputEncoding = Encoding.Default;
        _startInfo.RedirectStandardOutput = true;
        _process.StartInfo = _startInfo;
        _process.Start();
        using (StreamReader _reader = _process.StandardOutput)
        {
            StreamReader s = _process.StandardOutput;
            _process.WaitForExit();
            return s.ReadToEnd().Trim();
        }
    }
}


}
