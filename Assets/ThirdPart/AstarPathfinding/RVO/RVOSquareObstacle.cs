using UnityEngine;

namespace Pathfinding.RVO {
	/**
	 * Square Obstacle for RVO Simulation.
	 *
	 * \astarpro
	 */
	[AddComponentMenu("Pathfinding/Local Avoidance/Square Obstacle")]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_r_v_o_1_1_r_v_o_square_obstacle.php")]
	public class RVOSquareObstacle : RVOObstacle {
        /** Height of the obstacle */
	    //GG
        //public float height = 1;
        public int height = 1000;

        /** Size of the square */
	    //GG
        //public Vector2 size = Vector3.one;
        public VInt2 size = new VInt2(1000, 1000);

        /** Center of the square */
	    //GG
        //public Vector2 center = Vector3.zero;
        public VInt2 center = new VInt2(1000, 1000);

		protected override bool StaticObstacle { get { return false; } }
		protected override bool ExecuteInEditor { get { return true; } }
		protected override bool LocalCoordinates { get { return true; } }
	    //GG
        //protected override float Height { get { return height; } }
        protected override int Height { get { return height; } }

		//If UNITY_EDITOR to save a few bytes, these are only needed in the editor
	#if UNITY_EDITOR
		private Vector2 _size;
		private Vector2 _center;
		private float _height;
	#endif

		protected override bool AreGizmosDirty () {
#if UNITY_EDITOR
		    //GG
            /*bool ret = _size != size || _height != height || _center != center;
			_size = size;
			_center = center;*/
            bool ret = _size != (Vector2)size || _height != height || _center != (Vector2)center;
			_size = (Vector2)size;
			_center = (Vector2)center;
			_height = height;
			return ret;
	#else
			return false;
	#endif
		}

		protected override void CreateObstacles () {
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			height = Mathf.Abs(height);

            //GG
            /*var verts = new [] { new Vector3(1, 0, -1), new Vector3(1, 0, 1), new Vector3(-1, 0, 1), new Vector3(-1, 0, -1) };
			for (int i = 0; i < verts.Length; i++) {
				verts[i].Scale(new Vector3(size.x * 0.5f, 0, size.y * 0.5f));
				verts[i] += new Vector3(center.x, 0, center.y);
			}

			AddObstacle(verts, height);*/
		    VInt3[] array = new VInt3[]
		    {
		        new VInt3(1, 0, -1),
		        new VInt3(1, 0, 1),
		        new VInt3(-1, 0, 1),
		        new VInt3(-1, 0, -1)
		    };
		    for (int i = 0; i < array.Length; i++)
		    {
		        array[i] *= new VInt3(this.size.x >> 1, 0, this.size.y >> 1);
		        array[i] += new VInt3(this.center.x, 0, this.center.y);
		    }
		    base.AddObstacle(array, this.height);
        }
	}
}
