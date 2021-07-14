using System.Collections.Generic;
using UnityEngine;

public class GraphProcessor
{
  public static List<Room[]> GenerateAdditionalConnections(List<Room> graph, int radius, float probability)
  {
    List<Room[]> newRoomConnections = new List<Room[]>();
    foreach (var leafNode in graph)
    {
      if (leafNode.connections.Count > Mathf.Floor(1 + 3 * probability)) continue;

      foreach (var node in graph)
      {
        if (node == leafNode || leafNode.connections[0] == node) continue;
        if (!NodesCloseEnought(radius, leafNode, node)) continue;
        if (UnityEngine.Random.Range(0, 1f) > probability) continue;

        newRoomConnections.Add(new Room[] { leafNode, node });
      }
    }
    return newRoomConnections;
  }

  private static bool NodesCloseEnought(int radius, Room leafNode, Room node)
  {
    float centerDistance = Vector2Int.Distance(node.GetCenter(), leafNode.GetCenter());
    float nodeSpace = (node.size + leafNode.size / 2).magnitude;
    return centerDistance - nodeSpace < radius;
  }

  public static Room[] GetEdgeRooms(List<Room> graph)
  {
    Room topEdge, bottomEdge, leftEdge, rightEdge;
    topEdge = bottomEdge = leftEdge = rightEdge = graph[0];

    foreach (var node in graph)
    {
      if (node.GetCenter().y < topEdge.GetCenter().y) topEdge = node;
      if (node.GetCenter().y > bottomEdge.GetCenter().y) bottomEdge = node;
      if (node.GetCenter().x < leftEdge.GetCenter().x) leftEdge = node;
      if (node.GetCenter().x > rightEdge.GetCenter().x) rightEdge = node;
    }

    return new Room[] { topEdge, rightEdge, bottomEdge, leftEdge };
  }

  public static List<Room> CalculateConvexHull(List<Room> graph, Room firstNode)
  {
    List<Room> convexHullSet = new List<Room>();
    Room startNode = firstNode, endNode;
    do
    {
      convexHullSet.Add(startNode);
      startNode.isEdgeRoom = true;
      endNode = graph[0];
      if (startNode == endNode) endNode = graph[1];

      for (int j = 0; j < graph.Count; j++)
      {
        float angle = Vector2.SignedAngle(endNode.GetCenter() - startNode.GetCenter(), startNode.GetCenter() - graph[j].GetCenter());
        if (endNode == startNode || angle < 0) endNode = graph[j];
      }
      startNode = endNode;

    } while (endNode != convexHullSet[0]);

    return convexHullSet;
  }

  public static List<Room> GeneratePath(List<Room> graph, Room startNode, int minPathLength, int maxPathLength, bool endRoomLiesOnEdge)
  {
    List<Room> path = new List<Room>();
    startNode.pathDistance = 0;
    startNode.pathParent = null;

    EvaluateGraphConnections(startNode);

    Room currentNode = GetEndNode(graph, minPathLength, maxPathLength, endRoomLiesOnEdge);

    return GetShortestPathFromOriginToNode(currentNode);
  }

  // Evaluates Graph and returns shortest path between the two given rooms
  public static List<Room> GetShortestPathBetweenNodes(Room startNode, Room endNode)
  {
    startNode.pathDistance = 0;
    startNode.pathParent = null;
    EvaluateGraphConnections(startNode);
    return GetShortestPathFromOriginToNode(endNode);
  }

  //graph must be evaluated beforhand, returns the path between the 
  //origin node for which the graph was evaluated and the given node
  public static List<Room> GetShortestPathFromOriginToNode(Room node)
  {
    List<Room> path = new List<Room>();
    do
    {
      path.Add(node);
      node = node.pathParent;
    } while (node != null);
    path.Reverse();
    return path;
  }

  //dependent on the given parameters finds an end room
  //if parameters can't be fullfilled, returns the closest propertys
  private static Room GetEndNode(List<Room> graph, int minPathLength, int maxPathLength, bool endRoomLiesOnEdge)
  {
    List<Room> validCanidates = new List<Room>();
    Room closestCanidate = graph[0];
    foreach (var node in graph)
    {
      if (endRoomLiesOnEdge && !node.isEdgeRoom) continue;
      if (node.pathDistance > minPathLength && node.pathDistance < maxPathLength) validCanidates.Add(node);
      else if (node.pathDistance > closestCanidate.pathDistance) closestCanidate = node;
    }

    if (validCanidates.Count == 0) return closestCanidate;
    else return validCanidates[Random.Range(0, validCanidates.Count)];
  }

  //evaluates the graph and marks the shortest path from the parent
  //node to each other node within the graph
  public static void EvaluateGraphConnections(Room parentNode)
  {
    foreach (var node in parentNode.connections)
    {
      float distance = parentNode.pathDistance + 1;
      if (node.pathDistance < distance) continue;

      node.pathDistance = distance;
      node.pathParent = parentNode;

      EvaluateGraphConnections(node);
    }
  }
}
