using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UHelperConfig", menuName = "UHelper/Assets/UHelperConfig", order = 1)]
public class UHelperConfig : ScriptableObject
{
    private static UHelperConfig instance = null;
    private static UHelperConfig Self(){
        if(instance==null)
            instance = Resources.Load<UHelperConfig>("UHelperConfig");
        return instance;
    }

    public static string ResourceConfigPath{
        get{
            return Self().resPath;
        }
    }

    public static string UIConfigPath{
        get{
            return Self().uiPath;
        }
    }

    public static string AssemblyConfigPath{
        get{
            return Self().assemblyPath;
        }
    }

    public string resPath = "UHelper/res";
    public string uiPath = "UHelper/ui";
    public string assemblyPath = "UHelper/assemblies";
}
