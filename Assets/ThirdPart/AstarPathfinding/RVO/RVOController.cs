using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding.RVO {
	using Pathfinding.Util;

	/** RVO Character Controller.
	 * Similar to Unity's CharacterController. It handles movement calculations and takes other agents into account.
	 * It does not handle movement itself, but allows the calling script to get the calculated velocity and
	 * use that to move the object using a method it sees fit (for example using a CharacterController, using
	 * transform.Translate or using a rigidbody).
	 *
	 * \code
	 * public void Update () {
	 *     // Just some point far away
	 *     var targetPoint = transform.position + transform.forward * 100;
	 *
	 *     // Set the desired point to move towards using a desired speed of 10 and a max speed of 12
	 *     controller.SetTarget(targetPoint, 10, 12);
	 *
	 *     // Calculate how much to move during this frame
	 *     // This information is based on movement commands from earlier frames
	 *     // as local avoidance is calculated globally at regular intervals by the RVOSimulator component
	 *     var delta = controller.CalculateMovementDelta(transform.position, Time.deltaTime);
	 *     transform.position = transform.position + delta;
	 * }
	 * \endcode
	 *
	 * For documentation of many of the variables of this class: refer to the Pathfinding.RVO.IAgent interface.
	 *
	 * \note Requires a single RVOSimulator component in the scene
	 *
	 * \see Pathfinding.RVO.IAgent
	 * \see RVOSimulator
	 * \see \ref local-avoidance
	 *
	 * \astarpro
	 */
	[AddComponentMenu("Pathfinding/Local Avoidance/RVO Controller")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_controller.php")]
	public class RVOController : VersionedMonoBehaviour
	{

	    //GG Ã¿Ö¡Ê±¼ä£¬ºÁÃë
	    public const int deltaTimeMs = 20;
	    private VInt3 desiredVelocity = VInt3.zero;

        /** Radius of the agent in world units */
        [Tooltip("Radius of the agent")]
		//GG
        //public float radius = 0.5f;
        public int radius = 400;

		/** Height of the agent in world units */
		[Tooltip("Height of the agent. In world units")]
		//GG
        //public float height = 2;
        public int height = 2000;

		/** A locked unit cannot move. Other units will still avoid it but avoidance quality is not the best. */
		[Tooltip("A locked unit cannot move. Other units will still avoid it. But avoidance quality is not the best")]
		public bool locked;

		/** Automatically set #locked to true when desired velocity is approximately zero.
		 * This prevents other units from pushing them away when they are supposed to e.g block a choke point.
		 *
		 * When this is true every call to #SetTarget or #Move will set the #locked field to true if the desired velocity
		 * was non-zero or false if it was zero.
		 */
		[Tooltip("Automatically set #locked to true when desired velocity is approximately zero")]
		public bool lockWhenNotMoving = false;

		/** How far into the future to look for collisions with other agents (in seconds) */
		[Tooltip("How far into the future to look for collisions with other agents (in seconds)")]
		//GG
        //public float agentTimeHorizon = 2;
        public int agentTimeHorizon = 2000;

		/** How far into the future to look for collisions with obstacles (in seconds) */
		[Tooltip("How far into the future to look for collisions with obstacles (in seconds)")]
		//GG
        //public float obstacleTimeHorizon = 2;
        public int obstacleTimeHorizon = 2000;

		/** Max number of other agents to take into account.
		 * Decreasing this value can lead to better performance, increasing it can lead to better quality of the simulation.
		 */
		[Tooltip("Max number of other agents to take into account.\n" +
			 "A smaller value can reduce CPU load, a higher value can lead to better local avoidance quality.")]
		public int maxNeighbours = 10;

		/** Specifies the avoidance layer for this agent.
		 * The #collidesWith mask on other agents will determine if they will avoid this agent.
		 */
		public RVOLayer layer = RVOLayer.DefaultAgent;

		/** Layer mask specifying which layers this agent will avoid.
		 * You can set it as CollidesWith = RVOLayer.DefaultAgent | RVOLayer.Layer3 | RVOLayer.Layer6 ...
		 *
		 * This can be very useful in games which have multiple teams of some sort. For example you usually
		 * want the agents in one team to avoid each other, but you do not want them to avoid the enemies.
		 *
		 * This field only affects which other agents that this agent will avoid, it does not affect how other agents
		 * react to this agent.
		 *
		 * \see http://en.wikipedia.org/wiki/Mask_(computing)
		 */
		[Pathfinding.EnumFlag]
		public RVOLayer collidesWith = (RVOLayer)(-1);

		/** An extra force to avoid walls.
		 * This can be good way to reduce "wall hugging" behaviour.
		 *
		 * \deprecated This feature is currently disabled as it didn't work that well and was tricky to support after some changes to the RVO system. It may be enabled again in a future version.
		 */
		[HideInInspector]
		[System.Obsolete]
		//GG
        //public float wallAvoidForce = 1;
        public int wallAvoidForce = 1000;

		/** How much the wallAvoidForce decreases with distance.
		 * The strenght of avoidance is:
		 * \code str = 1/dist*wallAvoidFalloff \endcode
		 *
		 * \see wallAvoidForce
		 *
		 * \deprecated This feature is currently disabled as it didn't work that well and was tricky to support after some changes to the RVO system. It may be enabled again in a future version.
		 */
		[HideInInspector]
		[System.Obsolete]
		//GG
        //public float wallAvoidFalloff = 1;
        public int wallAvoidFalloff = 1000;

		/** \copydoc Pathfinding::RVO::IAgent::Priority */
		[Tooltip("How strongly other agents will avoid this agent")]
		//[UnityEngine.Range(0, 1)]
		//GG
        //public float priority = 0.5f;
        public VFactor priority = new VFactor(1, 2);

		/** Center of the agent relative to the pivot point of this game object */
		[Tooltip("Center of the agent relative to the pivot point of this game object")]
		//GG
        //public float center = 1f;
        public int center = 1000;

		/** \details \deprecated */
		[System.Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
		public LayerMask mask { get { return 0; } set {} }

		/** \details \deprecated */
		[System.Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
		public bool enableRotation { get { return false; } set {} }

		/** \details \deprecated */
		[System.Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
		public float rotationSpeed { get { return 0; } set {} }

		/** \details \deprecated */
		[System.Obsolete("This field is obsolete in version 4.0 and will not affect anything. Use the LegacyRVOController if you need the old behaviour")]
		public float maxSpeed { get { return 0; } set {} }

		/** Determines if the XY (2D) or XZ (3D) plane is used for movement */
		public MovementPlane movementPlane {
			get {
				if (simulator != null) return simulator.movementPlane;
				else if (RVOSimulator.active) return RVOSimulator.active.movementPlane;
				else return MovementPlane.XZ;
			}
		}

		/** Reference to the internal agent */
		public IAgent rvoAgent { get; private set; }

		/** Reference to the rvo simulator */
		public Simulator simulator { get; private set; }

		/** Cached tranform component */
		protected Transform tr;

		/** Cached reference to a movement script (if one is used) */
		protected IAstarAI ai;

		/** Enables drawing debug information in the scene view */
		public bool debug;

        /** Current position of the agent.
		 * Note that this is only updated every local avoidance simulation step, not every frame.
		 */
	    //GG
        //public Vector3 position {
        public VInt3 position {
			get {
				return To3D(rvoAgent.Position, rvoAgent.ElevationCoordinate);
			}
		}

        /** Current calculated velocity of the agent.
		 * This is not necessarily the velocity the agent is actually moving with
		 * (that is up to the movement script to decide) but it is the velocity
		 * that the RVO system has calculated is best for avoiding obstacles and
		 * reaching the target.
		 *
		 * \see CalculateMovementDelta
		 *
		 * You can also set the velocity of the agent. This will override the local avoidance input completely.
		 * It is useful if you have a player controlled character and want other agents to avoid it.
		 *
		 * Setting the velocity using this property will mark the agent as being externally controlled for 1 simulation step.
		 * Local avoidance calculations will be skipped for the next simulation step but will be resumed
		 * after that unless this property is set again.
		 *
		 * Note that if you set the velocity the value that can be read from this property will not change until
		 * the next simulation step.
		 *
		 * \see \link Pathfinding::RVO::IAgent::ForceSetVelocity IAgent.ForceSetVelocity\endlink
		 * \see \ref ManualRVOAgent.cs
		 */
	    //GG
        //public Vector3 velocity {
        public VInt3 velocity {
			get {
                // For best accuracy and to allow other code to do things like Move(agent.velocity * Time.deltaTime)
                // the code bases the velocity on how far the agent should move during this frame.
                // Unless the game is paused (timescale is zero) then just use a very small dt.
                //GG
                var dt = Time.deltaTime > 0.0001f ? Time.deltaTime : 0.02f;
			    //return CalculateMovementDelta(dt) / dt;
                return CalculateMovementDelta(deltaTimeMs) / deltaTimeMs;
			}
			set {
				rvoAgent.ForceSetVelocity(To2D(value));
			}
		}

        /** Direction and distance to move in a single frame to avoid obstacles.
		 * \param deltaTime How far to move [seconds].
		 *      Usually set to Time.deltaTime.
		 */
	    //GG
        /*public Vector3 CalculateMovementDelta (float deltaTime) {
			if (rvoAgent == null) return Vector3.zero;
			return To3D(Vector2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(ai != null ? ai.position : tr.position), rvoAgent.CalculatedSpeed * deltaTime), 0);
		}*/
        public VInt3 CalculateMovementDelta (int deltaTimeMS) {
			if (rvoAgent == null) return VInt3.zero;
            //Debug.Log($"--calculate target--{rvoAgent.CalculatedTargetPoint}");
			return To3D(VInt2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(ai != null ? (VInt3)ai.position : (VInt3)tr.position), (int)(rvoAgent.CalculatedSpeed * deltaTimeMS)), 0);
		}

        /** Direction and distance to move in a single frame to avoid obstacles.
		 * \param position Position of the agent.
		 * \param deltaTime How far to move [seconds].
		 *      Usually set to Time.deltaTime.
		 */
	    //GG
        /*public Vector3 CalculateMovementDelta (Vector3 position, float deltaTime) {
			return To3D(Vector2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(position), rvoAgent.CalculatedSpeed * deltaTime), 0);
		}*/
        public VInt3 CalculateMovementDelta (VInt3 position, int deltaTimeMS) {
			return To3D(VInt2.ClampMagnitude(rvoAgent.CalculatedTargetPoint - To2D(position), rvoAgent.CalculatedSpeed * deltaTimeMS), 0);
		}

        /** \copydoc Pathfinding::RVO::IAgent::SetCollisionNormal */
	    //GG
        /*public void SetCollisionNormal (Vector3 normal) {
			rvoAgent.SetCollisionNormal(To2D(normal));
		}*/
        public void SetCollisionNormal (VInt3 normal) {
			rvoAgent.SetCollisionNormal(To2D(normal));
		}

		/** \copydoc Pathfinding::RVO::IAgent::ForceSetVelocity.
		 * \deprecated Set the #velocity property instead
		  */
		[System.Obsolete("Set the 'velocity' property instead")]
		//GG
        //public void ForceSetVelocity (Vector3 velocity) {
        public void ForceSetVelocity (VInt3 velocity) {
			this.velocity = velocity;
		}

        /** Converts a 3D vector to a 2D vector in the movement plane.
		 * If movementPlane is XZ it will be projected onto the XZ plane
		 * otherwise it will be projected onto the XY plane.
		 */
	    //GG
        /*public Vector2 To2D (Vector3 p) {
			float dummy;

			return To2D(p, out dummy);
		}*/
        public VInt2 To2D (VInt3 p) {
			int dummy;

			return To2D(p, out dummy);
		}

        /** Converts a 3D vector to a 2D vector in the movement plane.
		 * If movementPlane is XZ it will be projected onto the XZ plane
		 * and the elevation coordinate will be the Y coordinate
		 * otherwise it will be projected onto the XY plane and elevation
		 * will be the Z coordinate.
		 */
	    //GG
        /*public Vector2 To2D (Vector3 p, out float elevation) {
			if (movementPlane == MovementPlane.XY) {
				elevation = -p.z;
				return new Vector2(p.x, p.y);
			} else {
				elevation = p.y;
				return new Vector2(p.x, p.z);
			}
		}*/
		public VInt2 To2D (VInt3 p, out int elevation) {
			if (movementPlane == MovementPlane.XY) {
				elevation = -p.z;
				return new VInt2(p.x, p.y);
			} else {
				elevation = p.y;
				return new VInt2(p.x, p.z);
			}
		}

        /** Converts a 2D vector in the movement plane as well as an elevation to a 3D coordinate.
		 * \see To2D
		 * \see movementPlane
		 */
	    //GG
        //public Vector3 To3D (Vector2 p, float elevationCoordinate) {
        public VInt3 To3D (VInt2 p, int elevationCoordinate) {
			if (movementPlane == MovementPlane.XY)
			{
			    //GG
                //return new Vector3(p.x, p.y, -elevationCoordinate);
                return new VInt3(p.x, p.y, -elevationCoordinate);
			} else
			{
			    //GG
                //return new Vector3(p.x, elevationCoordinate, p.y);
                return new VInt3(p.x, elevationCoordinate, p.y);
			}
		}

		void OnDisable () {
			if (simulator == null) return;

			// Remove the agent from the simulation but keep the reference
			// this component might get enabled and then we can simply
			// add it to the simulation again
			simulator.RemoveAgent(rvoAgent);
		}

		void OnEnable () {
			tr = transform;
			ai = GetComponent<IAstarAI>();

			if (RVOSimulator.active == null)
			{
				Debug.LogError("No RVOSimulator component found in the scene. Please add one.");
				enabled = false;
			} else
			{
				simulator = RVOSimulator.active.GetSimulator();

				// We might already have an rvoAgent instance which was disabled previously
				// if so, we can simply add it to the simulation again
				if (rvoAgent != null)
				{
					simulator.AddAgent(rvoAgent);
				} else
				{
				    //GG
                    //rvoAgent = simulator.AddAgent(Vector2.zero, 0);
                    rvoAgent = simulator.AddAgent(VInt2.zero, 0);
					rvoAgent.PreCalculationCallback = UpdateAgentProperties;
				    /*this.rvoAgent.desiredVelocity = VInt3.zero;
				    this.rvoAgent.DesiredVelocity = VInt3.zero;
				    this.rvoAgent.newVelocity = VInt3.zero;*/
                }
			}
		}

		protected void UpdateAgentProperties ()
		{
		    //GG
            //rvoAgent.Radius = Mathf.Max(0.001f, radius);
            rvoAgent.Radius = Mathf.Max(1, radius);
			rvoAgent.AgentTimeHorizon = agentTimeHorizon;
			rvoAgent.ObstacleTimeHorizon = obstacleTimeHorizon;
			rvoAgent.Locked = locked;
			rvoAgent.MaxNeighbours = maxNeighbours;
			rvoAgent.DebugDraw = debug;
			rvoAgent.Layer = layer;
			rvoAgent.CollidesWith = collidesWith;
			rvoAgent.Priority = priority;
		    //GG
		    rvoAgent.Height = height;
		    rvoAgent.CollidesWith = collidesWith;
            //rvoAgent.ElevationCoordinate = el

		    //float elevation;
		    int elevation = 0;
		    // Use the position from the movement script if one is attached
		    // as the movement script's position may not be the same as the transform's position
		    // (in particular if IAstarAI.updatePosition is false).
		    //GG
		    //rvoAgent.Position = To2D(ai != null ? ai.position : tr.position, out elevation);
		    rvoAgent.Position = To2D(ai != null ? (VInt3)ai.position : (VInt3)tr.position, out elevation);

		    if (movementPlane == MovementPlane.XZ) {
                rvoAgent.Height = height;
                //GG
                //rvoAgent.ElevationCoordinate = elevation + center - 0.5f * height;
                rvoAgent.ElevationCoordinate = elevation + center -  height / 2;
            } else {
                rvoAgent.Height = 1;
                rvoAgent.ElevationCoordinate = 0;
            }
		    //GG
		    //Debug.Log($"--agent--{gameObject.name}--neighbour count--{rvoAgent.NeighbourCount}");
        }

        /** Set the target point for the agent to move towards.
		 * Similar to the #Move method but this is more flexible.
		 * It is also better to use near the end of the path as when using the Move
		 * method the agent does not know where to stop, so it may overshoot the target.
		 * When using this method the agent will not overshoot the target.
		 * The agent will assume that it will stop when it reaches the target so make sure that
		 * you don't place the point too close to the agent if you actually just want to move in a
		 * particular direction.
		 *
		 * The target point is assumed to stay the same until something else is requested (as opposed to being reset every frame).
		 *
		 * \param pos Point in world space to move towards.
		 * \param speed Desired speed in world units per second.
		 * \param maxSpeed Maximum speed in world units per second.
		 *		The agent will use this speed if it is necessary to avoid collisions with other agents.
		 *		Should be at least as high as speed, but it is recommended to use a slightly higher value than speed (for example speed*1.2).
		 *
		 * \see Also take a look at the documentation for #IAgent.SetTarget which has a few more details.
		 * \see #Move
		 */
        //GG
        //public void SetTarget (Vector3 pos, float speed, float maxSpeed) {
        public void SetTarget (VInt3 pos, int speed, int maxSpeed) {
			if (simulator == null) return;

			rvoAgent.SetTarget(To2D(pos), speed, maxSpeed);

			if (lockWhenNotMoving)
			{
			    //GG
                //locked = speed < 0.001f;
                locked = speed < 1;
			}
		}

        /** Set the desired velocity for the agent.
		 * Note that this is a velocity (units/second), not a movement delta (units/frame).
		 *
		 * This is assumed to stay the same until something else is requested (as opposed to being reset every frame).
		 *
		 * \note In most cases the SetTarget method is better to use.
		 *  What this will actually do is call SetTarget with (position + velocity).
		 *  See the note in the documentation for IAgent.SetTarget about the potential
		 *  issues that this can cause (in particular that it might be hard to get the agent
		 *  to stop at a precise point).
		 *
		 * \see #SetTarget
		 */
	    //GG
        //public void Move (Vector3 vel) {
        public void Move (VInt3 vel) {
			if (simulator == null) return;

			var velocity2D = To2D(vel);
			var speed = velocity2D.magnitude;

			rvoAgent.SetTarget(To2D(ai != null ? ai.position : tr.position) + velocity2D, speed, speed);

			if (lockWhenNotMoving) {
                //GG
				//locked = speed < 0.001f;
				locked = speed < 1;
			}
		}
	    /*public void Move(VInt3 vel)
	    {
            des
	    }*/

		/** Teleport the agent to a new position.
		 * \deprecated Use transform.position instead, the RVOController can now handle that without any issues.
		 */
		[System.Obsolete("Use transform.position instead, the RVOController can now handle that without any issues.")]
		public void Teleport (Vector3 pos) {
			tr.position = pos;
	    }
	    public void DoUpdate(float dt)
	    {
	        if (this.rvoAgent == null)
	        {
	            return;
	        }
	        /*ActorRoot handle = this.actor.handle;
	        if (this.lastPosition != handle.location)
	        {
	            this.Teleport(handle.location);
	        }
	        if (this.lockWhenNotMoving)
	        {
	            this.locked = (this.desiredVelocity == VInt3.zero);
	        }
	        this.UpdateAgentProperties();
	        VInt3 interpolatedPosition = this.rvoAgent.InterpolatedPosition;
	        this.rvoAgent.SetYPosition(this.adjustedY);
	        this.rvoAgent.DesiredVelocity = this.desiredVelocity;
	        VInt3 vInt = interpolatedPosition - this.center;
	        vInt.y += this.height.i >> 1;
	        if (this.checkNavNode)
	        {
	            VInt3 delta = vInt - handle.location;
	            VInt groundY;
	            VInt3 rhs = PathfindingUtility.Move(this.actor, delta, out groundY, out handle.hasReachedNavEdge, null);
	            VInt3 vInt2 = handle.location + rhs;
	            handle.location = vInt2;
	            handle.groundY = groundY;
	            this.rvoAgent.Teleport(vInt2);
	            this.adjustedY = vInt2.y;
	        }
	        else
	        {
	            handle.location = vInt;
	        }
	        this.lastPosition = handle.location;
	        if (this.enableRotation && this.velocity != VInt3.zero)
	        {
	            Vector3 forward = (Vector3)this.velocity;
	            Transform transform = base.transform;
	            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forward), dt * this.rotationSpeed * Mathf.Min((float)this.velocity.magnitude, 0.2f));
	        }*/
	    }

	    public void Update()
	    {
	        if (!RVOSimulator.IsFrameMode)
	        {
	            this.DoUpdate(Time.deltaTime);
	        }
	    }

	    public void UpdateLogic(int dt)
	    {
	        if (RVOSimulator.IsFrameMode)
	        {
	            this.DoUpdate((float)dt * 0.001f);
	        }
	    }

        private static readonly Color GizmoColor = new Color(240/255f, 213/255f, 30/255f);

		void OnDrawGizmos () {
			var color = GizmoColor * (locked ? 0.5f : 1.0f);

			var pos = ai != null ? ai.position : transform.position;

			if (movementPlane == MovementPlane.XY) {
				Draw.Gizmos.Cylinder(pos, Vector3.forward, 0, radius, color);
			}
			else
			{
			    //GG
                //Draw.Gizmos.Cylinder(pos + To3D(Vector2.zero, center - height * 0.5f), To3D(Vector2.zero, 1), height, radius, color);
                Draw.Gizmos.Cylinder(pos + (Vector3)To3D(VInt2.zero, center - height / 2), (Vector3)To3D(VInt2.zero, 1), height, radius, color);
			}
		}
	}
}
