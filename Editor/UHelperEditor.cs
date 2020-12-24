using System.Linq;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace UHelper
{

public class UHelperEditor : Editor
{
    const string sceneEntryName = "SceneEntry";

    [InitializeOnLoadMethod]
    public static void OnLoad(){
        EditorSceneManager.newSceneCreated += NewSceneCreatedCallback;
        EditorSceneManager.sceneSaved += SceneSaved;
    }

    private static void NewSceneCreatedCallback(Scene scene, NewSceneSetup setup, NewSceneMode mode){
        
    }

    private static void SceneSaved(Scene scene){
        CodeTemplateGenerator.CreateSceneScriptIfNotExists(scene.name);
    }

    [MenuItem("UHelper/Initialize",priority=0)]
    public static void CreateDefault()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        var _sceneEntry = EditorSceneManager.GetSceneByName(sceneEntryName);
        if(!_sceneEntry.IsValid()){
                _sceneEntry = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
                EditorSceneManager.SaveScene(_sceneEntry, string.Format("Assets/Scenes/{0}.unity",sceneEntryName));
        }else{
            var _activeScene = EditorSceneManager.GetActiveScene();
            if(_activeScene.name!=sceneEntryName){
                EditorSceneManager.LoadScene(sceneEntryName,LoadSceneMode.Single);
            }
        }
       
        // 1. 复制  UHelper.prefab
        Component[] _objs = FindObjectsOfType(Type.GetType("UHelper.UHelperEntry, UHelper, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")) as Component[];
        if(_objs.Length>1){
            _objs.Skip(1)
                .Select(_uhelper=>_uhelper.gameObject)
                .ToList()
                .ForEach(_uhelperGO=>{
                    Debug.Log("Destory UHelperEntry: " + _uhelperGO.name);
                    DestroyImmediate(_uhelperGO,true);
                });
        }else if(_objs.Length<=0){
            string _uhelperPrefabPath = @"Assets\UHelper\Resources\Prefabs\UHelper.prefab";
            UnityEngine.Object _uhelperPrefab = AssetDatabase.LoadAssetAtPath(_uhelperPrefabPath,typeof(GameObject));
            GameObject _newUHelper = PrefabUtility.InstantiatePrefab(_uhelperPrefab) as GameObject;
        }

        // 2.   复制 配置文件
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
        List<string> _frame_dirs = new List<string>{
            Path.Combine(Application.dataPath, "Develop/Scripts"),
            Path.Combine(Application.dataPath, "Develop/Scripts/UIs"),
            Path.Combine(Application.dataPath, "Develop/Scripts/Configs"),
            Path.Combine(Application.dataPath, "ArtAssets")
        };

        _frame_dirs.ForEach(_path=>{
            if(!Directory.Exists(_path)){
                Directory.CreateDirectory(_path);
            };
        });

        // 3.   创建程序集定义文件
        string _dstAssemblyPath = Path.Combine(Path.GetFullPath("Assets/Develop/Scripts"),"GameMain.asmdef");
        if(!File.Exists(_dstAssemblyPath)){
            File.Copy(Path.Combine(_textTemplatePath,"GameMainAssembly.txt"),_dstAssemblyPath);
        }
        
        AssetDatabase.Refresh();
        Debug.Log("UHelper framework initalize successful.");
       //AssetDatabase.LoadAssetAtPath()
       //MonoScript _uhelperScript = MonoScript.FromMonoBehaviour(UHelper.UHelperEntry);
   }
}



}


