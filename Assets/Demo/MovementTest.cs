using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    private Transform wallRoot;
    private Transform playersRoot;
    private Transform robotRoot;
    private Transform pointRoot;

    private Transform dynamicRoot;

    private GameObject ball;
    Seeker seeker;
    Path path;

    private GUIStyle style;
    private GUISkin skin;

    private string fileName;


    void Awake()
    {
        //Debug.Log("---1---" + gameObject.isStatic);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.runInBackground = true;
        Application.targetFrameRate = 30;
    }

    // Use this for initialization
    void Start()
    {
        //Debug.Log("---2---" + gameObject.isStatic);
        style = new GUIStyle()
        {
            fontSize = 22,
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState() { textColor = Color.white }
        };
        //dynamicRoot = GameObject.Find("DynamicObstacles").transform;
        //seeker = GetComponent<Seeker>();
        /*wallRoot = GameObject.Find("Wall").transform;
        playersRoot = GameObject.Find("Players").transform;
        robotRoot = GameObject.Find("Robots").transform;*/
        pointRoot = GameObject.Find("Points").transform;
        ball = GameObject.Find("Ball");
        
        
        //获取当前点所在的图形节点
        GraphNode startNode = AstarPath.active.GetNearest((VInt3)pointRoot.GetChild(0).position).node;
        var other = startNode as TriangleMeshNode;
        GameObject go4 = Instantiate(ball);
        go4.transform.position = (Vector3)startNode.position;
        Debug.Log("--0--" + startNode.position);
        GraphHitInfo hit = new GraphHitInfo();
        var graph = AstarData.GetGraph(startNode) as NavmeshBase;
        bool canReach = graph.Linecast((VInt3) pointRoot.GetChild(0).position, (VInt3) pointRoot.GetChild(2).position, startNode, out hit);
        Debug.Log($"---hit point--{hit.point}--{canReach}");
        GameObject go5 = Instantiate(ball);
        go5.transform.position = (Vector3)hit.point;
        //seeker.StartPath((VInt3)pointRoot.GetChild(0).position, (VInt3)pointRoot.GetChild(2).position, OnPathComplete);
    }
    

    private int singleGroupPathId = 0;
    //寻路结束;
    public void OnPathComplete(Path p)
    {
        //UnityEngine.Debug.Log("OnPathComplete error = " + p.error);
        if (!p.error)
        {
            path = p;
            UnityEngine.Debug.Log("-1-" + p.path[0].position);
            UnityEngine.Debug.Log("-2-" + p.vectorPath[0]);
            GameObject go3 = Instantiate(ball);
            go3.transform.position = (Vector3)p.path[0].position;
            GameObject go4 = Instantiate(ball);
            go4.transform.position = (Vector3)p.vectorPath[0];
            //PositionsLog(p.vectorPath);
        }

        //StartCoroutine(NextPathfinding());
        /*for (int index = 0; index < path.vectorPath.Count; index++)
        {
            //UnityEngine.Debug.Log(gameObject.name + "-path.vectorPath[" + index + "]=" + path.vectorPath[index]);
        }*/
    }
    
    

    public void SeekPath(VInt3 from, VInt3 to)
    {
        seeker.StartPath(from, to, OnPathComplete);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(0, 0, 30, 20), (1/Time.smoothDeltaTime).ToString("f2"));
    }
}
