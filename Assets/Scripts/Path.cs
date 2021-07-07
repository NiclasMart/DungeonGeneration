using System.Collections.Generic;
using UnityEngine;

class Path
{
  //public Vector2Int origin;
  public List<Room> connectedRooms = new List<Room>();

  public Path(Room a, Room b)
  {
    connectedRooms.Add(a);
    connectedRooms.Add(b);
  }
}