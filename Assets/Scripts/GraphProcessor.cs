using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphProcessor
{
  public static List<Room[]> GenerateLeafConnections(List<Room> graph, int radius, float probability)
  {
    List<Room[]> newRoomConnections = new List<Room[]>();
    foreach (var leafNode in graph)
    {
      if (leafNode.connections.Count > 1) continue;

      foreach (var node in graph)
      {
        if (node == leafNode || leafNode.connections[0] == node) continue;
        if (Vector2Int.Distance(node.GetCenter(), leafNode.GetCenter()) > radius) continue;
        if (UnityEngine.Random.Range(0, 1f) > probability) continue;

        node.AddConnection(leafNode);
        leafNode.AddConnection(node);
        newRoomConnections.Add(new Room[] { leafNode, node });
      }
    }
    return newRoomConnections;
  }




}
