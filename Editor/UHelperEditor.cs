using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UHelperEditor : Editor
{
   [MenuItem("UHelper/Initialize",priority=0)]
   public static void CreateDefault()
   {
        UHelper.UHelperEntry[] _objs = FindObjectsOfType(typeof(UHelper.UHelperEntry)) as UHelper.UHelperEntry[];

        for(int _index=0; _index<_objs.Length;++_index){
            GameObject _obj = _objs[_index].gameObject;
            Debug.Log("Destory UHelperEntry: " + _obj.name);
            DestroyImmediate(_obj,true);
        }
        string _uhelperPrefabPath = @"Assets\UHelper\Resources\Prefabs\UHelper.prefab";
        Object _uhelperPrefab = AssetDatabase.LoadAssetAtPath(_uhelperPrefabPath,typeof(GameObject));
        GameObject _newUHelper = PrefabUtility.InstantiatePrefab(_uhelperPrefab) as GameObject;

        string _uhelperConfigPath = Application.dataPath + "/UHelper/Resources/Configs";
        string _customConfigPath = Application.dataPath + "/Resources/UHelper";
        string _textTemplatePath = Application.dataPath + "/UHelper/Editor/Templates";

        if(!Directory.Exists(_customConfigPath)){
            Directory.CreateDirectory(_customConfigPath);
        }
        
        string _dstResPath = Path.Combine(_customConfigPath,"resources.json");
        if(!File.Exists(_dstResPath)){
            File.Copy(Path.Combine(_uhelperConfigPath,"res.json"),_dstResPath);
        }

        string _dstUIPath = Path.Combine(_customConfigPath,"uis.json");
        if(!File.Exists(_dstUIPath)){
            File.Copy(Path.Combine(_uhelperConfigPath,"ui.json"),_dstUIPath);
        }

        string _dstAssembliesConfigPath = Path.Combine(_customConfigPath, "assemblies.json");
        if(!File.Exists(_dstAssembliesConfigPath)){
            File.Copy(Path.Combine(_textTemplatePath,"AssembliesTemplate.txt"),_dstAssembliesConfigPath);
        }

        // 做一些项目结构
        string _scriptDir = Path.Combine(Application.dataPath, "Develop/Scripts");
        string _uiScriptsDir = Path.Combine(_scriptDir,"UI");    // 存放UI脚本
        string _artAssetsDir = Path.Combine(Application.dataPath, "ArtAssets");             // 存放美工资源

        if(!Directory.Exists(_uiScriptsDir)){
            Directory.CreateDirectory(_uiScriptsDir);
        }

        if(!Directory.Exists(_artAssetsDir)){
            Directory.CreateDirectory(_artAssetsDir);
        }

        string _dstAssemblyPath = Path.Combine(_scriptDir,"MainGame.asmdef");
        if(!File.Exists(_dstAssemblyPath)){
            File.Copy(Path.Combine(_textTemplatePath,"MainGameAssembly.txt"),_dstAssemblyPath);
        }
        
        AssetDatabase.Refresh();
        Debug.Log("UHelper initalize completed.");
       //AssetDatabase.LoadAssetAtPath()
       //MonoScript _uhelperScript = MonoScript.FromMonoBehaviour(UHelper.UHelperEntry);
   }
}
