using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    
    public class PathfindingUtility
    {

        static GraphHitInfo hit = new GraphHitInfo();
        /// <summary>
        /// 判断两点之间是否有障碍，返回布尔值
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public static bool isHit(VInt3 startPoint, VInt3 endPoint)
        {
            GraphNode startNode = AstarPath.active.GetNearest(startPoint).node;
            var graph = AstarData.GetGraph(startNode) as NavmeshBase;
            return graph.Linecast(startPoint, endPoint, startNode, out hit);
        }

        /// <summary>
        /// 判断两点之间是否有障碍，并返回碰撞点信息
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="hit"></param>
        /// <returns></returns>
        public static bool isHit(VInt3 startPoint, VInt3 endPoint, out GraphHitInfo hit)
        {
            GraphNode startNode = AstarPath.active.GetNearest(startPoint).node;
            var graph = AstarData.GetGraph(startNode) as NavmeshBase;
            return graph.Linecast(startPoint, endPoint, startNode, out hit);
        }
        
    }

}