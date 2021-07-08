using System.Collections.Generic;
using UnityEngine;

class Path : Room
{
  public Path(Vector2Int size) : base(size)
  {
  }

  public void AddConnections(Room a, Room b)
  {
    connections.Add(a);
    connections.Add(b);
  }


}