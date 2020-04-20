using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UHelper
{

public static class Managements
{
    public static readonly UIManager UI = UIManager.Instance;
    public static readonly ResourceManager Resource = ResourceManager.Instance;
    public static readonly USceneManager Scene = USceneManager.Instance;

    public static readonly ConfigManager Config = ConfigManager.Instance;

    public static T SceneScript<T>() where T : SceneScriptBase
    {
        return SceneScriptManager.Instance.GetSceneScript<T>();
    }

}

}
