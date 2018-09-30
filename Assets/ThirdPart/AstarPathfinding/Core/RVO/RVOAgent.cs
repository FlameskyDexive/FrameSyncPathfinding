using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding.RVO.Sampled
{
    using Pathfinding;
    using Pathfinding.RVO;
    using Pathfinding.Util;

    /** Internal agent for the RVO system.
	 * Usually you will interface with the IAgent interface instead.
	 *
	 * \see IAgent
	 */
    public class Agent : IAgent
    {
        //Current values for double buffer calculation

        //GG
        //internal float radius, height, desiredSpeed, maxSpeed, agentTimeHorizon, obstacleTimeHorizon;
        internal VInt radius, height;
        internal int desiredSpeed, maxSpeed, agentTimeHorizon, obstacleTimeHorizon;
        internal bool locked = false;

        RVOLayer layer, collidesWith;

        int maxNeighbours;
        //GG
        /*internal Vector2 position;
        float elevationCoordinate;
        Vector2 currentVelocity;*/
        internal VInt2 position;
        int elevationCoordinate;
        VInt2 currentVelocity;

        /** Desired target point - position */
        //GG
        /*Vector2 desiredTargetPointInVelocitySpace;
        Vector2 desiredVelocity;
        Vector2 nextTargetPoint;
        float nextDesiredSpeed;
        float nextMaxSpeed;
        Vector2 collisionNormal;*/
        VInt2 desiredTargetPointInVelocitySpace;
        VInt2 desiredVelocity;
        VInt2 nextTargetPoint;
        int nextDesiredSpeed;
        int nextMaxSpeed;
        VInt2 collisionNormal;

        bool manuallyControlled;
        bool debugDraw;

        #region IAgent Properties

        /** \copydoc Pathfinding::RVO::IAgent::Position */
        //GG
        //public Vector2 Position { get; set; }
        public VInt2 Position { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::ElevationCoordinate */
        //GG
        //public float ElevationCoordinate { get; set; }
        public int ElevationCoordinate { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::CalculatedTargetPoint */
        //GG
        //public Vector2 CalculatedTargetPoint { get; private set; }
        public VInt2 CalculatedTargetPoint { get; private set; }

        /** \copydoc Pathfinding::RVO::IAgent::CalculatedSpeed */
        //GG
        //public float CalculatedSpeed { get; private set; }
        public int CalculatedSpeed { get; private set; }

        /** \copydoc Pathfinding::RVO::IAgent::Locked */
        public bool Locked { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::Radius */
        //GG
        //public float Radius { get; set; }
        public VInt Radius { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::Height */
        //GG
        //public float Height { get; set; }
        public VInt Height { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::AgentTimeHorizon */
        //GG
        //public float AgentTimeHorizon { get; set; }
        public int AgentTimeHorizon { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::ObstacleTimeHorizon */
        //GG
        //public float ObstacleTimeHorizon { get; set; }
        public int ObstacleTimeHorizon { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::MaxNeighbours */
        public int MaxNeighbours { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::NeighbourCount */
        public int NeighbourCount { get; private set; }

        /** \copydoc Pathfinding::RVO::IAgent::Layer */
        public RVOLayer Layer { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::CollidesWith */
        public RVOLayer CollidesWith { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::DebugDraw */
        public bool DebugDraw
        {
            get
            {
                return debugDraw;
            }
            set
            {
                debugDraw = value && simulator != null && !simulator.Multithreading;
            }
        }

        /** \copydoc Pathfinding::RVO::IAgent::Priority */
        //GG
        //public float Priority { get; set; }
        public VFactor Priority { get; set; }

        /** \copydoc Pathfinding::RVO::IAgent::PreCalculationCallback */
        public System.Action PreCalculationCallback { private get; set; }

        #endregion

        #region IAgent Methods

        /** \copydoc Pathfinding::RVO::IAgent::SetTarget */
        //GG
        //public void SetTarget(Vector2 targetPoint, float desiredSpeed, float maxSpeed)
        public void SetTarget(VInt2 targetPoint, int desiredSpeed, int maxSpeed)
        {
            maxSpeed = System.Math.Max(maxSpeed, 0);
            desiredSpeed = System.Math.Min(System.Math.Max(desiredSpeed, 0), maxSpeed);

            nextTargetPoint = targetPoint;
            nextDesiredSpeed = desiredSpeed;
            nextMaxSpeed = maxSpeed;
        }

        /** \copydoc Pathfinding::RVO::IAgent::SetCollisionNormal */
        //GG
        //public void SetCollisionNormal(Vector2 normal)
        public void SetCollisionNormal(VInt2 normal)
        {
            collisionNormal = normal;
        }

        /** \copydoc Pathfinding::RVO::IAgent::ForceSetVelocity */
        //GG
        //public void ForceSetVelocity(Vector2 velocity)
        public void ForceSetVelocity(VInt2 velocity)
        {
            // A bit hacky, but it is approximately correct
            // assuming the agent does not move significantly
            //GG
            nextTargetPoint = CalculatedTargetPoint = position + velocity * 1000;
            //nextTargetPoint = CalculatedTargetPoint = position + velocity;
            nextDesiredSpeed = CalculatedSpeed = velocity.magnitude;
            manuallyControlled = true;
        }

        #endregion

        /** Used internally for a linked list */
        internal Agent next;

        //GG
        /*float calculatedSpeed;
        Vector2 calculatedTargetPoint;*/
        int calculatedSpeed;
        VInt2 calculatedTargetPoint;

        /** Simulator which handles this agent.
		 * Used by this script as a reference and to prevent
		 * adding this agent to multiple simulations.
		 */
        internal Simulator simulator;

        List<Agent> neighbours = new List<Agent>();
        //GG
        //List<float> neighbourDists = new List<float>();
        List<int> neighbourDists = new List<int>();
        List<ObstacleVertex> obstaclesBuffered = new List<ObstacleVertex>();
        List<ObstacleVertex> obstacles = new List<ObstacleVertex>();

        //GG
        //const float DesiredVelocityWeight = 0.1f;
        const int DesiredVelocityWeight = 100;

        /** Extra weight that walls will have */
        //GG
        //const float WallWeight = 5;
        const int WallWeight = 5000;

        public List<ObstacleVertex> NeighbourObstacles
        {
            get
            {
                return null;
            }
        }

        //GG
        //public Agent(Vector2 pos, float elevationCoordinate)
        public Agent(VInt2 pos, int elevationCoordinate)
        {
            AgentTimeHorizon = 2;
            ObstacleTimeHorizon = 2;
            Height = 5;
            Radius = 5;
            MaxNeighbours = 10;
            Locked = false;
            Position = pos;
            ElevationCoordinate = elevationCoordinate;
            Layer = RVOLayer.DefaultAgent;
            CollidesWith = (RVOLayer)(-1);
            //GG
            //Priority = 0.5f;
            Priority = new VFactor(1, 2);
            CalculatedTargetPoint = pos;
            CalculatedSpeed = 0;
            SetTarget(pos, 0, 0);
        }

        /** Reads public properties and stores them in internal fields.
		 * This is required because multithreading is used and if another script
		 * updated the fields at the same time as this class used them in another thread
		 * weird things could happen.
		 *
		 * Will also set CalculatedTargetPoint and CalculatedSpeed to the result
		 * which was last calculated.
		 */
        public void BufferSwitch()
        {
            // <== Read public properties
            radius = Radius;
            height = Height;
            maxSpeed = nextMaxSpeed;
            desiredSpeed = nextDesiredSpeed;
            agentTimeHorizon = AgentTimeHorizon;
            obstacleTimeHorizon = ObstacleTimeHorizon;
            maxNeighbours = MaxNeighbours;
            // Manually controlled overrides the agent being locked
            // (if one for some reason uses them at the same time)
            locked = Locked && !manuallyControlled;
            position = Position;
            elevationCoordinate = ElevationCoordinate;
            collidesWith = CollidesWith;
            layer = Layer;

            if (locked)
            {
                // Locked agents do not move at all
                desiredTargetPointInVelocitySpace = position;
                //GG
                //desiredVelocity = currentVelocity = Vector2.zero;
                desiredVelocity = currentVelocity = VInt2.zero;
            }
            else
            {
                desiredTargetPointInVelocitySpace = nextTargetPoint - position;

                // Estimate our current velocity
                // This is necessary because other agents need to know
                // how this agent is moving to be able to avoid it
                currentVelocity = (CalculatedTargetPoint - position).normalized * CalculatedSpeed;

                // Calculate the desired velocity from the point we want to reach
                desiredVelocity = desiredTargetPointInVelocitySpace.normalized * desiredSpeed;

                //GG
                //if (collisionNormal != Vector2.zero)
                if (collisionNormal != VInt2.zero)
                {
                    collisionNormal.Normalize();
                    //GG
                    //var dot = Vector2.Dot(currentVelocity, collisionNormal);
                    /*if (VInt2.Dot(currentVelocity, collisionNormal) > int.MaxValue)
                    {
                        Debug.LogError($"--overflow--");
                    }*/
                    var dot = VInt2.Dot(currentVelocity, collisionNormal);

                    // Check if the velocity is going into the wall
                    if (dot < 0)
                    {
                        // If so: remove that component from the velocity
                        currentVelocity -= collisionNormal * dot;
                    }

                    // Clear the normal
                    //GG
                    //collisionNormal = Vector2.zero;
                    collisionNormal = VInt2.zero;
                }
            }
        }

        public void PreCalculation()
        {
            if (PreCalculationCallback != null)
            {
                PreCalculationCallback();
            }
        }

        public void PostCalculation()
        {
            // ==> Set public properties
            if (!manuallyControlled)
            {
                CalculatedTargetPoint = calculatedTargetPoint;
                CalculatedSpeed = calculatedSpeed;
            }

            List<ObstacleVertex> tmp = obstaclesBuffered;
            obstaclesBuffered = obstacles;
            obstacles = tmp;

            manuallyControlled = false;
        }

        /** Populate the neighbours and neighbourDists lists with the closest agents to this agent */
        public void CalculateNeighbours()
        {
            neighbours.Clear();
            neighbourDists.Clear();

            //GG
            if (MaxNeighbours > 0 && !locked)
                //simulator.Quadtree.Query(position, maxSpeed, agentTimeHorizon, radius, this);
                simulator.Quadtree.Query(position, maxSpeed, agentTimeHorizon, (long)radius, this);

            NeighbourCount = neighbours.Count;
        }

        /** Square a number */
        //GG
        /*static float Sqr(float x)
        {
            return x * x;
        }*/
        static int Sqr(int x)
        {
            return x * x;
        }
        //GG
        static VFactor Sqr(VFactor x)
        {
            return x * x;
        }

        /** Used by the Quadtree.
		 * \see CalculateNeighbours
		 */
        //GG
       // internal float InsertAgentNeighbour(Agent agent, float rangeSq)
        internal int InsertAgentNeighbour(Agent agent, int rangeSq)
        {
            // Check if this agent collides with the other agent
            if (this == agent || (agent.layer & collidesWith) == 0) return rangeSq;

            // 2D distance
            //GG
            //float dist = (agent.position - position).sqrMagnitude;
            int dist = (agent.position - position).sqrMagnitude;

            if (dist < rangeSq)
            {
                if (neighbours.Count < maxNeighbours)
                {
                    neighbours.Add(null);
                    //GG
                    //neighbourDists.Add(float.PositiveInfinity);
                    neighbourDists.Add(int.MaxValue);
                }

                // Insert the agent into the ordered list of neighbours
                int i = neighbours.Count - 1;
                if (dist < neighbourDists[i])
                {
                    while (i != 0 && dist < neighbourDists[i - 1])
                    {
                        neighbours[i] = neighbours[i - 1];
                        neighbourDists[i] = neighbourDists[i - 1];
                        i--;
                    }
                    neighbours[i] = agent;
                    neighbourDists[i] = dist;
                }

                if (neighbours.Count == maxNeighbours)
                {
                    rangeSq = neighbourDists[neighbourDists.Count - 1];
                }
            }
            return rangeSq;
        }


        /** (x, 0, y) */
        static Vector3 FromXZ(Vector2 p)
        {
            return new Vector3(p.x, 0, p.y);
        }

        /** (x, z) */
        static Vector2 ToXZ(Vector3 p)
        {
            return new Vector2(p.x, p.z);
        }

        /** Converts a 3D vector to a 2D vector in the movement plane.
		 * If movementPlane is XZ it will be projected onto the XZ plane
		 * and the elevation coordinate will be the Y coordinate
		 * otherwise it will be projected onto the XY plane and elevation
		 * will be the Z coordinate.
		 */
        //GG
        //Vector2 To2D(Vector3 p, out float elevation)
        VInt2 To2D(VInt3 p, out int elevation)
        {
            if (simulator.movementPlane == MovementPlane.XY)
            {
                elevation = -p.z;
                //GG
                //return new Vector2(p.x, p.y);
                return new VInt2(p.x, p.y);
            }
            else
            {
                elevation = p.y;
                //GG
                return new VInt2(p.x, p.z);
            }
        }

        static void DrawVO(Vector2 circleCenter, float radius, Vector2 origin)
        {
            float alpha = Mathf.Atan2((origin - circleCenter).y, (origin - circleCenter).x);
            float gamma = radius / (origin - circleCenter).magnitude;
            float delta = gamma <= 1.0f ? Mathf.Abs(Mathf.Acos(gamma)) : 0;

            Draw.Debug.CircleXZ(FromXZ(circleCenter), radius, Color.black, alpha - delta, alpha + delta);
            Vector2 p1 = new Vector2(Mathf.Cos(alpha - delta), Mathf.Sin(alpha - delta)) * radius;
            Vector2 p2 = new Vector2(Mathf.Cos(alpha + delta), Mathf.Sin(alpha + delta)) * radius;

            Vector2 p1t = -new Vector2(-p1.y, p1.x);
            Vector2 p2t = new Vector2(-p2.y, p2.x);
            p1 += circleCenter;
            p2 += circleCenter;

            Debug.DrawRay(FromXZ(p1), FromXZ(p1t).normalized * 100, Color.black);
            Debug.DrawRay(FromXZ(p2), FromXZ(p2t).normalized * 100, Color.black);
        }

        /** Velocity Obstacle.
		 * This is a struct to avoid too many allocations.
		 *
		 * \see https://en.wikipedia.org/wiki/Velocity_obstacle
		 */
        internal struct VO
        {
            //GG
            /*Vector2 line1, line2, dir1, dir2;
            Vector2 cutoffLine, cutoffDir;
            Vector2 circleCenter;*/
            /*public VInt2 origin;
            public VInt2 center;*/
            private VInt2 line1;
            private VInt2 line2;
            private VInt2 dir1;
            private VInt2 dir2;
            private VInt2 cutoffLine;
            private VInt2 cutoffDir;
            private VInt2 circleCenter;

            bool colliding;
            //GG
            /*float radius;
            float weightFactor;
            float weightBonus;*/
            int radius;
            VFactor weightFactor;
            int weightBonus;

            //GG
            //Vector2 segmentStart, segmentEnd;
            VInt2 segmentStart, segmentEnd;
            bool segment;

            /** Creates a VO for avoiding another agent.
			 * \param center The position of the other agent relative to this agent.
			 * \param offset Offset of the velocity obstacle. For example to account for the agents' relative velocities.
			 * \param radius Combined radius of the two agents (radius1 + radius2).
			 * \param inverseDt 1 divided by the local avoidance time horizon (e.g avoid agents that we will hit within the next 2 seconds).
			 * \param inverseDeltaTime 1 divided by the time step length.
			 */
            //GG
            //public VO(Vector2 center, Vector2 offset, float radius, float inverseDt, float inverseDeltaTime)
            public VO(VInt2 center, VInt2 offset, int radius, VFactor inverseDt, VFactor inverseDeltaTime)
            {
                // Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
                //GG
                //this.weightFactor = 1;
                this.weightFactor = new VFactor(1000, 1);
                weightBonus = 0;

                //this.radius = radius;
                //GG
                //Vector2 globalCenter;
                VInt2 globalCenter;

                circleCenter = center * inverseDt + offset;

                //GG
                /*this.weightFactor = 4 * Mathf.Exp(-Sqr(center.sqrMagnitude / (radius * radius))) + 1;
                this.weightFactor = 4 * Mathf.Exp(-Sqr(center.sqrMagnitude / (radius * radius))) + 1;*/
                // Collision?
                //GG
                //if (center.magnitude < radius)
                if (center.magnitude < radius)
                {
                    colliding = true;

                    // 0.001 is there to make sure lin1.magnitude is not so small that the normalization
                    // below will return Vector2.zero as that will make the VO invalid and it will be ignored.
                    //GG
                    /*line1 = center.normalized * (center.magnitude - radius - 0.001f) * 0.3f * inverseDeltaTime;
                    dir1 = new Vector2(line1.y, -line1.x).normalized;*/
                    line1 = center.normalized * (center.magnitude - radius - 1) * new VFactor(3, 10) * inverseDeltaTime;
                    dir1 = new VInt2(line1.y, -line1.x).normalized;
                    line1 += offset;

                    //GG
                    /*cutoffDir = Vector2.zero;
                    cutoffLine = Vector2.zero;
                    dir2 = Vector2.zero;
                    line2 = Vector2.zero;*/
                    cutoffDir = VInt2.zero;
                    cutoffLine = VInt2.zero;
                    dir2 = VInt2.zero;
                    line2 = VInt2.zero;
                    this.radius = 0;
                }
                else
                {
                    colliding = false;

                    center *= inverseDt;
                    radius *= inverseDt;
                    globalCenter = center + offset;

                    // 0.001 is there to make sure cutoffDistance is not so small that the normalization
                    // below will return Vector2.zero as that will make the VO invalid and it will be ignored.
                    //GG
                    //var cutoffDistance = center.magnitude - radius + 0.001f;
                    var cutoffDistance = center.magnitude - radius + 1;

                    cutoffLine = center.normalized * cutoffDistance;
                    //GG
                    //cutoffDir = new Vector2(-cutoffLine.y, cutoffLine.x).normalized;
                    cutoffDir = new VInt2(-cutoffLine.y, cutoffLine.x).normalized;
                    cutoffLine += offset;

                    //GG
                    //float alpha = Mathf.Atan2(-center.y, -center.x);
                    VFactor alpha = IntMath.atan2(-center.y, -center.x);

                    //GG
                    //float delta = Mathf.Abs(Mathf.Acos(radius / center.magnitude));
                    VFactor delta = IntMath.abs(IntMath.acos(radius , center.magnitude));

                    this.radius = radius;

                    // Bounding Lines

                    // Point on circle
                    //GG
                    //line1 = new Vector2(Mathf.Cos(alpha + delta), Mathf.Sin(alpha + delta));
                    line1 = new VInt2(IntMath.cos(alpha + delta).roundInt, IntMath.sin(alpha + delta).roundInt);
                    // Vector tangent to circle which is the correct line tangent
                    // Note that this vector is normalized
                    //GG
                    //dir1 = new Vector2(line1.y, -line1.x);
                    dir1 = new VInt2(line1.y, -line1.x);

                    // Point on circle
                    //GG
                    //line2 = new Vector2(Mathf.Cos(alpha - delta), Mathf.Sin(alpha - delta));
                    //line2 = new VInt2((int)IntMath.cos(alpha - delta), (int)IntMath.sin(alpha - delta));
                    line2 = new VInt2(IntMath.cos(alpha - delta).roundInt, IntMath.sin(alpha - delta).roundInt);
                    // Vector tangent to circle which is the correct line tangent
                    // Note that this vector is normalized
                    //GG
                    //dir2 = new Vector2(line2.y, -line2.x);
                    dir2 = new VInt2(line2.y, -line2.x);

                    line1 = line1 * radius + globalCenter;
                    line2 = line2 * radius + globalCenter;
                }

                //GG
                /*segmentStart = Vector2.zero;
                segmentEnd = Vector2.zero;*/
                segmentStart = VInt2.zero;
                segmentEnd = VInt2.zero;
                segment = false;
            }

            /** Creates a VO for avoiding another agent.
			 * Note that the segment is directed, the agent will want to be on the left side of the segment.
			 */
            //GG
            //public static VO SegmentObstacle(Vector2 segmentStart, Vector2 segmentEnd, Vector2 offset, float radius, float inverseDt, float inverseDeltaTime)
            public static VO SegmentObstacle(VInt2 segmentStart, VInt2 segmentEnd, VInt2 offset, int radius, VFactor inverseDt, VFactor inverseDeltaTime)
            {
                var vo = new VO();

                // Adjusted so that a parameter weightFactor of 1 will be the default ("natural") weight factor
                //GG
                //vo.weightFactor = 1;
                vo.weightFactor = new VFactor(1, 1);
                // Just higher than anything else
                vo.weightBonus = Mathf.Max(radius, 1) * 40;

                //GG
                //var closestOnSegment = VectorMath.ClosestPointOnSegment(segmentStart, segmentEnd, Vector2.zero);
                var closestOnSegment = IntMath.ClosestPointOnSegment(segmentStart, segmentEnd, VInt2.zero);

                // Collision?
                if (closestOnSegment.magnitude <= radius)
                {
                    vo.colliding = true;

                    //GG
                    /*vo.line1 = closestOnSegment.normalized * (closestOnSegment.magnitude - radius) * 0.3f * inverseDeltaTime;
                    vo.dir1 = new Vector2(vo.line1.y, -vo.line1.x).normalized;*/
                    vo.line1 = closestOnSegment.normalized * (closestOnSegment.magnitude - radius) / 3 * inverseDeltaTime;
                    vo.dir1 = new VInt2(vo.line1.y, -vo.line1.x).normalized;
                    vo.line1 += offset;

                    //GG
                    /*vo.cutoffDir = Vector2.zero;
                    vo.cutoffLine = Vector2.zero;
                    vo.dir2 = Vector2.zero;
                    vo.line2 = Vector2.zero;*/
                    vo.cutoffDir = VInt2.zero;
                    vo.cutoffLine = VInt2.zero;
                    vo.dir2 = VInt2.zero;
                    vo.line2 = VInt2.zero;
                    vo.radius = 0;

                    //GG
                    /*vo.segmentStart = Vector2.zero;
                    vo.segmentEnd = Vector2.zero;*/
                    vo.segmentStart = VInt2.zero;
                    vo.segmentEnd = VInt2.zero;
                    vo.segment = false;
                }
                else
                {
                    vo.colliding = false;

                    segmentStart *= inverseDt;
                    segmentEnd *= inverseDt;
                    radius *= inverseDt;

                    var cutoffTangent = (segmentEnd - segmentStart).normalized;
                    vo.cutoffDir = cutoffTangent;
                    //GG
                    //vo.cutoffLine = segmentStart + new Vector2(-cutoffTangent.y, cutoffTangent.x) * radius;
                    vo.cutoffLine = segmentStart + new VInt2(-cutoffTangent.y, cutoffTangent.x) * radius;
                    vo.cutoffLine += offset;

                    // See documentation for details
                    // The call to Max is just to prevent floating point errors causing NaNs to appear
                    var startSqrMagnitude = segmentStart.sqrMagnitude;
                    //GG
                    //var normal1 = -VectorMath.ComplexMultiply(segmentStart, new Vector2(radius, Mathf.Sqrt(Mathf.Max(0, startSqrMagnitude - radius * radius)))) / startSqrMagnitude;
                    var normal1 = -IntMath.ComplexMultiply(segmentStart, new VInt2(radius, IntMath.Sqrt(Mathf.Max(0, startSqrMagnitude - radius * radius)))) / startSqrMagnitude;
                    var endSqrMagnitude = segmentEnd.sqrMagnitude;
                    //GG
                    //var normal2 = -VectorMath.ComplexMultiply(segmentEnd, new Vector2(radius, -Mathf.Sqrt(Mathf.Max(0, endSqrMagnitude - radius * radius)))) / endSqrMagnitude;
                    var normal2 = -IntMath.ComplexMultiply(segmentEnd, new VInt2(radius, -IntMath.Sqrt(Mathf.Max(0, endSqrMagnitude - radius * radius)))) / endSqrMagnitude;

                    vo.line1 = segmentStart + normal1 * radius + offset;
                    vo.line2 = segmentEnd + normal2 * radius + offset;

                    // Note that the normals are already normalized
                    //GG
                    /*vo.dir1 = new Vector2(normal1.y, -normal1.x);
                    vo.dir2 = new Vector2(normal2.y, -normal2.x);*/
                    vo.dir1 = new VInt2(normal1.y, -normal1.x);
                    vo.dir2 = new VInt2(normal2.y, -normal2.x);

                    vo.segmentStart = segmentStart;
                    vo.segmentEnd = segmentEnd;
                    vo.radius = radius;
                    vo.segment = true;
                }

                return vo;
            }

            /** Returns a negative number of if \a p lies on the left side of a line which with one point in \a a and has a tangent in the direction of \a dir.
			 * The number can be seen as the double signed area of the triangle {a, a+dir, p} multiplied by the length of \a dir.
			 * If dir.magnitude=1 this is also the distance from p to the line {a, a+dir}.
			 */
            //GG
            //public static float SignedDistanceFromLine(Vector2 a, Vector2 dir, Vector2 p)
            public static int SignedDistanceFromLine(VInt2 a, VInt2 dir, VInt2 p)
            {
                return (p.x - a.x) * (dir.y) - (dir.x) * (p.y - a.y);
            }

            /** Gradient and value of the cost function of this VO.
			 * Very similar to the #Gradient method however the gradient
			 * and value have been scaled and tweaked slightly.
			 */
            //GG
            //public Vector2 ScaledGradient(Vector2 p, out float weight)
            public VInt2 ScaledGradient(VInt2 p, out int weight)
            {
                var grad = Gradient(p, out weight);

                if (weight > 0)
                {
                    //GG
                    //const float Scale = 2;
                    const int Scale = 2;
                    grad *= Scale * weightFactor;
                    weight *= Scale * weightFactor;
                    weight += 1 + weightBonus;
                }

                return grad;
            }

            /** Gradient and value of the cost function of this VO.
			 * The VO has a cost function which is 0 outside the VO
			 * and increases inside it as the point moves further into
			 * the VO.
			 *
			 * This is the negative gradient of that function as well as its
			 * value (the weight). The negative gradient points in the direction
			 * where the function decreases the fastest.
			 *
			 * The value of the function is the distance to the closest edge
			 * of the VO and the gradient is normalized.
			 */
            //GG
            //public Vector2 Gradient(Vector2 p, out float weight)
            public VInt2 Gradient(VInt2 p, out int weight)
            {
                if (colliding)
                {
                    // Calculate double signed area of the triangle consisting of the points
                    // {line1, line1+dir1, p}
                    //GG
                    //float l1 = SignedDistanceFromLine(line1, dir1, p);
                    int l1 = SignedDistanceFromLine(line1, dir1, p);

                    // Serves as a check for which side of the line the point p is
                    if (l1 >= 0)
                    {
                        weight = l1;
                        //GG
                        //return new Vector2(-dir1.y, dir1.x);
                        return new VInt2(-dir1.y, dir1.x);
                    }
                    else
                    {
                        weight = 0;
                        //GG
                        //return new Vector2(0, 0);
                        return new VInt2(0, 0);
                    }
                }

                //GG
                //float det3 = SignedDistanceFromLine(cutoffLine, cutoffDir, p);
                int det3 = SignedDistanceFromLine(cutoffLine, cutoffDir, p);
                if (det3 <= 0)
                {
                    weight = 0;
                    //GG
                    //return Vector2.zero;
                    return VInt2.zero;
                }
                else
                {
                    // Signed distances to the two edges along the sides of the VO
                    //GG
                    /*float det1 = SignedDistanceFromLine(line1, dir1, p);
                    float det2 = SignedDistanceFromLine(line2, dir2, p);*/
                    int det1 = SignedDistanceFromLine(line1, dir1, p);
                    int det2 = SignedDistanceFromLine(line2, dir2, p);
                    if (det1 >= 0 && det2 >= 0)
                    {
                        // We are inside both of the half planes
                        // (all three if we count the cutoff line)
                        // and thus inside the forbidden region in velocity space

                        // Actually the negative gradient because we want the
                        // direction where it slopes the most downwards, not upwards
                        //GG
                        //Vector2 gradient;
                        VInt2 gradient;

                        // Check if we are in the semicircle region near the cap of the VO
                        //GG
                        //if (Vector2.Dot(p - line1, dir1) > 0 && Vector2.Dot(p - line2, dir2) < 0)
                        if (VInt2.Dot(p - line1, dir1) > 0 && VInt2.Dot(p - line2, dir2) < 0)
                        {
                            if (segment)
                            {
                                // This part will only be reached for line obstacles (i.e not other agents)
                                if (det3 < radius)
                                {
                                    //GG
                                    //var closestPointOnLine = (Vector2)VectorMath.ClosestPointOnSegment(segmentStart, segmentEnd, p);
                                    var closestPointOnLine = IntMath.ClosestPointOnSegment(segmentStart, segmentEnd, p);
                                    var dirFromCenter = p - closestPointOnLine;
                                    //GG
                                    //float distToCenter;
                                    int distToCenter;
                                    //GG
                                    //gradient = VectorMath.Normalize(dirFromCenter, out distToCenter);
                                    gradient = VectorMath.Normalize(dirFromCenter, out distToCenter);
                                    // The weight is the distance to the edge
                                    weight = radius - distToCenter;
                                    return gradient;
                                }
                            }
                            else
                            {
                                var dirFromCenter = p - circleCenter;
                                //GG
                                //float distToCenter;
                                int distToCenter;
                                gradient = VectorMath.Normalize(dirFromCenter, out distToCenter);
                                // The weight is the distance to the edge
                                weight = radius - distToCenter;
                                return gradient;
                            }
                        }

                        if (segment && det3 < det1 && det3 < det2)
                        {
                            weight = det3;
                            //GG
                            //gradient = new Vector2(-cutoffDir.y, cutoffDir.x);
                            gradient = new VInt2(-cutoffDir.y, cutoffDir.x);
                            return gradient;
                        }

                        // Just move towards the closest edge
                        // The weight is the distance to the edge
                        if (det1 < det2)
                        {
                            weight = det1;
                            //GG
                           // gradient = new Vector2(-dir1.y, dir1.x);
                            gradient = new VInt2(-dir1.y, dir1.x);
                        }
                        else
                        {
                            weight = det2;
                            //GG
                            //gradient = new Vector2(-dir2.y, dir2.x);
                            gradient = new VInt2(-dir2.y, dir2.x);
                        }

                        return gradient;
                    }

                    weight = 0;
                    //GG
                    //return Vector2.zero;
                    return VInt2.zero;
                }
            }
        }

        /** Very simple list.
		 * Cannot use a List<T> because when indexing into a List<T> and T is
		 * a struct (which VO is) then the whole struct will be copied.
		 * When indexing into an array, that copy can be skipped.
		 */
        internal class VOBuffer
        {
            public VO[] buffer;
            public int length;

            public void Clear()
            {
                length = 0;
            }

            public VOBuffer(int n)
            {
                buffer = new VO[n];
                length = 0;
            }

            public void Add(VO vo)
            {
                if (length >= buffer.Length)
                {
                    var nbuffer = new VO[buffer.Length * 2];
                    buffer.CopyTo(nbuffer, 0);
                    buffer = nbuffer;
                }
                buffer[length++] = vo;
            }
        }

        internal void CalculateVelocity(Pathfinding.RVO.Simulator.WorkerContext context)
        {
            if (manuallyControlled)
            {
                return;
            }

            if (locked)
            {
                calculatedSpeed = 0;
                calculatedTargetPoint = position;
                return;
            }

            // Buffer which will be filled up with velocity obstacles (VOs)
            var vos = context.vos;
            vos.Clear();

            GenerateObstacleVOs(vos);
            GenerateNeighbourAgentVOs(vos);

            bool insideAnyVO = BiasDesiredVelocity(vos, ref desiredVelocity, ref desiredTargetPointInVelocitySpace, simulator.symmetryBreakingBias);

            if (!insideAnyVO)
            {
                // Desired velocity can be used directly since it was not inside any velocity obstacle.
                // No need to run optimizer because this will be the global minima.
                // This is also a special case in which we can set the
                // calculated target point to the desired target point
                // instead of calculating a point based on a calculated velocity
                // which is an important difference when the agent is very close
                // to the target point
                // TODO: Not actually guaranteed to be global minima if desiredTargetPointInVelocitySpace.magnitude < desiredSpeed
                // maybe do something different here?
                calculatedTargetPoint = desiredTargetPointInVelocitySpace + position;
                calculatedSpeed = desiredSpeed;
                //GG
                //if (DebugDraw) Draw.Debug.CrossXZ(FromXZ(calculatedTargetPoint), Color.white);
                if (DebugDraw) Draw.Debug.CrossXZ(FromXZ((Vector2)calculatedTargetPoint), Color.white);
                return;
            }

            //GG
            //Vector2 result = Vector2.zero;
            VInt2 result = VInt2.zero;

            result = GradientDescent(vos, currentVelocity, desiredVelocity);

            //GG
            //if (DebugDraw) Draw.Debug.CrossXZ(FromXZ(result + position), Color.white);
            if (DebugDraw) Draw.Debug.CrossXZ(FromXZ((Vector2)result + (Vector2)position), Color.white);
            //Debug.DrawRay (To3D (position), To3D (result));

            calculatedTargetPoint = position + result;
            calculatedSpeed = Mathf.Min(result.magnitude, maxSpeed);
        }

        static Color Rainbow(float v)
        {
            Color c = new Color(v, 0, 0);

            if (c.r > 1) { c.g = c.r - 1; c.r = 1; }
            if (c.g > 1) { c.b = c.g - 1; c.g = 1; }
            return c;
        }

        void GenerateObstacleVOs(VOBuffer vos)
        {
            //GG
            //var range = maxSpeed * obstacleTimeHorizon;
            //long range = maxSpeed * obstacleTimeHorizon * 1000000;
            long range = maxSpeed * obstacleTimeHorizon;
            /*if (range * range > long.MaxValue)
            {
                Debug.LogError($"--overflow--");
            }*/

            // Iterate through all obstacles that we might need to avoid
            for (int i = 0; i < simulator.obstacles.Count; i++)
            {
                var obstacle = simulator.obstacles[i];
                var vertex = obstacle;
                // Iterate through all edges (defined by vertex and vertex.dir) in the obstacle
                do
                {
                    // Ignore the edge if the agent should not collide with it
                    if (vertex.ignore || (vertex.layer & collidesWith) == 0)
                    {
                        vertex = vertex.next;
                        continue;
                    }

                    // Start and end points of the current segment
                    //GG
                    //float elevation1, elevation2;
                    int elevation1, elevation2;
                    var p1 = To2D(vertex.position, out elevation1);
                    var p2 = To2D(vertex.next.position, out elevation2);

                    //GG
                    //Vector2 dir = (p2 - p1).normalized;
                    VInt2 dir = (p2 - p1).normalized;

                    // Signed distance from the line (not segment, lines are infinite)
                    // TODO: Can be optimized
                    //GG
                    //float dist = VO.SignedDistanceFromLine(p1, dir, position);
                    int dist = VO.SignedDistanceFromLine(p1, dir, position);

                    //GG
                    //if (dist >= -0.01f && dist < range)
                    if (dist >= -10000 && dist < range)
                    {
                        //GG
                        //float factorAlongSegment = Vector2.Dot(position - p1, p2 - p1) / (p2 - p1).sqrMagnitude;
                        VFactor factorAlongSegment = new VFactor(VInt2.Dot(position - p1, p2 - p1), (p2 - p1).sqrMagnitude);

                        // Calculate the elevation (y) coordinate of the point on the segment closest to the agent
                        //GG
                        //var segmentY = Mathf.Lerp(elevation1, elevation2, factorAlongSegment);
                        var segmentY = IntMath.Lerp(elevation1, elevation2, factorAlongSegment);

                        // Calculate distance from the segment (not line)
                        //GG
                        //var sqrDistToSegment = (Vector2.Lerp(p1, p2, factorAlongSegment) - position).sqrMagnitude;
                        var sqrDistToSegment = (VInt2.Lerp(p1, p2, factorAlongSegment) - position).sqrMagnitude;

                        // Ignore the segment if it is too far away
                        // or the agent is too high up (or too far down) on the elevation axis (usually y axis) to avoid it.
                        // If the XY plane is used then all elevation checks are disabled
                        //GG
                        //if (sqrDistToSegment < range * range && (simulator.movementPlane == MovementPlane.XY || (elevationCoordinate <= segmentY + vertex.height && elevationCoordinate + height >= segmentY)))
                        if (sqrDistToSegment < range * range && (simulator.movementPlane == MovementPlane.XY || (elevationCoordinate <= segmentY + vertex.height && elevationCoordinate + (int)height >= segmentY)))
                        {
                            //GG
                            //vos.Add(VO.SegmentObstacle(p2 - position, p1 - position, Vector2.zero, radius * 0.01f, 1f / ObstacleTimeHorizon, 1f / simulator.DeltaTime));
                            vos.Add(VO.SegmentObstacle(p2 - position, p1 - position, VInt2.zero, (int)radius * 10, new VFactor(1, 1000), simulator.DeltaTimeFactor.Inverse));
                        }
                    }

                    vertex = vertex.next;
                } while (vertex != obstacle && vertex != null && vertex.next != null);
            }
        }

        void GenerateNeighbourAgentVOs(VOBuffer vos)
        {
            //GG
            //float inverseAgentTimeHorizon = 1.0f / agentTimeHorizon;
            VFactor inverseAgentTimeHorizon = new VFactor(1, agentTimeHorizon);

            // The RVO algorithm assumes we will continue to
            // move in roughly the same direction
            //GG
            //Vector2 optimalVelocity = currentVelocity;
            VInt2 optimalVelocity = currentVelocity;

            for (int o = 0; o < neighbours.Count; o++)
            {
                Agent other = neighbours[o];

                // Don't avoid ourselves
                if (other == this)
                    continue;

                // Interval along the y axis in which the agents overlap
                //GG
                /*float maxY = System.Math.Min(elevationCoordinate + height, other.elevationCoordinate + other.height);
                float minY = System.Math.Max(elevationCoordinate, other.elevationCoordinate);*/
                int maxY = System.Math.Min(elevationCoordinate + (int)height, other.elevationCoordinate + (int)other.height);
                int minY = System.Math.Max(elevationCoordinate, other.elevationCoordinate);

                // The agents cannot collide since they are on different y-levels
                if (maxY - minY < 0)
                {
                    continue;
                }

                //GG
                //float totalRadius = radius + other.radius;
                VInt totalRadius = radius + other.radius;

                // Describes a circle on the border of the VO
                //GG
                //Vector2 voBoundingOrigin = other.position - position;
                VInt2 voBoundingOrigin = other.position - position;

                //GG
                //float avoidanceStrength;
                VFactor avoidanceStrength;
                if (other.locked || other.manuallyControlled)
                {
                    //GG
                    //avoidanceStrength = 1;
                    avoidanceStrength = VFactor.one;
                }
                //GG
                //else if (other.Priority > 0.00001f || Priority > 0.00001f)
                else if (other.Priority > new VFactor(1, 100000) || Priority > new VFactor(1, 100000))
                {
                    avoidanceStrength = other.Priority / (Priority + other.Priority);
                }
                else
                {
                    // Both this agent's priority and the other agent's priority is zero or negative
                    // Assume they have the same priority
                    //GG
                    //avoidanceStrength = 0.5f;
                    avoidanceStrength = new VFactor(1, 2);
                }

                // We assume that the other agent will continue to move with roughly the same velocity if the priorities for the agents are similar.
                // If the other agent has a higher priority than this agent (avoidanceStrength > 0.5) then we will assume it will move more along its
                // desired velocity. This will have the effect of other agents trying to clear a path for where a high priority agent wants to go.
                // If this is not done then even high priority agents can get stuck when it is really crowded and they have had to slow down.
                //GG
                /*Vector2 otherOptimalVelocity = Vector2.Lerp(other.currentVelocity, other.desiredVelocity, 2 * avoidanceStrength - 1);

                var voCenter = Vector2.Lerp(optimalVelocity, otherOptimalVelocity, avoidanceStrength);*/
                VInt2 otherOptimalVelocity = VInt2.Lerp(other.currentVelocity, other.desiredVelocity, 2 * avoidanceStrength - VFactor.one);

                var voCenter = VInt2.Lerp(optimalVelocity, otherOptimalVelocity, avoidanceStrength);

                //GG
                //vos.Add(new VO(voBoundingOrigin, voCenter, totalRadius, inverseAgentTimeHorizon, 1 / simulator.DeltaTime));
                vos.Add(new VO(voBoundingOrigin, voCenter, (int)totalRadius, inverseAgentTimeHorizon, simulator.DeltaTimeFactor.Inverse));

                //GG
                if (DebugDraw)
                    //DrawVO(position + voBoundingOrigin * inverseAgentTimeHorizon + voCenter, totalRadius * inverseAgentTimeHorizon, position + voCenter);
                    DrawVO((Vector2)position + (Vector2)voBoundingOrigin * inverseAgentTimeHorizon.single + (Vector2)voCenter, (int)totalRadius * inverseAgentTimeHorizon, (Vector2)position + (Vector2)voCenter);
            }
        }

        //GG
        //Vector2 GradientDescent(VOBuffer vos, Vector2 sampleAround1, Vector2 sampleAround2)
        VInt2 GradientDescent(VOBuffer vos, VInt2 sampleAround1, VInt2 sampleAround2)
        {
            //GG
            //float score1;
            int score1;
            var minima1 = Trace(vos, sampleAround1, out score1);

            //GG
            //if (DebugDraw) Draw.Debug.CrossXZ(FromXZ(minima1 + position), Color.yellow, 0.5f);
            if (DebugDraw) Draw.Debug.CrossXZ(FromXZ((Vector2)minima1 + (Vector2)position), Color.yellow, 0.5f);

            // Can be uncommented for higher quality local avoidance
            // for ( int i = 0; i < 3; i++ ) {
            //	Vector2 p = desiredVelocity + new Vector2(Mathf.Cos(Mathf.PI*2*(i/3.0f)), Mathf.Sin(Mathf.PI*2*(i/3.0f)));
            //	float score;Vector2 res = Trace ( vos, p, velocity.magnitude*simulator.qualityCutoff, out score );
            //
            //	if ( score < best ) {
            //		result = res;
            //		best = score;
            //	}
            // }

            //GG
            /*float score2;
            Vector2 minima2 = Trace(vos, sampleAround2, out score2);
            if (DebugDraw) Draw.Debug.CrossXZ(FromXZ(minima2 + position), Color.magenta, 0.5f);

            return score1 < score2 ? minima1 : minima2;*/
            int score2;
            VInt2 minima2 = Trace(vos, sampleAround2, out score2);
            if (DebugDraw) Draw.Debug.CrossXZ(FromXZ((Vector2)minima2 + (Vector2)position), Color.magenta, 0.5f);

            return score1 < score2 ? minima1 : minima2;
        }


        /** Bias towards the right side of agents.
		 * Rotate desiredVelocity at most [value] number of radians. 1 radian ≈ 57°
		 * This breaks up symmetries.
		 *
		 * The desired velocity will only be rotated if it is inside a velocity obstacle (VO).
		 * If it is inside one, it will not be rotated further than to the edge of it
		 *
		 * The targetPointInVelocitySpace will be rotated by the same amount as the desired velocity
		 *
		 * \returns True if the desired velocity was inside any VO
		 */
        //GG
        //static bool BiasDesiredVelocity(VOBuffer vos, ref Vector2 desiredVelocity, ref Vector2 targetPointInVelocitySpace, float maxBiasRadians)
        static bool BiasDesiredVelocity(VOBuffer vos, ref VInt2 desiredVelocity, ref VInt2 targetPointInVelocitySpace, int maxBiasRadians)
        {
            var desiredVelocityMagn = desiredVelocity.magnitude;
            //GG
            //var maxValue = 0f;
            var maxValue = 0;

            for (int i = 0; i < vos.length; i++)
            {
                //GG
                //float value;
                int value;
                // The value is approximately the distance to the edge of the VO
                // so taking the maximum will give us the distance to the edge of the VO
                // which the desired velocity is furthest inside
                vos.buffer[i].Gradient(desiredVelocity, out value);
                maxValue = Mathf.Max(maxValue, value);
            }

            // Check if the agent was inside any VO
            var inside = maxValue > 0;

            // Avoid division by zero below
            //GG
            //if (desiredVelocityMagn < 0.001f)
            if (desiredVelocityMagn < 1000)
            {
                return inside;
            }

            // Rotate the desired velocity clockwise (to the right) at most maxBiasRadians number of radians
            // Assuming maxBiasRadians is small, we can just move it instead and it will give approximately the same effect
            // See https://en.wikipedia.org/wiki/Small-angle_approximation
            //GG
            /*var angle = Mathf.Min(maxBiasRadians, maxValue / desiredVelocityMagn);
            desiredVelocity += new Vector2(desiredVelocity.y, -desiredVelocity.x) * angle;
            targetPointInVelocitySpace += new Vector2(targetPointInVelocitySpace.y, -targetPointInVelocitySpace.x) * angle;*/
            var angle = Math.Min(maxBiasRadians, maxValue / desiredVelocityMagn);
            desiredVelocity += new VInt2(desiredVelocity.y, -desiredVelocity.x) * angle;
            targetPointInVelocitySpace += new VInt2(targetPointInVelocitySpace.y, -targetPointInVelocitySpace.x) * angle;
            return inside;
        }

        /** Evaluate gradient and value of the cost function at velocity p */
        //GG
        //Vector2 EvaluateGradient(VOBuffer vos, Vector2 p, out float value)
        VInt2 EvaluateGradient(VOBuffer vos, VInt2 p, out int value)
        {
            //GG
            VInt2 gradient = VInt2.zero;

            value = 0;

            // Avoid other agents
            for (int i = 0; i < vos.length; i++)
            {
                //GG
                //float w;
                int w;
                var grad = vos.buffer[i].ScaledGradient(p, out w);
                if (w > value)
                {
                    value = w;
                    gradient = grad;
                }
            }

            // Move closer to the desired velocity
            var dirToDesiredVelocity = desiredVelocity - p;
            var distToDesiredVelocity = dirToDesiredVelocity.magnitude;
            //GG
            //if (distToDesiredVelocity > 0.0001f)
            if (distToDesiredVelocity > 100)
            {
                gradient += dirToDesiredVelocity * (DesiredVelocityWeight / distToDesiredVelocity);
                value += distToDesiredVelocity * DesiredVelocityWeight;
            }

            // Prefer speeds lower or equal to the desired speed
            // and avoid speeds greater than the max speed
            var sqrSpeed = p.sqrMagnitude;
            if (sqrSpeed > desiredSpeed * desiredSpeed)
            {
                //GG
                //var speed = Mathf.Sqrt(sqrSpeed);
                var speed = IntMath.Sqrt(sqrSpeed);

                if (speed > maxSpeed)
                {
                    //GG
                    //const float MaxSpeedWeight = 3;
                    const int MaxSpeedWeight = 3;
                    value += MaxSpeedWeight * (speed - maxSpeed);
                    gradient -= (p / speed) * MaxSpeedWeight;
                }

                // Scale needs to be strictly greater than DesiredVelocityWeight
                // otherwise the agent will not prefer the desired speed over
                // the maximum speed
                //GG
                /*float scale = 2 * DesiredVelocityWeight;
                value += scale * (speed - desiredSpeed);
                gradient -= scale * (p / speed);*/
                int scale = 2 * DesiredVelocityWeight;
                value += scale * (speed - desiredSpeed);
                gradient -= (p / speed) * scale;
            }

            return gradient;
        }

        /** Traces the vector field constructed out of the velocity obstacles.
		 * Returns the position which gives the minimum score (approximately).
		 *
		 * \see https://en.wikipedia.org/wiki/Gradient_descent
		 */
        //GG
        //Vector2 Trace(VOBuffer vos, Vector2 p, out float score)
        VInt2 Trace(VOBuffer vos, VInt2 p, out int score)
        {
            // Pick a reasonable initial step size
            //GG
            //float stepSize = Mathf.Max(radius, 0.2f * desiredSpeed);
            int stepSize = Mathf.Max((int)radius, desiredSpeed / 5);

            //GG
            /*float bestScore = float.PositiveInfinity;
            Vector2 bestP = p;*/
            int bestScore = int.MaxValue;
            VInt2 bestP = p;

            // TODO: Add momentum to speed up convergence?

            const int MaxIterations = 50;

            for (int s = 0; s < MaxIterations; s++)
            {
                //GG
                //float step = 1.0f - (s / (float)MaxIterations);
                VFactor step = VFactor.one - new VFactor(s, MaxIterations);
                step = Sqr(step) * stepSize;

                //GG
                //float value;
                int value;
                var gradient = EvaluateGradient(vos, p, out value);

                if (value < bestScore)
                {
                    bestScore = value;
                    bestP = p;
                }

                // TODO: Add cutoff for performance

                gradient.Normalize();

                gradient *= step;
                //GG
                //Vector2 prev = p;
                VInt2 prev = p;
                p += gradient;

                //GG
                //if (DebugDraw) Debug.DrawLine(FromXZ(prev + position), FromXZ(p + position), Rainbow(s * 0.1f) * new Color(1, 1, 1, 1f));
                if (DebugDraw) Debug.DrawLine(FromXZ((Vector2)prev + (Vector2)position), FromXZ((Vector2)p + (Vector2)position), Rainbow(s * 0.1f) * new Color(1, 1, 1, 1f));
            }

            score = bestScore;
            return bestP;
        }
    }
}
