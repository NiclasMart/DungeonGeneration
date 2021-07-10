using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphProcessor
{
  public static void GenerateLeafConnections(List<Room> graph, int radius)
  {
    foreach (var leafNode in graph)
    {
      if (leafNode.connections.Count == 1)
      {
        foreach (var node in graph)
        {
          if (node == leafNode || leafNode.connections[0] == node) continue;

          float roomDistance = Vector2Int.Distance(node.GetCenter(), leafNode.GetCenter());
          if (roomDistance <= radius && RoomsCanBeConnected(leafNode, node))
          {
            node.AddConnection(leafNode);
            leafNode.AddConnection(node);
          }
        }
      }
    }
  }



  private static bool RoomsCanBeConnected(Room leafNode, Room node)
  {
    if (node.GetBottomRight().x > leafNode.position.x && node.position.x < leafNode.GetBottomRight().x)
    {
      int minXOffset = Mathf.Max(0, node.position.x - leafNode.position.x);
      int maxXOffset = Mathf.Min(leafNode.size.x, node.GetBottomRight().x - leafNode.position.x);

      int XOffset = UnityEngine.Random.Range(minXOffset, maxXOffset);
      Path path = new Path(2, distance);
      path.SetPosition()
      return true;
    }
    if (node.GetBottomRight().y > leafNode.position.y && node.position.y < leafNode.GetBottomRight().y) return true;

    return false;
  }
}
