using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Lean;
using RVO2;
using UnityEngine;
using UnityEngine.Assertions;

public class WWZTest : SingletonBehaviour<WWZTest>
{
    private string path;
    private string csv;

    public GameObject agentPrefab;

    public bool draw;

    [HideInInspector] public Vector2 mousePosition;

    private Plane m_hPlane = new Plane(Vector3.up, Vector3.zero);
    private Dictionary<int, GameAgent> m_agentMap = new Dictionary<int, GameAgent>();

    public int cnt = 1;
    //public Transform root;

    // Use this for initialization
    void Start()
    {
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.SetSingleTonMode(true);
        Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.0f, 2.0f, KInt2.zero);
        //TextAsset csvAsset = 
        Load(asset);
        SpawnMapObstacles();
    }

    public KdtreeAsset asset;
    

    public void Load(KdtreeAsset treeasset)
    {
        if (treeasset != null)
        {
            float time = Time.realtimeSinceStartup;
            Simulator.Instance.CreateKdtreeFromAsset(treeasset);
            Debug.Log("--Load asset--" + (Time.realtimeSinceStartup - time).ToString("f6"));
            Debug.Log(Simulator.Instance.GetObstacles().Count);
        }
    }

    private void SpawnMapObstacles()
    {
        if(asset == null)
        { return;}

        float time = Time.realtimeSinceStartup;
        path = Application.streamingAssetsPath + "/" + asset.name + ".csv";
        if (File.Exists(path))
        {
            csv = File.ReadAllText(path);
        }
        Debug.Log("--1--" + (Time.realtimeSinceStartup - time).ToString("f6"));
        string[] lines = csv.Split('\n');
        int x = 0, z = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                string xx = lines[i];
                x = Convert.ToInt32(xx.Split(',')[0]);
                z = Convert.ToInt32(xx.Split(',')[1]);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.transform.SetParent(transform);
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                go.transform.localRotation = Quaternion.Euler(90, 0, 0);
                go.transform.position = new Vector3((x + 250)/ 1000f, 0, (z + 250) / 1000f);

                //if(i < 500)
                /*{ 
                    IList<KInt2> obstacle = new List<KInt2>();
                    obstacle.Add(KInt2.ToInt2(x, z));
                    obstacle.Add(KInt2.ToInt2(x + 250, z));
                    obstacle.Add(KInt2.ToInt2(x, z + 250));
                    obstacle.Add(KInt2.ToInt2(x + 250, z + 250));
                    Simulator.Instance.addObstacle(obstacle);
                }*/
            }
        }
        //导出序列化障碍部分已经放到editor模式，
        //Simulator.Instance.processObstacles();
        Debug.Log("--2--" + (Time.realtimeSinceStartup - time).ToString("f6"));
        Debug.Log("----csv count-----" + lines.Length);
        Debug.Log("----obc count-----" + Simulator.Instance.GetObstacles().Count);
    }

    private void UpdateMousePosition()
    {
        Vector3 position = Vector3.zero;
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (m_hPlane.Raycast(mouseRay, out rayDistance))
            position = mouseRay.GetPoint(rayDistance);

        mousePosition.x = position.x;
        mousePosition.y = position.z;
    }

    void DeleteAgent()
    {
        int agentNo = Simulator.Instance.queryNearAgent((KInt2)mousePosition, 1.5f);
        if (agentNo == -1 || !m_agentMap.ContainsKey(agentNo))
            return;

        Simulator.Instance.delAgent(agentNo);
        LeanPool.Despawn(m_agentMap[agentNo].gameObject);
        m_agentMap.Remove(agentNo);
    }

    public float neighborDist = 15;
    public int maxNeighbors = 10;
    public float timeHorizon = 5;
    public float timeHorizonObst = 5;
    public float radius = 2;
    public float maxSpeed = 2;
    public KInt2 velocity;


    void CreatAgent()
    {
        int sid = Simulator.Instance.addAgent((KInt2)mousePosition, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, velocity);
        if (sid >= 0)
        {
            GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(mousePosition.x, 0, mousePosition.y), Quaternion.identity);
            GameAgent ga = go.GetComponent<GameAgent>();
            Assert.IsNotNull(ga);
            ga.sid = sid;
            m_agentMap.Add(sid, ga);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateMousePosition();
        if (Input.GetMouseButtonUp(0))
        {
            if (Input.GetKey(KeyCode.Delete))
            {
                DeleteAgent();
            }
            else
            {
                CreatAgent();
            }
        }

        Simulator.Instance.doStep();

    }

    void OnDrawGizmos()
    {
        if (draw)
            Simulator.Instance.DrawObstacles();
    }
}
