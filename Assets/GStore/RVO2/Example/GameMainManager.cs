﻿using System;
using System.Collections;
using System.Collections.Generic;
using Lean;
using RVO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Comparers;
using Random = System.Random;
using RVO2;
using Pathfinding.Examples;

public class GameMainManager : SingletonBehaviour<GameMainManager>
{
    public GameObject agentPrefab;

    public bool draw;

    [HideInInspector] public Vector2 mousePosition;

    private Plane m_hPlane = new Plane(Vector3.up, Vector3.zero);
    private Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();

    public Vector3 goalOffset = Vector3.zero;
    public int cnt=60;
    public float ringSize = 100;

    // Use this for initialization
    void Start()
    {
        KInt timestep = 0.25f;
        Simulator.Instance.setTimeStep(timestep);
        Simulator.Instance.SetSingleTonMode(true);
        Simulator.Instance.setAgentDefaults(15, 10, 5, 5, 2, 2, KInt2.zero);
        
        /*for(int i =0; i < cnt;++i)
        {
            CreatAgent();
        }*/
        for (int i = 0; i < cnt; i++)
        {
            float angle = ((float)i / cnt) * (float)System.Math.PI * 2;

            Vector3 pos = new Vector3((float)System.Math.Cos(angle), 0, (float)System.Math.Sin(angle)) * ringSize;
            Vector3 antipodal = -pos + goalOffset;
            
            int sid = Simulator.Instance.addAgent((KInt2)pos, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, velocity);
            //UnityEngine.Debug.Log("--radius--" + ((KInt)radius) + "--timeH--" + ((KInt)timeHorizon));
            if (sid >= 0)
            {
                GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(pos.x, 0, pos.y), Quaternion.Euler(0, angle + 180, 0));
                go.transform.parent = transform;
                go.transform.position = pos;
                GameAgent ga = go.GetComponent<GameAgent>();
                Assert.IsNotNull(ga);
                ga.sid = sid;
                m_agentMap.Add(sid, ga);
            }
        }

        Simulator.Instance.SetNumWorkers(1);
    }
    
    public KInt neighborDist =15;
    public int maxNeighbors=10;
    public KInt timeHorizon = 5;
    public KInt timeHorizonObst =5;
    public KInt radius = 2;
    public KInt maxSpeed =2;
    public KInt2 velocity;

    

    // Update is called once per frame
    private void Update()
    {
        Simulator.Instance.doStep();
    }

    void OnDrawGizmos()
    {
        if (draw)
            Simulator.Instance.DrawObstacles();
    }
}