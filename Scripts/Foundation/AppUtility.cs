using System.Diagnostics;
using UnityEngine;

namespace UHelper
{

public static class AppUtility
{
    public static void KeepWindowTop(float InInterval=10.0f){
        Managements.Timer.SetInterval(()=>{
            string _process = Process.GetCurrentProcess().ProcessName;
            WinAPI.ShowWindow(_process,WindowType.SW_SHOWMAXIMIZED);
        }, InInterval);
    }
}

}