using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class GSeekerTest : MonoBehaviour
{
    private GSeeker seeker = new GSeeker();
    private Transform pointsRoot;
    //private GameObject ball;
    private GTileHandlerHelper cutHelper;

    GNavmeshCut cut;
    public Transform cutcube;

    // Use this for initialization
    void Start()
    {
        pointsRoot = GameObject.Find("Points").transform;
        seeker.StartPath(pointsRoot.GetChild(0).position, pointsRoot.GetChild(1).position, OnPathComplete);
        //cutcube = GameObject.Find("CutCube").transform;
        cutHelper = GameObject.Find("DynamicManager").GetComponent<GTileHandlerHelper>();
        cut = new GNavmeshCut();
    }

    void OnPathComplete(Path p)
    {
        Debug.Log("-path complete, path count-" + p.vectorPath.Count);
        for (int i = 0; i < p.vectorPath.Count; i++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.position = p.vectorPath[i];
        }
    }
    
    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 150, 30), "ShowDynamicObstacle"))
        {
            cut.InitNavmeshCut(cutcube.position, 2, 6, 2);
            cut.OnEnable();
        }
        if (GUI.Button(new Rect(0, 40, 150, 30), "HideDynamicObstacle"))
        {
            cut.OnDisable();
        }
    }

    public void OnDrawGizmos()
    {
        seeker.OnDrawGizmos();
    }
}
