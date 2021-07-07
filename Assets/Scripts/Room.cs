using System.Collections.Generic;
using UnityEngine;

class Room
{
  public Vector2Int size;
  public Vector2Int position;
  public List<Room> connections = new List<Room>();

  public Room(Vector2Int size)
  {
    this.size = size;
  }

  public void SetPosition(Vector2Int topCorner)
  {
    this.position = topCorner;
  }

  public Vector2Int GetCenter()
  {
    return position + (size / 2);
  }

  public Vector3 GetCenterWorld()
  {
    return new Vector3(GetCenter().y, 0, GetCenter().x);
  }

  public Vector2Int GetTopRight()
  {
    return position + new Vector2Int(size.x - 1, 0);
  }

  public Vector2Int GetBottomLeft()
  {
    return position + new Vector2Int(0, size.y - 1);
  }


}