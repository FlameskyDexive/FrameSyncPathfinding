using UnityEngine;

namespace Pathfinding {
	/** Attach to any GameObject and the object will be clamped to the navmesh.
	 * If a GameObject has this component attached, one or more graph linecasts will be carried out every frame to ensure that the object
	 * does not leave the navmesh area.\n
	 * It can be used with GridGraphs, but Navmesh based ones are prefered.
	 *
	 * \note This has partly been replaced by using an RVOController along with RVONavmesh.
	 * It will not yield exactly the same results though, so this script is still useful in some cases.
	 *
	 * \astarpro */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_navmesh_clamp.php")]
	public class NavmeshClamp : MonoBehaviour {
		GraphNode prevNode;
	    //Good Game
        //Vector3 prevPos;
        VInt3 prevPos;

		// Update is called once per frame
		void LateUpdate () {
			if (prevNode == null)
			{
			    //Good Game
                //var nninfo = AstarPath.active.GetNearest(transform.position);
                var nninfo = AstarPath.active.GetNearest((VInt3)transform.position);
				prevNode = nninfo.node;
			    //Good Game
                //prevPos = transform.position;
                prevPos = (VInt3)transform.position;
			}

			if (prevNode == null) {
				return;
			}

			if (prevNode != null) {
				var graph = AstarData.GetGraph(prevNode) as IRaycastableGraph;
				if (graph != null) {
					GraphHitInfo hit;
				    //Good Game
                    //if (graph.Linecast(prevPos, transform.position, prevNode, out hit)) {
                    if (graph.Linecast(prevPos, (VInt3)transform.position, prevNode, out hit))
                    {
                        //Good Game
                        //hit.point.y = transform.position.y;
                        hit.point.y = (int)transform.position.y;
                        //Good Game
                        //Vector3 closest = VectorMath.ClosestPointOnLine(hit.tangentOrigin, hit.tangentOrigin+hit.tangent, transform.position);
                        Vector3 closest = VectorMath.ClosestPointOnLine((Vector3)hit.tangentOrigin, (Vector3)(hit.tangentOrigin+hit.tangent), transform.position);
                        //Good Game
                        //Vector3 ohit = hit.point;
                        Vector3 ohit = (Vector3)hit.point;
						ohit = ohit + Vector3.ClampMagnitude((Vector3)hit.node.position-ohit, 0.008f);
                        //Good Game
                        /*if (graph.Linecast(ohit, closest, hit.node, out hit)) {
							hit.point.y = transform.position.y;
							transform.position = hit.point;*/
                        if (graph.Linecast((VInt3)ohit, (VInt3)closest, hit.node, out hit)) {
							hit.point.y = (int)transform.position.y;
							transform.position = (Vector3)hit.point;
						} else {
							closest.y = transform.position.y;

							transform.position = closest;
						}
					}
					prevNode = hit.node;
				}
			}

		    //Good Game
            //prevPos = transform.position;
            prevPos = (VInt3)transform.position;
		}
	}
}
