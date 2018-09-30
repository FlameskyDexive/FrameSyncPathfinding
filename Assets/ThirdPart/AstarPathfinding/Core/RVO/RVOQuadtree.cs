using UnityEngine;
using Pathfinding.RVO.Sampled;

namespace Pathfinding.RVO
{
    /** Quadtree for quick nearest neighbour search of rvo agents.
	 * \see Pathfinding.RVO.Simulator
	 */
    public class RVOQuadtree
    {
        const int LeafSize = 15;

        //GG
        //float maxRadius = 0;
        int maxRadius = 0;

        /** Node in a quadtree for storing RVO agents.
		 * \see Pathfinding.GraphNode for the node class that is used for pathfinding data.
		 */
        struct Node
        {
            public int child00;
            public int child01;
            public int child10;
            public int child11;
            public Agent linkedList;
            public byte count;

            /** Maximum speed of all agents inside this node */
            //GG
            //public float maxSpeed;
            public int maxSpeed;

            public void Add(Agent agent)
            {
                agent.next = linkedList;
                linkedList = agent;
            }

            /** Distribute the agents in this node among the children.
			 * Used after subdividing the node.
			 */
            //GG
            //public void Distribute(Node[] nodes, Rect r)
            public void Distribute(Node[] nodes, VRect r)
            {
                //GG
                //Vector2 c = r.center;
                VInt2 c = r.center;

                while (linkedList != null)
                {
                    Agent nx = linkedList.next;
                    if (linkedList.position.x > c.x)
                    {
                        if (linkedList.position.y > c.y)
                        {
                            nodes[child11].Add(linkedList);
                        }
                        else
                        {
                            nodes[child10].Add(linkedList);
                        }
                    }
                    else
                    {
                        if (linkedList.position.y > c.y)
                        {
                            nodes[child01].Add(linkedList);
                        }
                        else
                        {
                            nodes[child00].Add(linkedList);
                        }
                    }
                    linkedList = nx;
                }
                count = 0;
            }

            //GG
            //public float CalculateMaxSpeed(Node[] nodes, int index)
            public int CalculateMaxSpeed(Node[] nodes, int index)
            {
                if (child00 == index)
                {
                    // Leaf node
                    for (var agent = linkedList; agent != null; agent = agent.next)
                    {
                        maxSpeed = System.Math.Max(maxSpeed, agent.CalculatedSpeed);
                    }
                }
                else
                {
                    maxSpeed = System.Math.Max(nodes[child00].CalculateMaxSpeed(nodes, child00), nodes[child01].CalculateMaxSpeed(nodes, child01));
                    maxSpeed = System.Math.Max(maxSpeed, nodes[child10].CalculateMaxSpeed(nodes, child10));
                    maxSpeed = System.Math.Max(maxSpeed, nodes[child11].CalculateMaxSpeed(nodes, child11));
                }
                return maxSpeed;
            }
        }

        Node[] nodes = new Node[42];
        int filledNodes = 1;

        //GG
        //Rect bounds;
        VRect bounds;

        /** Removes all agents from the tree */
        public void Clear()
        {
            nodes[0] = new Node();
            filledNodes = 1;
            maxRadius = 0;
        }

        //GG
        //public void SetBounds(Rect r)
        public void SetBounds(VRect r)
        {
            bounds = r;
        }

        int GetNodeIndex()
        {
            if (filledNodes == nodes.Length)
            {
                var nds = new Node[nodes.Length * 2];
                for (int i = 0; i < nodes.Length; i++) nds[i] = nodes[i];
                nodes = nds;
            }
            nodes[filledNodes] = new Node();
            nodes[filledNodes].child00 = filledNodes;
            filledNodes++;
            return filledNodes - 1;
        }

