using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
  public List<Room> nodes;
  public Room originNode;
  public int maxDistanceFromOrigin;

  public int Count => nodes.Count;

  public Graph()
  {
    this.nodes = new List<Room>();
  }

  public void AddNode(Room node)
  {
    nodes.Add(node);
  }

  public void ResetEvaluation()
  {
    foreach (var node in nodes)
    {
      node.pathDistance = Mathf.Infinity;
      node.pathParent = null;
    }
    maxDistanceFromOrigin = 0;
    originNode = null;
  }

  public Room this[int key]
  {
    get => nodes[key];
    set => nodes[key] = value;
  }
}
