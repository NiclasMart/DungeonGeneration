using System;
using System.Collections;
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
      if (node.GetCenter().x < topEdge.GetCenter().x) topEdge = node;
      if (node.GetCenter().x > bottomEdge.GetCenter().x) bottomEdge = node;
      if (node.GetCenter().y < leftEdge.GetCenter().y) leftEdge = node;
      if (node.GetCenter().y > rightEdge.GetCenter().y) rightEdge = node;
    }

    return new Room[] { topEdge, bottomEdge, leftEdge, rightEdge };
  }
}
