﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using Pathfinding;
using UnityEngine;
using UnityEngine.Networking;
using Path = Pathfinding.Path;

public class PathFindingDemo : MonoBehaviour
{
    private Transform wallRoot;
    private Transform playersRoot;
    private Transform robotRoot;
    private Transform pointRoot;

    private GameObject ball;
    Seeker seeker;
    Path path;

    private GUIStyle style;
    private GUISkin skin;

    private string fileName;

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.runInBackground = true;
    }

    // Use this for initialization
    void Start()
    {
        style = new GUIStyle()
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState() { textColor = Color.white}
        };
        
        seeker = GetComponent<Seeker>();
        /*wallRoot = GameObject.Find("Wall").transform;
        playersRoot = GameObject.Find("Players").transform;
        robotRoot = GameObject.Find("Robots").transform;
        pointRoot = GameObject.Find("Points").transform;
        ball = GameObject.Find("Ball");*/

#if UNITY_EDITOR || UNITY_STANDALONE
        intervalTime = 0.11f;
        fileName = "PathLog_PC.txt";
#elif UNITY_IOS
        intervalTime = 0.2f;
        fileName = "PathLog_IOS.txt";
#elif UNITY_ANDROID
        intervalTime = 0.18f;
        fileName = "PathLog_Android.txt";
