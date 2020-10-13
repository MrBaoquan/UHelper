using System.Diagnostics;
using UnityEngine;

namespace UHelper
{

public static class AppUtility
{
    public static void KeepWindowTop(){
        Managements.Timer.SetInterval(()=>{
            string _process = Process.GetCurrentProcess().ProcessName;
            WinAPI.ShowWindow(_process,WindowType.SW_SHOWMAXIMIZED);
        },3);
    }
}

}