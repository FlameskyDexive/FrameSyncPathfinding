using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using RVO2;
//using Vector2 = RVO.Vector2;

public class GameAgent : MonoBehaviour
{
    [HideInInspector] public int sid = -1;

    /** Random number generator. */
    private Random m_random = new Random();

    private Vector3 targetpos;
    // Use this for initialization
    void Start()
    {
        targetpos = -transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (sid >= 0)
        {
            Vector2 pos = (Vector2)(VInt2)Simulator.Instance.getAgentPosition(sid);
            Vector2 vel = (Vector2)(VInt2)Simulator.Instance.getAgentPrefVelocity(sid);
            transform.position = new Vector3(pos.x, transform.position.y, pos.y);
            if (Math.Abs(vel.x) > 0.01f && Math.Abs(vel.y) > 0.01f)
                transform.forward = new Vector3(vel.x, 0, vel.y).normalized;
        }

        Simulator.Instance.setAgentPrefVelocity(sid, VInt2.zero);

        //KInt2 goalVector = GameMainManager.Instance.mousePosition - Simulator.Instance.getAgentPosition(sid);//GameMainManager.Instance.mousePosition
        VInt2 goalVector = (VInt2)targetpos- Simulator.Instance.getAgentPosition(sid);//GameMainManager.Instance.mousePosition
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

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(240 / 255f, 213 / 255f, 30 / 255f);
        Gizmos.DrawWireSphere(transform.position , Simulator.Instance.getAgentRadius(this.sid).floatvalue);

        /*KInt2 point =  KInt2.zero;
        if(Simulator.Instance.ClosedObstaclePoint(sid,ref point))
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, new Vector3(point.x,0, point.y));
        }
        if(Simulator.Instance.isInEdge(sid))
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;

            UnityEditor.Handles.Label(transform.position, ("最近距离" + Simulator.Instance.ClosedEdgeDist(sid)) + " 在角落 " + Simulator.Instance.isInEdge(sid), style);
        }
        else
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;
            UnityEditor.Handles.Label(transform.position, ("最近距离" + Simulator.Instance.ClosedEdgeDist(sid)) + " 在角落 " + Simulator.Instance.isInEdge(sid), style);
        }*/
        
    }
#endif
}