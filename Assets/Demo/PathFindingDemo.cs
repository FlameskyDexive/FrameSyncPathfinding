using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PathFindingDemo : MonoBehaviour
{
    private Transform wallRoot;
    private Transform playersRoot;
    private Transform robotRoot;
    private Transform pointRoot;

    // Use this for initialization
    void Start()
    {
        wallRoot = GameObject.Find("Wall").transform;
        playersRoot = GameObject.Find("Players").transform;
        robotRoot = GameObject.Find("Robots").transform;
        pointRoot = GameObject.Find("Points").transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 90, 26), "关闭墙1"))
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
        }
    }

    private void SetTarget(int index)
    {
        for (int i = 0; i < playersRoot.childCount; i++)
        {
            //playersRoot.GetChild(i).GetComponent<AIDestinationSetter>().target = pointRoot.GetChild(index);
        }
    }
}
