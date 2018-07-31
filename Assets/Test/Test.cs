using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;


public class Test : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Int3 int3 = new Int3(1, 1, 1);
        Int3 int3_2 = new Int3(1, 1, 1);
        Debug.Log("--加法--" + (int3 + int3_2));
        Debug.Log("--减法--" + (int3 - int3_2));
        Debug.Log("--点乘和--" + Int3.Dot(int3, int3_2));
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
