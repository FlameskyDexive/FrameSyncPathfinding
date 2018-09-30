using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public abstract class GNavmeshClipper
    {
        /** Called every time a NavmeshCut/NavmeshAdd component is enabled. */
        static System.Action<GNavmeshClipper> OnEnableCallback;

        /** Called every time a NavmeshCut/NavmeshAdd component is disabled. */
        static System.Action<GNavmeshClipper> OnDisableCallback;

        static readonly LinkedList<GNavmeshClipper> all = new LinkedList<GNavmeshClipper>();
        readonly LinkedListNode<GNavmeshClipper> node;

        public GNavmeshClipper()
        {
            node = new LinkedListNode<GNavmeshClipper>(this);
        }

        public static void AddEnableCallback(System.Action<GNavmeshClipper> onEnable, System.Action<GNavmeshClipper> onDisable)
        {
            OnEnableCallback += onEnable;
            OnDisableCallback += onDisable;

            for (var current = all.First; current != null; current = current.Next)
            {
                onEnable(current.Value);
            }
        }

        public static void RemoveEnableCallback(System.Action<GNavmeshClipper> onEnable, System.Action<GNavmeshClipper> onDisable)
        {
            OnEnableCallback -= onEnable;
            OnDisableCallback -= onDisable;

            for (var current = all.First; current != null; current = current.Next)
            {
                onDisable(current.Value);
            }
        }

        public static bool AnyEnableListeners
        {
            get
            {
                return OnEnableCallback != null;
            }
        }

        protected virtual void OnEnable()
        {
            all.AddFirst(node);
            if (OnEnableCallback != null) OnEnableCallback(this);
        }

        protected virtual void OnDisable()
        {
            if (OnDisableCallback != null) OnDisableCallback(this);
            all.Remove(node);
        }

        internal abstract void NotifyUpdated();
        internal abstract Rect GetBounds(Pathfinding.Util.GraphTransform transform);
        public abstract bool RequiresUpdate();
        public abstract void ForceUpdate();
    }

    public class GNavmeshCut : GNavmeshClipper
    {
        public enum MeshType
        {
            Rectangle,
            Circle,
            CustomMesh
        }
        public GNavmeshCut.MeshType type = MeshType.Circle;
        
        public Mesh mesh;
        public Vector2 rectangleSize = new Vector2(1, 1);
        public float circleRadius = 1.32f;
        public int circleResolution = 6;
        public float height = 2;
        /** Scale of the custom mesh, if used */
        [Tooltip("Scale of the custom mesh")]
        public float meshScale = 1;
        public Vector3 center = Vector3.zero;
        public float updateDistance = 0.5f;
        public bool isDual;
        public bool cutsAddedGeom = true;
        public float updateRotationDistance = 10;
        [UnityEngine.Serialization.FormerlySerializedAsAttribute("useRotation")]
        public bool useRotationAndScale;
        // mesh的边界坐标集合，整型化只需要直接客户端传入这个数据即可
        //Vector3[][] contours;

        /** cached transform component */
        //protected Transform tr;

        //Mesh lastMesh;
        Vector3 lastPosition;
        //Quaternion lastRotation;
        private Vector3 cutPos;

        /*protected override void Awake()
        {
            base.Awake();
            tr = transform;
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos">当前障碍世界坐标</param>
        /// <param name="radius">当前障碍半径大小</param>
        /// <param name="circleResolution">当前障碍边数，默认为6，即六边形</param>
        /// <param name="height">当前障碍高度</param>
        public void InitNavmeshCut(Vector3 pos, float radius, int circleResolution, float height)
        {
            this.cutPos = pos;
            this.circleRadius = radius;
            this.circleResolution = circleResolution;
            this.height = height;

            //OnEnable();
        }

        /// <summary>
        /// 启用当前障碍，刷新navmesh
        /// </summary>
        public void OnEnable()
        {
            base.OnEnable();
            lastPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            //lastRotation = tr.rotation;
        }

        /// <summary>
        /// 删除当前障碍，刷新navmesh
        /// </summary>
        public void OnDisable()
        {
            base.OnDisable();
        }

        /** Cached variable, to avoid allocations */
        //static readonly Dictionary<VInt2, int> edges = new Dictionary<VInt2, int>();
        /** Cached variable, to avoid allocations */
        //static readonly Dictionary<int, int> pointers = new Dictionary<int, int>();
        
        public override void ForceUpdate()
        {
            lastPosition = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        }

        /** Returns true if this object has moved so much that it requires an update.
		 * When an update to the navmesh has been done, call NotifyUpdated to be able to get
		 * relavant output from this method again.
		 */
        public override bool RequiresUpdate()
        {
            //return false;
            return (cutPos - lastPosition).sqrMagnitude > updateDistance * updateDistance;
        }

        /**
		 * Called whenever this navmesh cut is used to update the navmesh.
		 * Called once for each tile the navmesh cut is in.
		 * You can override this method to execute custom actions whenever this happens.
		 */
        public virtual void UsedForCut()
        {
        }

        /** Internal method to notify the NavmeshCut that it has just been used to update the navmesh */
        internal override void NotifyUpdated()
        {
            lastPosition = cutPos;

            /*if (useRotationAndScale)
            {
                lastRotation = tr.rotation;
            }*/
        }

        
        /** Bounds in XZ space after transforming using the *inverse* transform of the \a inverseTranform parameter.
		 * The transformation will typically transform the vertices to graph space and this is used to
		 * figure out which tiles the cut intersects.
		 */
        internal override Rect GetBounds(Pathfinding.Util.GraphTransform inverseTranform)
        {
            var buffers = Pathfinding.Util.ListPool<List<Vector3>>.Claim();

            GetContour(buffers);

            Rect r = new Rect();
            for (int i = 0; i < buffers.Count; i++)
            {
                var buffer = buffers[i];
                for (int k = 0; k < buffer.Count; k++)
                {
                    var p = inverseTranform.InverseTransform(buffer[k]);
                    if (k == 0)
                    {
                        r = new Rect(p.x, p.z, 0, 0);
                    }
                    else
                    {
                        r.xMax = System.Math.Max(r.xMax, p.x);
                        r.yMax = System.Math.Max(r.yMax, p.z);
                        r.xMin = System.Math.Min(r.xMin, p.x);
                        r.yMin = System.Math.Min(r.yMin, p.z);
                    }
                }
            }

            Pathfinding.Util.ListPool<List<Vector3>>.Release(ref buffers);
            return r;
        }

        /**
		 * World space contour of the navmesh cut.
		 * Fills the specified buffer with all contours.
		 * The cut may contain several contours which is why the buffer is a list of lists.
		 */
        public void GetContour(List<List<Vector3>> buffer)
        {
            if (circleResolution < 3) circleResolution = 3;

            bool reverse;
            switch (type)
            {
                case GNavmeshCut.MeshType.Rectangle:
                    List<Vector3> buffer0 = Pathfinding.Util.ListPool<Vector3>.Claim();

                    buffer0.Add(new Vector3(-rectangleSize.x, 0, -rectangleSize.y) * 0.5f);
                    buffer0.Add(new Vector3(rectangleSize.x, 0, -rectangleSize.y) * 0.5f);
                    buffer0.Add(new Vector3(rectangleSize.x, 0, rectangleSize.y) * 0.5f);
                    buffer0.Add(new Vector3(-rectangleSize.x, 0, rectangleSize.y) * 0.5f);

                    reverse = (rectangleSize.x < 0) ^ (rectangleSize.y < 0);
                    TransformBuffer(buffer0, reverse);
                    buffer.Add(buffer0);
                    break;
                case GNavmeshCut.MeshType.Circle:
                    buffer0 = Pathfinding.Util.ListPool<Vector3>.Claim(circleResolution);

                    for (int i = 0; i < circleResolution; i++)
                    {
                        buffer0.Add(new Vector3(Mathf.Cos((i * 2 * Mathf.PI) / circleResolution), 0, Mathf.Sin((i * 2 * Mathf.PI) / circleResolution)) * circleRadius);
                    }

                    reverse = circleRadius < 0;
                    TransformBuffer(buffer0, reverse);
                    buffer.Add(buffer0);
                    break;
                /*case NavmeshCut.MeshType.CustomMesh:
                    if (mesh != lastMesh || contours == null)
                    {
                        CalculateMeshContour();
                        lastMesh = mesh;
                    }

                    if (contours != null)
                    {
                        reverse = meshScale < 0;

                        for (int i = 0; i < contours.Length; i++)
                        {
                            Vector3[] contour = contours[i];

                            buffer0 = Pathfinding.Util.ListPool<Vector3>.Claim(contour.Length);
                            for (int x = 0; x < contour.Length; x++)
                            {
                                buffer0.Add(contour[x] * meshScale);
                            }

                            TransformBuffer(buffer0, reverse);
                            buffer.Add(buffer0);
                        }
                    }
                    break;*/
            }
        }

        void TransformBuffer(List<Vector3> buffer, bool reverse)
        {
            var offset = center;

            // Take rotation and scaling into account
            /*if (useRotationAndScale)
            {
                var local2world = tr.localToWorldMatrix;
                for (int i = 0; i < buffer.Count; i++) buffer[i] = local2world.MultiplyPoint3x4(buffer[i] + offset);
                reverse ^= VectorMath.ReversesFaceOrientationsXZ(local2world);
            }
            else*/
            {
                offset += cutPos;
                for (int i = 0; i < buffer.Count; i++) buffer[i] += offset;
            }

            if (reverse) buffer.Reverse();
        }

        /** Y coordinate of the center of the bounding box in graph space */
        internal float GetY(Pathfinding.Util.GraphTransform transform)
        {
            return transform.InverseTransform(cutPos + center).y;
        }
    }
}
