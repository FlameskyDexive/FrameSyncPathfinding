using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class Float2Int : EditorWindow
{

    [MenuItem("Tools/SetFloatTags")]
    private static void ShowWindow()
    {
        SetFloatTags();
    }
    
    private static string path = "Assets/AstarPathfindingProject/";
    static void SetFloatTags()
    {
        DirectoryInfo direction = new DirectoryInfo(path);
        FileInfo[] files = direction.GetFiles("*.cs", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].FullName.Contains("Int3.cs"))
            {
                continue;
            }
            string codes = File.ReadAllText(files[i].FullName);
            string[] codeLines = codes.Split('\n');
            for (int j = 0; j < codeLines.Length; j++)
            {
                if (codeLines[j].Contains("*/") || codeLines[j].Contains("/*"))
                {
                    continue;
                }
                if (codeLines[j].Contains("Vector2") || codeLines[j].Contains("Vector3") ||
                    codeLines[j].Contains("float")|| codeLines[j].Contains("double") || 
                    codeLines[j].Contains("Int3"))
                {
                    codeLines[j] = "//【整型转换】" + codeLines[j] + "\n" + codeLines[j];
                }
            }
            File.WriteAllLines(files[i].FullName, codeLines);
            //Debug.Log("----" + files[i] + "\n" + codeLines[0]);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
}
