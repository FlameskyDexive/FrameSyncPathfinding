using UnityEngine;
using System.Collections.Generic;
using Pathfinding.RVO;

namespace Pathfinding.Examples
{
    /** Example movement script for using RVO.
	 *
	 * Primarily intended for the example scenes.
	 * You can use the AIPath or RichAI movement scripts in your own projects.
	 *
	 * \see #Pathfinding.AIPath
	 * \see #Pathfinding.RichAI
	 * \see #Pathfinding.RVO.RVOController
	 */
    [RequireComponent(typeof(RVOController))]
    [RequireComponent(typeof(Seeker))]
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_v_o_example_agent.php")]
    public class RVOExampleAgent : MonoBehaviour
    {
        public float repathRate = 1;

        private float nextRepath = 0;

        //Good Game
        //private Vector3 target;
        private VInt3 target;
        private bool canSearchAgain = true;

        private RVOController controller;
        public float maxSpeed = 10;

        Path path = null;

        //Good Game
        //List<Vector3> vectorPath;
        List<VInt3> vectorPath;
        int wp;

        public float moveNextDist = 1;
        public float slowdownDistance = 1;
        public LayerMask groundMask;

        Seeker seeker;

        MeshRenderer[] rends;

        public void Awake()
        {
            seeker = GetComponent<Seeker>();
            controller = GetComponent<RVOController>();
        }

        /** Set the point to move to */
        public void SetTarget(Vector3 target)
        {
            //Good Game
            //this.target = target;
            this.target = (VInt3)target;
            //Debug.Log("--target point--" + target + "--" + gameObject.transform.GetSiblingIndex());
            RecalculatePath();
        }

        /** Animate the change of color */
        public void SetColor(Color color)
        {
            if (rends == null) rends = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in rends)
            {
                Color current = rend.material.GetColor("_TintColor");
                AnimationCurve curveR = AnimationCurve.Linear(0, current.r, 1, color.r);
                AnimationCurve curveG = AnimationCurve.Linear(0, current.g, 1, color.g);
                AnimationCurve curveB = AnimationCurve.Linear(0, current.b, 1, color.b);

                AnimationClip clip = new AnimationClip();
#if !(UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8)
                // Needed to make Unity5 happy
                clip.legacy = true;
#endif
                clip.SetCurve("", typeof(Material), "_TintColor.r", curveR);
                clip.SetCurve("", typeof(Material), "_TintColor.g", curveG);
                clip.SetCurve("", typeof(Material), "_TintColor.b", curveB);

                Animation anim = rend.gameObject.GetComponent<Animation>();
                if (anim == null)
                {
                    anim = rend.gameObject.AddComponent<Animation>();
                }
                clip.wrapMode = WrapMode.Once;
                anim.AddClip(clip, "ColorAnim");
                anim.Play("ColorAnim");
            }
        }

        public void RecalculatePath()
        {
            canSearchAgain = false;
            nextRepath = Time.time + repathRate * (Random.value + 0.5f);
            //Good Game
            //seeker.StartPath(transform.position, target, OnPathComplete);
            seeker.StartPath((VInt3)transform.position, (VInt3)target, OnPathComplete);
            //Debug.Log(gameObject.name + "start--" + transform.position + "--end--" + target);
        }

