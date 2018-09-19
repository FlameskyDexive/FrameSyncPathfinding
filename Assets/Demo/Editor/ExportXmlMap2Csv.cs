using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class ExportXmlMap2Csv : EditorWindow
{

    private static List<Vector2Int> listObstacle = new List<Vector2Int>();
    private static List<Vector2Int> listBoundObstacle = new List<Vector2Int>();
    private static string path = "Assets/StreamingAssets/";

    [MenuItem("GStore/Tools/ExportXmlMap2Csv")]
    private static void ShowWindow()
    {
        Export2CsvMap();
    }

    static void Export2CsvMap()
    {
        DirectoryInfo direction = new DirectoryInfo(path);
        FileInfo[] files = direction.GetFiles("*.xml", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            Debug.Log(files[i].Name);
            EditorUtility.DisplayProgressBar("ExportXmlMap2Csv", files[i].Name, (float)i / files.Length);
            GetRVOObstacles(files[i].Name);
        }

        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 获取地图障碍的边界信息
    /// </summary>
    /// <param name="xmlNodeList"></param>
    private static void GetRVOObstacles( string fileName)
    {
        listObstacle.Clear();
        listBoundObstacle.Clear();
        float lastTime = Time.realtimeSinceStartup;


        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(path + fileName);
        XmlElement zElement = (XmlElement)xmlDocument.FirstChild;
        int mSizeX = Convert.ToInt32(zElement.GetAttribute("SizeX"));
        int mSizeY = Convert.ToInt32(zElement.GetAttribute("SizeY"));
        XmlNodeList xmlNodeList = xmlDocument.FirstChild.ChildNodes;

        //存储所有静态障碍点到字典
        foreach (XmlElement ele in xmlNodeList)
        {
            int obstacleId = Convert.ToInt32(ele.GetAttribute("obstacle"));
            if (obstacleId < 5000)
            {
                int tileX = Convert.ToInt32(ele.GetAttribute("tile_x"));
                int tileY = Convert.ToInt32(ele.GetAttribute("tile_y"));
                Vector2Int vector2Int = new Vector2Int(tileX, tileY);
                //dicObstacles.Add(tileX, tileY);
                listObstacle.Add(vector2Int);
                listBoundObstacle.Add(vector2Int);
            }
        }
        Debug.Log("---Obstacle Count---" + listObstacle.Count);
        for (int i = 0; i < listObstacle.Count; i++)
        {
            if (ContainVector2(listObstacle[i].x, listObstacle[i].y + 1)
                && ContainVector2(listObstacle[i].x, listObstacle[i].y - 1)
                && ContainVector2(listObstacle[i].x + 1, listObstacle[i].y)
                && ContainVector2(listObstacle[i].x - 1, listObstacle[i].y))
            {
                listBoundObstacle.Remove(listObstacle[i]);
            }
        }
        Debug.Log("---Bounds Obstacle Count---" + listBoundObstacle.Count);
        Debug.Log("---Bounds Spend Time---" + (Time.realtimeSinceStartup - lastTime).ToString("f6"));
        string content = string.Empty;
        VInt2 leftBottom = VInt2.zero;
        for (int i = 0; i < listBoundObstacle.Count; i++)
        {
            leftBottom = TilePosToVInt2(listBoundObstacle[i].x, listBoundObstacle[i].y);
            content += leftBottom.x + "," + leftBottom.y + "\n";
            // 创建地图块
            //AddTileHelper(listBoundObstacle[i].x, listBoundObstacle[i].y, "0", true);
        }
        string filePath = path + fileName.Replace(".xml", ".csv");
        File.WriteAllText(filePath, content, Encoding.UTF8);
    }
    /// <summary>
    /// 格子坐标转世界坐标整型，格子左下角坐标
    /// </summary>
    private static int singleSize = 500;
    public static VInt2 TilePosToVInt2(int tileX, int tileY)
    {
        return new VInt2(tileX * singleSize, tileY * singleSize);
    }

    private static bool ContainVector2(int x, int y)
    {
        for (int i = 0; i < listObstacle.Count; i++)
        {
            if (listObstacle[i].x == x && listObstacle[i].y == y)
            {
                return true;
            }
        }

        return false;
    }
}
