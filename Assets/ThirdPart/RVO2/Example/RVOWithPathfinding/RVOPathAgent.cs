using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using Random = System.Random;
using RVO2;

public class RVOPathAgent : MonoBehaviour
{
    [HideInInspector] public int sid = -1;

    /** Random number generator. */
    private Random m_random = new Random();

    private Vector3 targetpos;

    private Seeker seeker;
    private bool isPathFound = false;
    //当前寻路ID
    private int curPathId = 0;  
    //当前寻路点列表
    private List<VInt3> paths = new List<VInt3>();
    //定义碰撞信息，一个角色定义一次
    private GraphHitInfo hit = new GraphHitInfo();

    // Use this for initialization
    void Start()
    {
        targetpos = -transform.position;

        seeker = gameObject.GetComponent<Seeker>();
        seeker.StartPath(transform.position, -transform.position, OnCompleted);
    }

    // Update is called once per frame
    void Update()
    {
        //寻路未完成则不执行移动
        if (!isPathFound)
        {
            return;
        }
        //到最后一个寻路点之前，都会执行
        if (curPathId < paths.Count)
        {
            if (sid >= 0)
            {
                VInt3 pos = (VInt3)Simulator.Instance.getAgentPosition(sid);
                Vector2 vel = (Vector2)(VInt2)Simulator.Instance.getAgentPrefVelocity(sid);
                /*if (PathfindingUtility.isCollide(transform.position, pos, out hit))
                {
                    pos = hit.point;
                }*/
                transform.position = new Vector3(pos.x / 1000f, transform.position.y, pos.z / 1000f);
                if (Math.Abs(vel.x) > 0.01f && Math.Abs(vel.y) > 0.01f)
                    transform.forward = new Vector3(vel.x, 0, vel.y).normalized;
            }

            Simulator.Instance.setAgentPrefVelocity(sid, VInt2.zero);

            //KInt2 goalVector = GameMainManager.Instance.mousePosition - Simulator.Instance.getAgentPosition(sid);//GameMainManager.Instance.mousePosition
            VInt2 goalVector = (VInt2)targetpos - Simulator.Instance.getAgentPosition(sid);//GameMainManager.Instance.mousePosition
            /*if (((VInt2) goalVector).sqrMagnitudeLong < 1000)
            {
                return;
            }*/
            if (RVOMath.absSq((KInt2)goalVector) > 1)
            {
                goalVector = (VInt2)RVOMath.normalize((KInt2)goalVector);
            }
            else
            {
                //已经到达当前的寻路终点，将目标替换成下一个点，当前路径点id也+1
                if (curPathId < paths.Count - 1)
                {
                    curPathId++;
                    targetpos = paths[curPathId];
                }
                return;
            }

            Simulator.Instance.setAgentPrefVelocity(sid, goalVector);

            /* Perturb a little to avoid deadlocks due to perfect symmetry. */
            /*float angle = (float) m_random.NextDouble()*2.0f*(float) Math.PI;
            float dist = (float) m_random.NextDouble()*0.0001f;

            Simulator.Instance.setAgentPrefVelocity(sid, Simulator.Instance.getAgentPrefVelocity(sid) +
                                                         dist*
                                                         new KInt2((float) Math.Cos(angle), (float) Math.Sin(angle)));*/
            /*Simulator.Instance.setAgentPrefVelocity(sid, Simulator.Instance.getAgentPrefVelocity(sid) +
                                                         (VInt2)(dist*
                                                         new KInt2((float) Math.Cos(angle), (float) Math.Sin(angle))));*/
        }

    }

    void OnCompleted(Path path)
    {
        Debug.Log("--agent--" + transform.GetSiblingIndex() + "---" + path.vectorPath.Count);
        paths = path.vectorPath;
        targetpos = paths[curPathId];
        isPathFound = true;
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(240 / 255f, 213 / 255f, 30 / 255f);
        Gizmos.DrawWireSphere(transform.position, Simulator.Instance.getAgentRadius(this.sid).floatvalue);
        

    }
#endif
}
