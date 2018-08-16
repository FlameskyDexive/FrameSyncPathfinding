
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pathfinding;
using Debug = UnityEngine.Debug;
using Path = Pathfinding.Path;

public class AStarPlayer : MonoBehaviour
{
    //目标位置;
    Vector3 targetPosition;
    Seeker seeker;

    CharacterController characterController;
    //计算出来的路线;

    Path path;
   //移动速度;

    float playerMoveSpeed = 10f;
    //当前点
    int currentWayPoint = 0;
    bool stopMove = true;
    //Player中心点;

    float playerCenterY = 1.0f;
    // Use this for initialization

    private Transform startObj;
    private Transform endObj;
    private Transform pointRoot;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        playerCenterY = transform.localPosition.y;
        startObj = GameObject.Find("Sphere").transform;
        endObj = GameObject.Find("Sphere3").transform;
        pointRoot = GameObject.Find("Points").transform;
    }
    
    //寻路结束;

    public void OnPathComplete(Path p)
    {
        //UnityEngine.Debug.Log("OnPathComplete error = " + p.error);
        if (!p.error)
        {
            currentWayPoint = 0;
            path = p;
            stopMove = false;
        }

        PositionsLog(p.vectorPath);
        for (int index = 0; index < path.vectorPath.Count; index++)
        {
            //UnityEngine.Debug.Log(gameObject.name + "-path.vectorPath[" + index + "]=" + path.vectorPath[index]);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButtonDown(0))
        {

            RaycastHit hit;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
            {
                return;
            }

            if (!hit.transform)
            {
                return;
            }

            targetPosition = hit.point;// new Vector3(hit.point.x, transform.localPosition.y, hit.point.z);
            UnityEngine.Debug.Log("targetPosition=" + targetPosition);
            seeker.StartPath((VInt3)transform.position, (VInt3)targetPosition, OnPathComplete);
        }*/
        
    }



    /// <summary>
    /// Log Test path finding points 
    /// </summary>
    private int funnelIndex = 0;
    string content = String.Empty;
    private void PositionsLog(List<VInt3> p)
    {
        for (int i = 0; i < p.Count; i++)
        {
            string log = "---funnel index---" + funnelIndex + "---position---" + i + "---" + p[i];

            Debug.Log(log);
            content += log + "\n";
        }

        funnelIndex++;
        /*if (!File.Exists(PathHelper.AppHotfixResPath))
        {
            File.Create(PathHelper.AppHotfixResPath);
        }*/
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 300, 2, 250, 50), "Seek Path ABCD"))
        {
            //Debug.Log("--start position--" + (VInt3)startObj.position + "--end position--" + (VInt3)endObj.position);
            StartCoroutine(Seeking());
        }
        /*GUI.BeginScrollView(new Rect(0, 50, 700, 700), new Vector2(0, 0), new Rect(0, 50, 700, 700));
        for (int i = 0; i < UPPER; i++)
        {

        }
        GUI.Label(new Rect(0, 80, 700, 800), content);
        GUI.EndScrollView();*/
    }

    IEnumerator Seeking()
    {
        SeekPath(0, 1);
        yield return new WaitForSeconds(0.5f);
        SeekPath(0, 2);
        yield return new WaitForSeconds(0.5f);
        SeekPath(0, 3);
        yield return new WaitForSeconds(0.5f);
        SeekPath(1, 0);
        yield return new WaitForSeconds(0.5f);
        SeekPath(1, 2);
        yield return new WaitForSeconds(0.5f);
        SeekPath(1, 3);
        yield return new WaitForSeconds(0.5f);
        SeekPath(2, 0);
        yield return new WaitForSeconds(0.5f);
        SeekPath(2, 1);
        yield return new WaitForSeconds(0.5f);
        SeekPath(2, 3);
        yield return new WaitForSeconds(0.5f);
        SeekPath(3, 0);
        yield return new WaitForSeconds(0.5f);
        SeekPath(3, 1);
        yield return new WaitForSeconds(0.5f);
        SeekPath(3, 2);
        yield return new WaitForSeconds(0.5f);
        string positionLog = PathHelper.AppHotfixResPath + "/positionLog.txt";
        File.WriteAllText(positionLog, content, Encoding.UTF8);
    }

    public void SeekPath(int from, int to)
    {
         seeker.StartPath((VInt3)pointRoot.GetChild(from).position, (VInt3)pointRoot.GetChild(to).position, OnPathComplete);
    }

    /*void FixedUpdate()
    {
        if (path == null || stopMove)
        {
            return;
        }
        
        //根据Player当前位置和 下一个寻路点的位置，计算方向;
        Vector3 currentWayPointV = new Vector3(path.vectorPath[currentWayPoint].x, path.vectorPath[currentWayPoint].y + playerCenterY, path.vectorPath[currentWayPoint].z);
        Vector3 dir = (currentWayPointV - transform.position).normalized;
        
        //计算这一帧要朝着 dir方向 移动多少距离;
        dir *= playerMoveSpeed * Time.fixedDeltaTime;

        //计算加上这一帧的位移，是不是会超过下一个节点;
        float offset = Vector3.Distance(transform.localPosition, currentWayPointV);

        if (offset < 0.1f)
        {
            transform.localPosition = currentWayPointV;
            currentWayPoint++;
            if (currentWayPoint == path.vectorPath.Count)
            {
                stopMove = true;
                currentWayPoint = 0;
                path = null;
            }
        }
        else
        {
            if (dir.magnitude > offset)
            {
                Vector3 tmpV3 = dir * (offset / dir.magnitude);
                dir = tmpV3;
                currentWayPoint++;
                if (currentWayPoint == path.vectorPath.Count)
                {
                    stopMove = true;
                    currentWayPoint = 0;
                    path = null;
                }
            }
            transform.localPosition += dir;
        }

    }*/

}