        public void OnPathComplete(Path _p)
        {
            //Good Game
            /*if(!_p.error)
            {
                for (int i = 0; i < _p.vectorPath.Count; i++)
                {
                    Debug.Log(gameObject.name + "--first--" + i + "--" + IntMath.Int3s2Vector3s(_p.vectorPath)[i]);
                }
            }*/
            ABPath p = _p as ABPath;

            canSearchAgain = true;

            if (path != null) path.Release(this);
            path = p;
            p.Claim(this);

            if (p.error)
            {
                wp = 0;
                vectorPath = null;
                return;
            }


            //Good Game
            /*Vector3 p1 = (Vector3)p.originalStartPoint;
			Vector3 p2 = transform.position;*/
            VInt3 p1 = p.originalStartPoint;
            VInt3 p2 = (VInt3)transform.position;
            p1.y = p2.y;
            //GG
            //float d = (p2 - p1).magnitude;
            float d = ((Vector3)(p2 - p1)).magnitude;
            wp = 0;

            //Good Game
            vectorPath = p.vectorPath;
            //vectorPath = IntMath.Int3s2Vector3s(p.vectorPath);
            for (int i = 0; i < vectorPath.Count; i++)
		    {
		        Debug.Log(gameObject.name + "--path count--" + i + "--" + vectorPath[i]);
		    }
            //Good Game
            //Vector3 waypoint;
            VInt3 waypoint;

            if (moveNextDist > 0)
            {
                for (float t = 0; t <= d; t += moveNextDist * 0.6f)
                {
                    wp--;
                    //Good Game
                    //Vector3 pos = p1 + (p2-p1)*t;
                    VInt3 pos = p1 + (p2 - p1) * t;

                    do
                    {
                        wp++;
                        waypoint = vectorPath[wp];
                        //Debug.Log($"--waypoint--{gameObject.name}--{((Vector2)controller.To2D((VInt3)pos - vectorPath[wp])).sqrMagnitude}--{moveNextDist * moveNextDist}");
                    }
                    //Good Game
                    //while (controller.To2D(pos - waypoint).sqrMagnitude < moveNextDist*moveNextDist && wp != vectorPath.Count-1);
                    //while (((Vector2)controller.To2D((VInt3)pos - vectorPath[wp])).sqrMagnitude < moveNextDist * moveNextDist && wp != vectorPath.Count - 1);
                    while (controller.To2D((pos - waypoint)).sqrMagnitude < moveNextDist * moveNextDist && wp != vectorPath.Count - 1);
                }
                //Debug.Log($"--waypoint index--{gameObject.name}--{wp}");
            }
            //GG Test
            //wp = 2;
        }

