using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;


public class Test : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        VInt3 vInt3 = new VInt3(1, 1, 1);
        VInt3 vInt32 = new VInt3(1, 1, 1);
        Debug.Log("--加法--" + (vInt3 + vInt32));
        Debug.Log("--减法--" + (vInt3 - vInt32));
        Debug.Log("--点乘和--" + VInt3.Dot(vInt3, vInt32));
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public class PathDebug
{
    static List<int> logList = new List<int>()
    {
        1,
        //2,
        //3,
        //4,
        5
    };
    public static void LogError(int type, string content)
    {
        if(logList.Contains(type))
            Debug.Log("type--" + type +"--" + content);
    }
}
