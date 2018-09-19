using System.Collections;
using System.Collections.Generic;
using Lean;
using RVO2;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class RVOPathManager : SingletonBehaviour<RVOPathManager>
{
    private GUIStyle style;
    private GUISkin skin;

    public GameObject agentPrefab;
    public GameObject obstacles;

    public bool draw;

    [HideInInspector] public Vector2 mousePosition;

    private Plane m_hPlane = new Plane(Vector3.up, Vector3.zero);
    private Dictionary<int, RVOPathAgent> m_agentMap = new Dictionary<int, RVOPathAgent>();

    public Vector3 goalOffset = Vector3.zero;
    public int agentCount = 20;
    public float ringSize = 100;

    public KInt neighborDist = 15;
    public int maxNeighbors = 10;
    public KInt timeHorizon = 5;
    public KInt timeHorizonObst = 5;
    public KInt radius = 2;
    public KInt maxSpeed = 2;
    public KInt2 velocity = new KInt2(2, 2);

    void Awake()
    {
        Application.targetFrameRate = 240;
    }

    // Use this for initialization
    void Start()
    {
        KInt timestep = 0.25f;
        Simulator.Instance.setTimeStep(timestep);
        //设置单线程，true:Unity主线程工作，false:开启多线程
        Simulator.Instance.SetSingleTonMode(true);
        Simulator.Instance.setAgentDefaults(15, 10, 5, 5, 2, 2, KInt2.zero);

        for (int i = 0; i < agentCount; i++)
        {
            float angle = ((float)i / agentCount) * (float)System.Math.PI * 2;

            Vector3 pos = new Vector3((float)System.Math.Cos(angle), 0, (float)System.Math.Sin(angle)) * ringSize;
            Vector3 antipodal = -pos + goalOffset;

            int sid = Simulator.Instance.addAgent((KInt2)pos, neighborDist, maxNeighbors, timeHorizon, timeHorizonObst, radius, maxSpeed, velocity);

            if (sid >= 0)
            {
                GameObject go = LeanPool.Spawn(agentPrefab, new Vector3(pos.x, 0, pos.y), Quaternion.Euler(0, angle + 180, 0));
                go.transform.parent = transform;
                go.transform.position = pos;
                RVOPathAgent ga = go.GetComponent<RVOPathAgent>();
                Assert.IsNotNull(ga);
                ga.sid = sid;
                m_agentMap.Add(sid, ga);
            }
        }

        Simulator.Instance.SetNumWorkers(0);
        SetObstacles();
        Simulator.Instance.processObstacles();

        style = new GUIStyle()
        {
            fontSize = 26,
            alignment = TextAnchor.MiddleCenter,
            normal = new GUIStyleState() { textColor = Color.white }
        };
    }

    private void SetObstacles()
    {
        BoxCollider[] boxColliders = obstacles.GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < boxColliders.Length; i++)
        {
            float minX = boxColliders[i].transform.position.x -
                         boxColliders[i].size.x * boxColliders[i].transform.lossyScale.x * 0.5f;
            float minZ = boxColliders[i].transform.position.z -
                         boxColliders[i].size.z * boxColliders[i].transform.lossyScale.z * 0.5f;
            float maxX = boxColliders[i].transform.position.x +
                         boxColliders[i].size.x * boxColliders[i].transform.lossyScale.x * 0.5f;
            float maxZ = boxColliders[i].transform.position.z +
                         boxColliders[i].size.z * boxColliders[i].transform.lossyScale.z * 0.5f;

            IList<KInt2> obstacle = new List<KInt2>();
            obstacle.Add(new KInt2(maxX, maxZ));
            obstacle.Add(new KInt2(minX, maxZ));
            obstacle.Add(new KInt2(minX, minZ));
            obstacle.Add(new KInt2(maxX, minZ));
            Simulator.Instance.addObstacle(obstacle);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        UnityEngine.Profiling.Profiler.BeginSample("DoStep");
        Simulator.Instance.doStep();
        UnityEngine.Profiling.Profiler.EndSample();
    }

    void OnDrawGizmos()
    {
        if (draw)
            Simulator.Instance.DrawObstacles();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width - 80, 0, 100, 30), (/*1 / */Time.smoothDeltaTime * 1000).ToString("f2") + " ms");
        /*if (GUI.Button(new Rect(Screen.width - 100, 30, 100, 30), "WithObstacles"))
        {
            SceneManager.LoadScene("example");
        }*/
    }
}
