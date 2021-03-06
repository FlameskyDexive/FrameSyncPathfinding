﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RVO2;

public class WWZAgent : MonoBehaviour
{
    [HideInInspector] public int sid = -1;

    /** Random number generator. */
    private Random m_random = new Random();

    private Vector3 targetpos;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (sid >= 0)
        {
            Vector2 pos = Simulator.Instance.getAgentPosition(sid);
            Vector2 vel = Simulator.Instance.getAgentPrefVelocity(sid);
            transform.position = new Vector3(pos.x, transform.position.y, pos.y);
            /*if (Math.Abs(vel.x) > 0.01f && Math.Abs(vel.y) > 0.01f)
                transform.forward = new Vector3(vel.x, 0, vel.y).normalized;*/
        }

        if (!Input.GetMouseButton(1))
        {
            Simulator.Instance.setAgentPrefVelocity(sid, VInt2.zero);
            return;
        }

        VInt2 goalVector = (VInt2)WWZTest.Instance.mousePosition - Simulator.Instance.getAgentPosition(sid);//GameMainManager.Instance.mousePosition
        if (RVOMath.absSq((KInt2)goalVector) > 1)
        {
            goalVector = (VInt2)RVOMath.normalize((KInt2)goalVector);
        }

        Simulator.Instance.setAgentPrefVelocity(sid, goalVector);

        /* Perturb a little to avoid deadlocks due to perfect symmetry. */
        /*float angle = (float) m_random.NextDouble()*2.0f*(float) Math.PI;
        float dist = (float) m_random.NextDouble()*0.0001f;

        Simulator.Instance.setAgentPrefVelocity(sid, Simulator.Instance.getAgentPrefVelocity(sid) +
                                                     dist*
                                                     new KInt2((float) Math.Cos(angle), (float) Math.Sin(angle)));*/
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(240 / 255f, 213 / 255f, 30 / 255f);
        Gizmos.DrawWireSphere(transform.position, Simulator.Instance.getAgentRadius(this.sid).floatvalue);

        KInt2 point = KInt2.zero;
        if (Simulator.Instance.ClosedObstaclePoint(sid, ref point))
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, new Vector3(point.x, 0, point.y));
        }
        /*if (Simulator.Instance.isInEdge(sid))
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