        /** Add a new agent to the tree.
		 * \warning Agents must not be added multiple times to the same tree
		 */
        public void Insert(Agent agent)
        {
            int i = 0;
            //GG
            /*Rect r = bounds;
            Vector2 p = new Vector2(agent.position.x, agent.position.y);*/
            VRect r = bounds;
            VInt2 p = new VInt2(agent.position.x, agent.position.y);

            agent.next = null;

            //GG
            //maxRadius = System.Math.Max(agent.radius, maxRadius);
            maxRadius = System.Math.Max((int)agent.radius, maxRadius);

            int depth = 0;

            while (true)
            {
                depth++;

                if (nodes[i].child00 == i)
                {
                    // Leaf node. Break at depth 10 in case lots of agents ( > LeafSize ) are in the same spot
                    if (nodes[i].count < LeafSize || depth > 10)
                    {
                        nodes[i].Add(agent);
                        nodes[i].count++;
                        break;
                    }
                    else
                    {
                        // Split
                        Node node = nodes[i];
                        node.child00 = GetNodeIndex();
                        node.child01 = GetNodeIndex();
                        node.child10 = GetNodeIndex();
                        node.child11 = GetNodeIndex();
                        nodes[i] = node;

                        nodes[i].Distribute(nodes, r);
                    }
                }
                // Note, no else
                if (nodes[i].child00 != i)
                {
                    // Not a leaf node
                    //GG
                    //Vector2 c = r.center;
                    VInt2 c = r.center;
                    if (p.x > c.x)
                    {
                        if (p.y > c.y)
                        {
                            i = nodes[i].child11;
                            //GG
                            //r = Rect.MinMaxRect(c.x, c.y, r.xMax, r.yMax);
                            r = VRect.MinMaxRect(c.x, c.y, r.xMax, r.yMax);
                        }
                        else
                        {
                            i = nodes[i].child10;
                            //GG
                            //r = Rect.MinMaxRect(c.x, r.yMin, r.xMax, c.y);
                            r = VRect.MinMaxRect(c.x, r.yMin, r.xMax, c.y);
                        }
                    }
                    else
                    {
                        if (p.y > c.y)
                        {
                            i = nodes[i].child01;
                            //GG
                                //r = Rect.MinMaxRect(r.xMin, c.y, c.x, r.yMax);
                            r = VRect.MinMaxRect(r.xMin, c.y, c.x, r.yMax);
                        }
                        else
                        {
                            i = nodes[i].child00;
                            //GG
                            //r = Rect.MinMaxRect(r.xMin, r.yMin, c.x, c.y);
                            r = VRect.MinMaxRect(r.xMin, r.yMin, c.x, c.y);
                        }
                    }
                }
            }
        }

        public void CalculateSpeeds()
        {
            nodes[0].CalculateMaxSpeed(nodes, 0);
        }

        //GG
        //public void Query(Vector2 p, float speed, float timeHorizon, float agentRadius, Agent agent)
        public void Query(VInt2 p, long speed, long timeHorizon, long agentRadius, Agent agent)
        {
            new QuadtreeQuery
            {
                p = p,
                speed = speed,
                timeHorizon = timeHorizon,
                //GG
                //maxRadius = float.PositiveInfinity,
                maxRadius = int.MaxValue,
                agentRadius = agentRadius,
                agent = agent,
                nodes = nodes
            }.QueryRec(0, bounds);
        }

        struct QuadtreeQuery
        {
            //GG
            //public Vector2 p;
            public VInt2 p;
            //GG
            //public float speed, timeHorizon, agentRadius, maxRadius;
            public long speed, timeHorizon, agentRadius, maxRadius;
            public Agent agent;
            public Node[] nodes;

