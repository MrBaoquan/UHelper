using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UHelperConfig", menuName = "UHelper/Config File", order = 2)]
public class UHelperConfig : ScriptableObject
{
    public string resPath = "Configs/res";
    public string uiPath = "Configs/ui";
}