#endif

        SpawnPathPoint();
        /*string url = "http://119.23.130.26/";
        string str = "test for path finding";
        byte[] bs = System.Text.Encoding.UTF8.GetBytes(str);
        StartCoroutine(SendPost(url, str, bs));*/
    }

    /// <summary>
    /// 文本写入远程服务器，方便不同平台直接拿到log，后续配置
    /// </summary>
    /// <param name="url"></param>
    /// <param name="content"></param>
    /// <param name="bs"></param>
    /// <returns></returns>
    public IEnumerator SendPost(string url, string content, byte[] bs)
    {
        WWWForm form = new WWWForm();
        form.AddField("", "");
        form.AddBinaryData("", bs, "123.txt");
        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
        /*WWW www = new WWW(url, bs);
        yield return www;
        if (www.isDone)
        {
            Debug.Log(www.text);
        }*/
    }

    void SpawnPathPoint()
    {
        for (int i = 1; i <= 6; i++)
        {
            for (int j = 1; j <= 6; j++)
            {
                /*GameObject go = Instantiate(ball);
                go.transform.position = new Vector3(i * 15, 0, j * 15);
                GameObject go1 = Instantiate(ball);
                go1.transform.position = new Vector3(i * -15, 0, j * 15);
                GameObject go2 = Instantiate(ball);
                go2.transform.position = new Vector3(i * 15, 0, j * -15);
                GameObject go3 = Instantiate(ball);
                go3.transform.position = new Vector3(i * -15, 0, j * -15);*/
                listPaths.Add(new Int3(i * 23241, 2000, j * 25482));
                listPaths.Add(new Int3(i * -24573, 2000, j * 26644));
                listPaths.Add(new Int3(i * 23765, 2000, j * -25276));
                listPaths.Add(new Int3(i * -24687, 2000, j * -26878));
            }
        }

        content = "Path Points\n";
        for (int i = 0; i < listPaths.Count; i++)
        {
            content += listPaths[i] + "\n";
        }
        totalpathCount = listPaths.Count * (listPaths.Count + 1) / 2;
        content += "\n";
    }

    private bool isPathing = false;
    private int totalpathCount = 0;
    private float intervalTime = 0.15f;
    void OnGUI()
    {
#region Unuse
        /*if (GUI.Button(new Rect(0, 0, 90, 26), "关闭墙1"))
        {
            UnityEngine.Debug.Log("关闭墙1并重新生成网格导航");
            wallRoot.GetChild(0).gameObject.SetActive(false);
            AstarPath.active.Scan();
        }
        if (GUI.Button(new Rect(0, 30, 90, 26), "关闭墙2"))
        {
            UnityEngine.Debug.Log("关闭墙2并重新生成网格导航");
            wallRoot.GetChild(1).gameObject.SetActive(false);
            AstarPath.active.Scan();
        }
        if (GUI.Button(new Rect(0, 60, 90, 26), "关闭墙1&2"))
        {
            UnityEngine.Debug.Log("关闭墙1&2并重新生成网格导航");
            wallRoot.GetChild(0).gameObject.SetActive(false);
            wallRoot.GetChild(1).gameObject.SetActive(false);
            AstarPath.active.Scan();
        }
        if (GUI.Button(new Rect(0, 90, 90, 26), "开启墙1&2"))
        {
            UnityEngine.Debug.Log("关闭墙1&2并重新生成网格导航");
            wallRoot.GetChild(0).gameObject.SetActive(true);
            wallRoot.GetChild(1).gameObject.SetActive(true);
            AstarPath.active.Scan();
        }

        if (GUI.Button(new Rect(0, 120, 100, 26), "自动导航到点A"))
        {
            UnityEngine.Debug.Log("自动导航到点A");
            SetTarget(0);
        }

        if (GUI.Button(new Rect(0, 150, 100, 26), "自动导航到点B"))
        {
            UnityEngine.Debug.Log("自动导航到点B");
            SetTarget(1);
        }

        if (GUI.Button(new Rect(0, 180, 100, 26), "自动导航到点C"))
        {
            UnityEngine.Debug.Log("自动导航到点C");
            SetTarget(2);
        }

        if (GUI.Button(new Rect(0, 210, 100, 26), "自动导航到点D"))
        {
            UnityEngine.Debug.Log("自动导航到点D");
            SetTarget(3);
        }*/
#endregion
        
        /*if (GUI.Button(new Rect(Screen.width - 110, 260, 100, 26), "测试生成点"))
        {
            SpawnPathPoint();
        }*/
        if (isPathing)
        {
            GUI.Label(new Rect(Screen.width / 2 - 80, Screen.height/2, 200, 70), "On Finding The Path..\n-group- " + groupId + " -path id- " 
                + pathId + " -total path- " + totalpathCount+ "\n预计耗时 " + (totalpathCount * intervalTime).ToString("f0") + "s", style);
        }
        else
        {
            if (GUI.Button(new Rect(Screen.width - 190, 2, 180, 60), "SeekPaths-TotalGroup-" + listPaths.Count))
            {
                //Debug.Log("--start position--" + (Int3)startObj.position + "--end position--" + (Int3)endObj.position);
                isPathing = true;
                StartCoroutine(Seeking(listPaths.Count));
            }
        }
    }

    //寻路结束;
    public void OnPathComplete(Path p)
    {
        //UnityEngine.Debug.Log("OnPathComplete error = " + p.error);
        if (!p.error)
        {
            //currentWayPoint = 0;
            path = p;
            //stopMove = false;
            PositionsLog(p.vectorPath);
            //pathFindingTimes++;
        }

        /*for (int index = 0; index < path.vectorPath.Count; index++)
        {
            //UnityEngine.Debug.Log(gameObject.name + "-path.vectorPath[" + index + "]=" + path.vectorPath[index]);
        }*/
    }
    
    
    /// <summary>
    /// Log Test path finding points 
    /// </summary>
    //private int pathFindingTimes = 0;
    private int pathId = 0;
    string content = String.Empty;
    private void PositionsLog(List<Int3> p)
    {
        for (int i = 0; i < p.Count; i++)
        {
            string log = "-group-" + groupId +  "-path-" + pathId + "-pos-" + i + "-" + p[i];

            //Debug.Log(log);
            content += log + "\n";
        }

        content += "\n";
        pathId++;
    }
    
    private List<Int3> listPaths = new List<Int3>();
        int groupId = 0;
    IEnumerator Seeking(int id)
    {
        for (int i = groupId; i < listPaths.Count; i++)
        {
            SeekPath(listPaths[groupId], listPaths[i]);
            //Debug.Log("--current path id--" + groupId + "-from-" + listPaths[groupId]+ "-to-" + listPaths[i]);
            yield return new WaitForSeconds(intervalTime);
        }
            groupId++;
        if (groupId < listPaths.Count)
        {
            StartCoroutine(Seeking(groupId));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("Path Find Complete");
            string positionLog = PathHelper.AppHotfixResPath + "/" + fileName;
            File.WriteAllText(positionLog, content, Encoding.UTF8);
            isPathing = false;
        }
    }

    public void SeekPath(Int3 from, Int3 to)
    {
        seeker.StartPath(from, to, OnPathComplete);
    }

    private void SetTarget(int index)
    {
        for (int i = 0; i < playersRoot.childCount; i++)
        {
            //playersRoot.GetChild(i).GetComponent<AIDestinationSetter>().target = pointRoot.GetChild(index);
        }
    }
}