            //GG
            //public void QueryRec(int i, Rect r)
            public void QueryRec(int i, VRect r)
            {
                // Determine the radius that we need to search to take all agents into account
                // Note: the second agentRadius usage should actually be the radius of the other agents, not this agent
                // but for performance reasons and for simplicity we assume that agents have approximately the same radius.
                // Thus an agent with a very small radius may in some cases detect an agent with a very large radius too late
                // however this effect should be minor.
                var radius = System.Math.Min(System.Math.Max((nodes[i].maxSpeed + speed) * timeHorizon, agentRadius) + agentRadius, maxRadius);

                if (nodes[i].child00 == i)
                {
                    // Leaf node
                    for (Agent a = nodes[i].linkedList; a != null; a = a.next)
                    {
                        //GG
                        //float v = agent.InsertAgentNeighbour(a, radius * radius);
                        long v = agent.InsertAgentNeighbour(a, (int)(radius * radius));
                        // Limit the search if the agent has hit the max number of nearby agents threshold
                        if (v < maxRadius * maxRadius)
                        {
                            //GG
                            //maxRadius = Mathf.Sqrt(v);
                            maxRadius = IntMath.Sqrt(v);
                        }
                    }
                }
                else
                {
                    // Not a leaf node
                    //GG
                    //Vector2 c = r.center;
                    VInt2 c = r.center;
                    if (p.x - radius < c.x)
                    {
                        if (p.y - radius < c.y)
                        {
                            //GG
                            //QueryRec(nodes[i].child00, Rect.MinMaxRect(r.xMin, r.yMin, c.x, c.y));
                            QueryRec(nodes[i].child00, VRect.MinMaxRect(r.xMin, r.yMin, c.x, c.y));
                            radius = System.Math.Min(radius, maxRadius);
                        }
                        if (p.y + radius > c.y)
                        {
                            //GG
                            //QueryRec(nodes[i].child01, Rect.MinMaxRect(r.xMin, c.y, c.x, r.yMax));
                            QueryRec(nodes[i].child01, VRect.MinMaxRect(r.xMin, c.y, c.x, r.yMax));
                            radius = System.Math.Min(radius, maxRadius);
                        }
                    }

                    if (p.x + radius > c.x)
                    {
                        if (p.y - radius < c.y)
                        {
                            //GG
                            //QueryRec(nodes[i].child10, Rect.MinMaxRect(c.x, r.yMin, r.xMax, c.y));
                            QueryRec(nodes[i].child10, VRect.MinMaxRect(c.x, r.yMin, r.xMax, c.y));
                            radius = System.Math.Min(radius, maxRadius);
                        }
                        if (p.y + radius > c.y)
                        {
                            //GG
                            //QueryRec(nodes[i].child11, Rect.MinMaxRect(c.x, c.y, r.xMax, r.yMax));
                            QueryRec(nodes[i].child11, VRect.MinMaxRect(c.x, c.y, r.xMax, r.yMax));
                        }
                    }
                }
            }
        }

        public void DebugDraw()
        {
            //GG
            //DebugDrawRec(0, bounds);
            //DebugDrawRec(0, (Rect)bounds);
        }

        void DebugDrawRec(int i, Rect r)
        {
            Debug.DrawLine(new Vector3(r.xMin, 0, r.yMin), new Vector3(r.xMax, 0, r.yMin), Color.white);
            Debug.DrawLine(new Vector3(r.xMax, 0, r.yMin), new Vector3(r.xMax, 0, r.yMax), Color.white);
            Debug.DrawLine(new Vector3(r.xMax, 0, r.yMax), new Vector3(r.xMin, 0, r.yMax), Color.white);
            Debug.DrawLine(new Vector3(r.xMin, 0, r.yMax), new Vector3(r.xMin, 0, r.yMin), Color.white);

            if (nodes[i].child00 != i)
            {
                // Not a leaf node
                Vector2 c = r.center;
                DebugDrawRec(nodes[i].child11, Rect.MinMaxRect(c.x, c.y, r.xMax, r.yMax));
                DebugDrawRec(nodes[i].child10, Rect.MinMaxRect(c.x, r.yMin, r.xMax, c.y));
                DebugDrawRec(nodes[i].child01, Rect.MinMaxRect(r.xMin, c.y, c.x, r.yMax));
                DebugDrawRec(nodes[i].child00, Rect.MinMaxRect(r.xMin, r.yMin, c.x, c.y));
            }

            for (Agent a = nodes[i].linkedList; a != null; a = a.next)
            {
                var p = nodes[i].linkedList.position;
                Debug.DrawLine(new Vector3(p.x, 0, p.y) + Vector3.up, new Vector3(a.position.x, 0, a.position.y) + Vector3.up, new Color(1, 1, 0, 0.5f));
            }
        }
    }
}
