using System.Collections.Generic;
using UnityEngine;

public class Path : Room
{

  public Vector2Int connectionPoint;
  public Path(Vector2Int size) : base(size)
  {
  }

  public void AddConnections(Room a, Room b)
  {
    AddConnection(a);
    AddConnection(b);
  }


}