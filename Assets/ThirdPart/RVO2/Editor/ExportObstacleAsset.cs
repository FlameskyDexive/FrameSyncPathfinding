using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RVO2;
using UnityEditor;
using UnityEngine;

public class ExportObstacleAsset : EditorWindow
{

    [MenuItem("GStore/Tools/ExportObstacles2Asset")]
    private static void ShowWindow()
    {
        if (Application.isPlaying)
        {
            string file = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!file.EndsWith(".csv"))
            {
                EditorUtility.DisplayDialog("提示", "请确保您选中了一个有效的csv地图文件", "确定");
                return;
            }

            Debug.Log(Selection.activeObject.name + "\n" + file);
            EditorUtility.DisplayProgressBar("导出障碍————" + Selection.activeObject.name, "正在读取障碍并构建障碍树，此过程时间较长，请耐心等候", 0);
            ReadObstacle(file);
            EditorUtility.DisplayProgressBar("导出障碍————" + Selection.activeObject.name, "正在导出障碍到本地，请耐心等候", 0.9f);
            ExportObstacles(file.Replace(".csv", ".asset"));
            //ExportObstacles();
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "请运行游戏后点击此按钮", "确定");
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }


    static string csv;
    //单个格子边长
    private static int cellSize = 500;
    private static void ReadObstacle(string filePath)
    {
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.SetSingleTonMode(true);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.0f, 2.0f, KInt2.zero);
        float time = Time.realtimeSinceStartup;

        if (!File.Exists(filePath))
        {
            return;
        }
        csv = File.ReadAllText(filePath);
        Debug.Log("--1--" + (Time.realtimeSinceStartup - time).ToString("f6"));
        string[] lines = csv.Split('\n');
        int x = 0, z = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                string xx = lines[i];
                x = Convert.ToInt32(xx.Split(',')[0]);
                z = Convert.ToInt32(xx.Split(',')[1]);

                IList<KInt2> obstacle = new List<KInt2>();
                obstacle.Add(KInt2.ToInt2(x, z));
                obstacle.Add(KInt2.ToInt2(x + cellSize, z));
                obstacle.Add(KInt2.ToInt2(x, z + cellSize));
                obstacle.Add(KInt2.ToInt2(x + cellSize, z + cellSize));
                Simulator.Instance.addObstacle(obstacle);

            }
        }
        Simulator.Instance.processObstacles();
        Debug.Log("--2--" + (Time.realtimeSinceStartup - time).ToString("f6"));
        Debug.Log("----csv count-----" + lines.Length);
        Debug.Log("----obc count-----" + Simulator.Instance.GetObstacles().Count);
    }

    static void ExportObstacles(string assetPath)
    {
        if (!string.IsNullOrEmpty(assetPath))
        {
            KdtreeAsset node = ScriptableObject.CreateInstance<KdtreeAsset>();

            KdTree tree = Simulator.Instance.GetKdTree();
            IList<Obstacle> obstacles = Simulator.Instance.GetObstacles();
            node.CreateKdtree(tree, obstacles);

            AssetDatabase.CreateAsset(node, assetPath);
        }
    }
}
