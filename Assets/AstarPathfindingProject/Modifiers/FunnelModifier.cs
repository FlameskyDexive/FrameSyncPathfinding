using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pathfinding.Util;

namespace Pathfinding {
	[AddComponentMenu("Pathfinding/Modifiers/Funnel")]
	[System.Serializable]
	/** Simplifies paths on navmesh graphs using the funnel algorithm.
	 * The funnel algorithm is an algorithm which can, given a path corridor with nodes in the path where the nodes have an area, like triangles, it can find the shortest path inside it.
	 * This makes paths on navmeshes look much cleaner and smoother.
	 * \shadowimage{funnelModifier_on.png}
	 *
	 * The funnel modifier also works on grid graphs however since it only simplifies the paths within the nodes which the original path visited it may not always
	 * simplify the path as much as you would like it to. The \link Pathfinding.RaycastModifier RaycastModifier\endlink can be a better fit for grid graphs.
	 * \shadowimage{funnel_on_grid.png}
	 *
	 * \ingroup modifiers
	 * \see http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
	 */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_funnel_modifier.php")]
	public class FunnelModifier : MonoModifier {
		/** Determines if twists and bends should be straightened out before running the funnel algorithm.
		 * If the unwrap option is disabled the funnel will simply be projected onto the XZ plane.
		 * If the unwrap option is enabled then the funnel may be oriented arbitrarily and may have twists and bends.
		 * This makes it possible to support the funnel algorithm in XY space as well as in more complicated cases, such
		 * as on curved worlds.
		 *
		 * \note This has a performance overhead, so if you do not need it you can disable it to improve
		 * performance.
		 *
		 * \shadowimage{funnel_unwrap_illustration.png}
		 *
		 * \see #Pathfinding.Funnel.Unwrap for more example images.
		 *
		 * \note This is required if you want to use the funnel modifier for 2D games (i.e in the XY plane).
		 */
		public bool unwrap = true;

		/** Insert a vertex every time the path crosses a portal instead of only at the corners of the path.
		 * The resulting path will have exactly one vertex per portal if this is enabled.
		 * This may introduce vertices with the same position in the output (esp. in corners where many portals meet).
		 * \shadowimage{funnel_split_at_every_portal.png}
		 */
		public bool splitAtEveryPortal;

	#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Funnel Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command) {
			(command.context as Component).gameObject.AddComponent(typeof(FunnelModifier));
		}
	#endif

		public override int Order { get { return 10; } }

        public override void Apply(Path p)
        {
            List<GraphNode> path = p.path;
            List<Int3> vectorPath = p.vectorPath;
            if (((path != null) && (path.Count != 0)) && ((vectorPath != null) && (vectorPath.Count == path.Count)))
            {
                List<Int3> funnelPath = ListPool<Int3>.Claim();
                List<Int3> left = ListPool<Int3>.Claim(path.Count + 1);
                List<Int3> right = ListPool<Int3>.Claim(path.Count + 1);
                left.Add(vectorPath[0]);
                right.Add(vectorPath[0]);
                for (int i = 0; i < (path.Count - 1); i++)
                {
                    bool flag = path[i].GetPortal(path[i + 1], left, right, false);
                    bool flag2 = false;
                    if (!flag && !flag2)
                    {
                        left.Add(path[i].position);
                        right.Add(path[i].position);
                        left.Add(path[i + 1].position);
                        right.Add(path[i + 1].position);
                    }
                }
                left.Add(vectorPath[vectorPath.Count - 1]);
                right.Add(vectorPath[vectorPath.Count - 1]);
                if (!this.RunFunnel(left, right, funnelPath))
                {
                    funnelPath.Add(vectorPath[0]);
                    funnelPath.Add(vectorPath[vectorPath.Count - 1]);
                }
                ListPool<Int3>.Release(p.vectorPath);
                p.vectorPath = funnelPath;
                //PositionsLog(funnelPath);
                ListPool<Int3>.Release(left);
                ListPool<Int3>.Release(right);
            }
        }


        public void OnCreate()
        {
        }

        public void OnGet()
        {
            base.seeker = null;
            //base.priority = 0;
            base.Awake();
        }

        public void OnRecycle()
        {
        }

        public bool RunFunnel(List<Int3> left, List<Int3> right, List<Int3> funnelPath)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }
            if (funnelPath == null)
            {
                throw new ArgumentNullException("funnelPath");
            }
            if (left.Count != right.Count)
            {
                throw new ArgumentException("left and right lists must have equal length");
            }
            if (left.Count <= 3)
            {
                return false;
            }
            while ((left[1] == left[2]) && (right[1] == right[2]))
            {
                left.RemoveAt(1);
                right.RemoveAt(1);
                if (left.Count <= 3)
                {
                    return false;
                }
            }
            Int3 c = left[2];
            if (c == left[1])
            {
                c = right[2];
            }
            while (VectorMath.IsColinearXZ(left[0], left[1], right[1]) || (VectorMath.RightOrColinearXZ(left[1], right[1], c) == VectorMath.RightOrColinearXZ(left[1], right[1], left[0])))
            {
                left.RemoveAt(1);
                right.RemoveAt(1);
                if (left.Count <= 3)
                {
                    return false;
                }
                c = left[2];
                if (c == left[1])
                {
                    c = right[2];
                }
            }
            if (!VectorMath.IsClockwiseXZ(left[0], left[1], right[1]) && !VectorMath.IsColinearXZ(left[0], left[1], right[1]))
            {
                List<Int3> list = left;
                left = right;
                right = list;
            }
            funnelPath.Add(left[0]);
            Int3 a = left[0];
            Int3 b = left[1];
            Int3 num4 = right[1];
            int num5 = 0;
            int num6 = 1;
            int num7 = 1;
            for (int i = 2; i < left.Count; i++)
            {
                if (funnelPath.Count > 0x7d0)
                {
                    Debug.LogWarning("Avoiding infinite loop. Remove this check if you have this long paths.");
                    break;
                }
                Int3 num9 = left[i];
                Int3 num10 = right[i];
                if (VectorMath.SignedTriangleAreaTimes2XZ(a, num4, num10) >= 0L)
                {
                    if ((a == num4) || (VectorMath.SignedTriangleAreaTimes2XZ(a, b, num10) <= 0L))
                    {
                        num4 = num10;
                        num6 = i;
                    }
                    else
                    {
                        funnelPath.Add(b);
                        a = b;
                        num5 = num7;
                        b = a;
                        num4 = a;
                        num7 = num5;
                        num6 = num5;
                        i = num5;
                        continue;
                    }
                }
                if (VectorMath.SignedTriangleAreaTimes2XZ(a, b, num9) <= 0L)
                {
                    if ((a == b) || (VectorMath.SignedTriangleAreaTimes2XZ(a, num4, num9) >= 0L))
                    {
                        b = num9;
                        num7 = i;
                    }
                    else
                    {
                        funnelPath.Add(num4);
                        a = num4;
                        num5 = num6;
                        b = a;
                        num4 = a;
                        num7 = num5;
                        num6 = num5;
                        i = num5;
                    }
                }
            }
            funnelPath.Add(left[left.Count - 1]);
            return true;
        }
    }
}
