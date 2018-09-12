using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class PathfindingUtility
{

    public static bool isCollide(VInt3 origin, VInt3 end)
    {
        GraphNode startNode = AstarPath.active.GetNearest(origin).node;
        var graph = AstarData.GetGraph(startNode) as NavmeshBase;
        return graph.Linecast(origin, end, startNode);
    }

    public static bool isCollide(VInt3 origin, VInt3 end, out GraphHitInfo hit)
    {
        GraphNode startNode = AstarPath.active.GetNearest(origin).node;
        var graph = AstarData.GetGraph(startNode) as NavmeshBase;
        return graph.Linecast(origin, end, startNode, out hit);
    }
}
