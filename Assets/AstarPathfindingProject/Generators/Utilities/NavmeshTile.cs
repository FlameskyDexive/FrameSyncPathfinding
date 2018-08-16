namespace Pathfinding {
	using Pathfinding.Util;
	using UnityEngine;

	public class NavmeshTile : INavmeshHolder {
		/** Tile triangles */
		public int[] tris;

		/** Tile vertices */
		public VInt3[] verts;

		/** Tile vertices in graph space */
		public VInt3[] vertsInGraphSpace;

		/** Tile X Coordinate */
		public int x;

		/** Tile Z Coordinate */
		public int z;

		/** Width, in tile coordinates.
		 * \warning Widths other than 1 are not supported. This is mainly here for possible future features.
		 */
		public int w;

		/** Depth, in tile coordinates.
		 * \warning Depths other than 1 are not supported. This is mainly here for possible future features.
		 */
		public int d;

		/** All nodes in the tile */
		public TriangleMeshNode[] nodes { get; set; }

		/** Bounding Box Tree for node lookups */
		public BBTree bbTree;

		/** Temporary flag used for batching */
		public bool flag;

		public NavmeshBase graph;

		#region INavmeshHolder implementation

		public void GetTileCoordinates (int tileIndex, out int x, out int z) {
			x = this.x;
			z = this.z;
		}

		public int GetVertexArrayIndex (int index) {
			return index & NavmeshBase.VertexIndexMask;
		}

		/** Get a specific vertex in the tile */
		public VInt3 GetVertex (int index) {
			int idx = index & NavmeshBase.VertexIndexMask;

			return verts[idx];
		}

		public VInt3 GetVertexInGraphSpace (int index) {
			return vertsInGraphSpace[index & NavmeshBase.VertexIndexMask];
		}

		/** Transforms coordinates from graph space to world space */
		public GraphTransform transform { get { return graph.transform; } }

		#endregion

		public void GetNodes (System.Action<GraphNode> action) {
			if (nodes == null) return;
			for (int i = 0; i < nodes.Length; i++) action(nodes[i]);
		}
	}
}
