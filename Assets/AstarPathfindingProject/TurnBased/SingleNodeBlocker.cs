using UnityEngine;

namespace Pathfinding {
	/** Blocks single nodes in a graph.
	 *
	 * This is useful in turn based games where you want
	 * units to avoid all other units while pathfinding
	 * but not be blocked by itself.
	 *
	 * \note This cannot be used together with any movement script
	 * as the nodes are not blocked in the normal way.
	 * \see TurnBasedAI for example usage
	 *
	 * \see BlockManager
	 * \see \ref turnbased
	 */
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_single_node_blocker.php")]
	public class SingleNodeBlocker : VersionedMonoBehaviour {
		public GraphNode lastBlocked { get; private set; }
		public BlockManager manager;

		/** Block node closest to the position of this object.
		 *
		 * Will unblock the last node that was reserved (if any)
		 */
		public void BlockAtCurrentPosition ()
		{
		    //Good Game
            //BlockAt(transform.position);
            BlockAt((Int3)transform.position);
		}

        /** Block node closest to the specified position.
		 *
		 * Will unblock the last node that was reserved (if any)
		 */
	    //Good Game
        //public void BlockAt (Vector3 position) {
        public void BlockAt (Int3 position) {
			Unblock();
			var node = AstarPath.active.GetNearest(position, NNConstraint.None).node;
			if (node != null) {
				Block(node);
			}
		}

		/** Block specified node.
		 *
		 * Will unblock the last node that was reserved (if any)
		 */
		public void Block (GraphNode node) {
			if (node == null)
				throw new System.ArgumentNullException("node");

			manager.InternalBlock(node, this);
			lastBlocked = node;
		}

		/** Unblock the last node that was blocked (if any) */
		public void Unblock () {
			if (lastBlocked == null || lastBlocked.Destroyed) {
				lastBlocked = null;
				return;
			}

			manager.InternalUnblock(lastBlocked, this);
			lastBlocked = null;
		}
	}
}