using System.Collections.Generic;
using UnityEngine;

public class GraphProcessor
{
  public static List<Room[]> GenerateAdditionalConnections(Graph graph, int radius, float probability)
  {
    List<Room[]> newRoomConnections = new List<Room[]>();
    foreach (var leafNode in graph.nodes)
    {
      if (leafNode.connections.Count > Mathf.Floor(1 + 3 * probability)) continue;

      foreach (var node in graph.nodes)
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
    float nodeSpace = (node.GetSize() + leafNode.GetSize() / 2).magnitude;
    return centerDistance - nodeSpace < radius;
  }

  public static Room[] GetEdgeRooms(Graph graph)
  {
    Room topEdge, bottomEdge, leftEdge, rightEdge;
    topEdge = bottomEdge = leftEdge = rightEdge = graph[0];

    foreach (var node in graph.nodes)
    {
      if (node.GetCenter().y < topEdge.GetCenter().y) topEdge = node;
      if (node.GetCenter().y > bottomEdge.GetCenter().y) bottomEdge = node;
      if (node.GetCenter().x < leftEdge.GetCenter().x) leftEdge = node;
      if (node.GetCenter().x > rightEdge.GetCenter().x) rightEdge = node;
    }

    return new Room[] { topEdge, rightEdge, bottomEdge, leftEdge };
  }

  public static List<Room> CalculateConvexHull(Graph graph, Room firstNode)
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

  public static List<Room> GeneratePath(Graph graph, Room startNode, int minPathLength, int maxPathLength, bool endNodeMustLieOnEdge)
  {
    EvaluateGraphConnections(graph, startNode);
    Room currentNode = GetRandomeNodeFromGraph(graph, minPathLength, maxPathLength, endNodeMustLieOnEdge);

    return GetShortestPathFromOriginToNode(currentNode);
  }

  // Evaluates Graph and returns shortest path between the two given nodes
  public static List<Room> GetShortestPathBetweenNodes(Graph graph, Room startNode, Room endNode)
  {
    EvaluateGraphConnections(graph, startNode);
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

  //evaluates the graph and marks the shortest path from the parent
  //node to each other node within the graph
  public static void EvaluateGraphConnections(Graph graph, Room originNode)
  {
    if (graph.originNode == originNode) return;

    graph.ResetEvaluation();
    originNode.pathDistance = 0;
    originNode.pathParent = null;
    SimplifiedDijkstraRecursion(graph, originNode);
    graph.originNode = originNode;
  }

  static Queue<Room> callQueue = new Queue<Room>();
  static void SimplifiedDijkstraRecursion(Graph graph, Room parentNode)
  {
    float distance = parentNode.pathDistance + 1;
    foreach (var node in parentNode.connections)
    {
      if (node.pathDistance <= distance) continue;
      if (graph.maxDistanceFromOrigin < distance) graph.maxDistanceFromOrigin = (int)distance;

      node.pathDistance = distance;
      node.pathParent = parentNode;
      callQueue.Enqueue(node);
    }

    while (callQueue.Count > 0)
    {
      SimplifiedDijkstraRecursion(graph, callQueue.Dequeue());
    }
  }

  public static List<Room> GetRandomNodesWithDistanceFromOrigin(Graph graph, Room startRoom, int amount, int mininalDistanceToOrigin)
  {
    if (amount >= graph.nodes.Count) return graph.nodes;
    if (graph.originNode != startRoom) EvaluateGraphConnections(graph, startRoom);
    mininalDistanceToOrigin = Mathf.Min(mininalDistanceToOrigin, graph.maxDistanceFromOrigin);

    //select valid rooms
    List<Room> validNodes = new List<Room>();
    do
    {
      foreach (var node in graph.nodes)
      {
        if (node.pathDistance >= mininalDistanceToOrigin && !validNodes.Contains(node)) validNodes.Add(node);
      }
      mininalDistanceToOrigin--;
    } while (validNodes.Count < amount);

    //select amount of nodes from valid possibilities
    List<Room> selectedNodes = new List<Room>();
    foreach (var i in GetRandomNumbersWithoutDuplicate(validNodes.Count - 1, amount))
    {
      selectedNodes.Add(validNodes[i]);
    }
    return selectedNodes;
  }

  static int[] GetRandomNumbersWithoutDuplicate(int range, int amount)
  {
    int[] list = new int[amount];
    int[] selectionSet = new int[range + 1];
    for (int i = 0; i <= range; i++)
    {
      selectionSet[i] = i;
    }

    int remainingRange = range;
    for (int i = 0; i < amount; i++)
    {
      int randomNumber = Random.Range(0, remainingRange);
      list[i] = selectionSet[randomNumber];
      selectionSet[randomNumber] = selectionSet[remainingRange];
      remainingRange--;
    }

    return list;
  }

  //dependent on the given parameters finds an end node within the evaluaded graph
  //if parameters can't be fullfilled, returns the node closest to the given propertys
  private static Room GetRandomeNodeFromGraph(Graph graph, int minPathLength, int maxPathLength, bool nodeMustLieOnEdge)
  {
    minPathLength = Mathf.Min(graph.maxDistanceFromOrigin, minPathLength);
    List<Room> validCanidates = new List<Room>();
    foreach (var node in graph.nodes)
    {
      if (nodeMustLieOnEdge && !node.isEdgeRoom) continue;
      if (node.pathDistance >= minPathLength && node.pathDistance <= maxPathLength) validCanidates.Add(node);
    }

    return validCanidates[Random.Range(0, validCanidates.Count)];
  }
}
