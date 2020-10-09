﻿using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace UHelper
{
public class CodeTemplateGenerator
{
       /// Inherits from EndNameAction, must override EndNameAction.Action
       public class DoCreateCodeFile : EndNameEditAction
       {
           public override void Action (int instanceId, string pathName, string resourceFile)
           {
               Object o = CreateScript(pathName, resourceFile);
               ProjectWindowUtil.ShowCreatedAsset (o);
           }
       }

    private const string REPLACABLE_NAME_TAG = "##CLASSNAME##";              ///< <summary>NAME's replacement tag.</summary>
    private const string REPLACABLE_TABSPACE_TAG = "##TABSPACE##";      ///< <summary>TABSPACE's replacement tag.</summary>

    /// <summary>C#'s Script Icon [The one MonoBhevaiour Scripts have].</summary>
    private static Texture2D scriptIcon = (EditorGUIUtility.IconContent ("cs Script Icon").image as Texture2D);

    /// <summary>Creates a new C# Class.</summary>
    [MenuItem("Assets/Create/UHelper/SceneScript", priority = 1)]
    [MenuItem("UHelper/Create/SceneScript", priority=1)]
    private static void CreateSceneScript()
    {
        CreateFromTemplate
        (
            "NewSceneScript.cs",                                                // Class's temporal name.
            @"Assets\UHelper\Editor\Templates\SceneScriptTemplate.txt"          // Template's path.
        );
    }

    [MenuItem("Assets/Create/UHelper/UIScript", priority = 1)]
    [MenuItem("UHelper/Create/UIScript", priority = 1)]
    private static void CreateUIScript()
    {
        CreateFromTemplate
        (
            "NewUI.cs",
            @"Assets/UHelper/Editor/Templates/UIScriptTemplate.txt"
        );
    }

    [MenuItem("Assets/Create/UHelper/ConfigScript", priority = 1)]
    [MenuItem("UHelper/Create/ConfigScript", priority = 1)]
    private static void CreateConfigScript()
    {
        CreateFromTemplate
        (
            "NewConfig.cs",
            @"Assets/UHelper/Editor/Templates/ConfigScriptTemplate.txt"
        );
    }

    /// <summary>Creates Script from Template's path.</summary>
    internal static UnityEngine.Object CreateScript(string pathName, string templatePath)
    {
        /// Subtract spaces [" "].
        string className = NormalizeClassName(Path.GetFileNameWithoutExtension(pathName));
        string templateText = string.Empty;

        UTF8Encoding encoding = new UTF8Encoding (true, false);

        if (File.Exists(templatePath))
        {
            /// Read procedures.
            StreamReader reader = new StreamReader(templatePath);
            templateText = reader.ReadToEnd();
            reader.Close();

            templateText = templateText.Replace(REPLACABLE_NAME_TAG, className);
            templateText = templateText.Replace(REPLACABLE_TABSPACE_TAG, string.Empty);
            /// You can replace as many tags you make on your templates, just repeat Replace function
            /// e.g.:
            /// templateText = templateText.Replace("#NEWTAG#", "MyText");

            /// Write procedures.
            StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
            writer.Write (templateText);
            writer.Close();

            AssetDatabase.ImportAsset(pathName);            
            return AssetDatabase.LoadAssetAtPath (pathName, typeof(Object));
        }
        else
        {
            Debug.LogError(string.Format("The template file was not found: {0}", templatePath));
            return null;
        }
    }

    /// <summary>Creates a new code file from a template file.</summary>
    /// <param name="initialName">The initial name to give the file in the UI</param>
    /// <param name="templatePath">The full path of the template file to use</param>
    public static void CreateFromTemplate(string initialName, string templatePath)
    {
        ProjectWindowUtil.StartNameEditingIfProjectWindowExists
        (
            0,
            ScriptableObject.CreateInstance<DoCreateCodeFile>(),
            initialName,
            scriptIcon,
            templatePath
        );
    }

    /// <summary>Subtracts white spaces [" "] for Class's name.</summary>
    private static string NormalizeClassName(string fileName)
    {
        return fileName.Replace(" ", string.Empty);
    }
}

}