        public void Update()
        {
            if (Time.time >= nextRepath && canSearchAgain)
            {
                RecalculatePath();
            }

            Vector3 pos = transform.position;

            if (vectorPath != null && vectorPath.Count != 0)
            {
                //Good Game
                //while ((controller.To2D(pos - vectorPath[wp]).sqrMagnitude < moveNextDist*moveNextDist && wp != vectorPath.Count-1) || wp == 0)
                //while ((controller.To2D((VInt3)pos - vectorPath[wp]).sqrMagnitude < moveNextDist*moveNextDist && wp != vectorPath.Count-1) || wp == 0) 
                //Debug.Log($"--waypoint--{gameObject.name}--{(controller.To2D((VInt3)pos - vectorPath[wp])).sqrMagnitude / 1000000f}--{moveNextDist * moveNextDist}");
                while ((((Vector2)controller.To2D((VInt3)pos - vectorPath[wp])).sqrMagnitude < moveNextDist * moveNextDist && wp != vectorPath.Count - 1) || wp == 0)
                {
                    wp++;
                    //Debug.Log($"--agent{transform.GetSiblingIndex()}--wp--{wp}");
                }

                // Current path segment goes from vectorPath[wp-1] to vectorPath[wp]
                // We want to find the point on that segment that is 'moveNextDist' from our current position.
                // This can be visualized as finding the intersection of a circle with radius 'moveNextDist'
                // centered at our current position with that segment.
                var p1 = vectorPath[wp - 1];
                var p2 = vectorPath[wp];

                // Calculate the intersection with the circle. This involves some math.
                //Good Game
                //var t = VectorMath.LineCircleIntersectionFactor(controller.To2D(transform.position), controller.To2D(p1), controller.To2D(p2), moveNextDist);
                var t = VectorMath.LineCircleIntersectionFactor((Vector2)controller.To2D((VInt3)transform.position), (Vector2)controller.To2D(p1), (Vector2)controller.To2D(p2), moveNextDist );
                // Clamp to a point on the segment
                t = Mathf.Clamp01(t);
                //Good Game
                //Vector3 waypoint = Vector3.Lerp(p1, p2, t);
                VInt3 waypoint = VInt3.Lerp(p1, p2, t);

                // Calculate distance to the end of the path
                //Good Game
                /*float remainingDistance = controller.To2D(waypoint - pos).magnitude + controller.To2D(waypoint - p2).magnitude;
				for (int i = wp; i < vectorPath.Count - 1; i++)
				    remainingDistance += controller.To2D(vectorPath[i+1] - vectorPath[i]).magnitude;*/
                float remainingDistance = controller.To2D(waypoint - (VInt3)pos).magnitude + controller.To2D(waypoint - p2).magnitude;
                for (int i = wp; i < vectorPath.Count - 1; i++)
                    remainingDistance += controller.To2D(vectorPath[i + 1] - vectorPath[i]).magnitude;

                // Set the target to a point in the direction of the current waypoint at a distance
                // equal to the remaining distance along the path. Since the rvo agent assumes that
                // it should stop when it reaches the target point, this will produce good avoidance
                // behavior near the end of the path. When not close to the end point it will act just
                // as being commanded to move in a particular direction, not toward a particular point
                //GG
                remainingDistance /= 1000000;
                //Debug.Log("--remian--" + remainingDistance.ToString("f2"));
                //var rvoTarget = (waypoint - pos).normalized * remainingDistance + pos;
                var rvoTarget = ((Vector3)waypoint - pos).normalized * remainingDistance + pos;
                // When within [slowdownDistance] units from the target, use a progressively lower speed
                var desiredSpeed = Mathf.Clamp01(remainingDistance / slowdownDistance) * maxSpeed;
                Debug.DrawLine(transform.position, waypoint, Color.red);
                //GG
                //controller.SetTarget(rvoTarget, desiredSpeed, maxSpeed);
                //Debug.Log($"--target--{rvoTarget}--de speed--{desiredSpeed}--maxSpeed--{maxSpeed}");
                controller.SetTarget((VInt3)rvoTarget, (int)(desiredSpeed * 1000), (int)(maxSpeed* 1000));
                //controller.SetTarget((VInt3)rvoTarget, 50, (int)(maxSpeed* 1000));
            }
            else
            {
                // Stand still
                //GG
                //controller.SetTarget(pos, maxSpeed, maxSpeed);
                controller.SetTarget((VInt3)pos, (int)(maxSpeed * 1000), (int)(maxSpeed * 1000));
            }

            // Get a processed movement delta from the rvo controller and move the character.
            // This is based on information from earlier frames.
            //GG
            //var movementDelta = controller.CalculateMovementDelta(Time.deltaTime);
            //var movementDelta = controller.CalculateMovementDelta((int)(Time.deltaTime * 1000));
            var movementDelta = controller.CalculateMovementDelta(50);
            //GG
            //pos += movementDelta;
            pos += (Vector3)movementDelta;
            //Debug.Log("--" + movementDelta.ToString());

            //Rotate the character if the velocity is not extremely small
            //GG
            //if (Time.deltaTime > 0 && movementDelta.magnitude / Time.deltaTime > 0.01f)
            if (Time.deltaTime > 0 && ((Vector3)movementDelta).magnitude / Time.deltaTime > 0.01f)
            {
                var rot = transform.rotation;
                //GG
                //var targetRot = Quaternion.LookRotation((Vector3)movementDelta, (Vector3)controller.To3D(Vector2.zero, 1));
                var targetRot = Quaternion.LookRotation((Vector3)movementDelta, (Vector3)controller.To3D(VInt2.zero, 1000));
                const float RotationSpeed = 5;
                if (controller.movementPlane == MovementPlane.XY)
                {
                    targetRot = targetRot * Quaternion.Euler(-90, 180, 0);
                }
                transform.rotation = Quaternion.Slerp(rot, targetRot, Time.deltaTime * RotationSpeed);
            }

            if (controller.movementPlane == MovementPlane.XZ)
            {
                RaycastHit hit;
                if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 2, groundMask))
                {
                    pos.y = hit.point.y;
                }
            }

            transform.position = pos;
            //Good Game
            /*if(pos.x > 200 || pos.y > 200 || pos.z > 200 || pos.x > -200 || pos.z < -200)
                Debug.LogError("--" + gameObject.name + "--" + pos);*/
        }
    }
}
