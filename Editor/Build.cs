using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace UHelper
{
    

public class BuildUHelper
{

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuildProject)
    {
        string _projectDir = Directory.GetParent(Application.dataPath).FullName;
        string _devConfigDir = Path.Combine(_projectDir, "Configs");
        
        string _buildRootDir = Directory.GetParent(pathToBuildProject).FullName;
        string _buildConfigDir = Path.Combine(_buildRootDir, "Configs");

        Utility.DirectoryCopy(_devConfigDir, _buildConfigDir, true);


        

    }

    



}



}