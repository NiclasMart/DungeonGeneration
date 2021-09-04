using System.Collections.Generic;
using UnityEngine;

public class Room
{
  public Vector2Int size;
  public Vector2Int position;
  public List<Room> connections = new List<Room>();
  public bool isEdgeRoom;
  public float pathDistance = Mathf.Infinity;
  public Room pathParent;

  public Room(Vector2Int size)
  {
    this.size = size;
  }

  public void SetPosition(Vector2Int topCorner)
  {
    this.position = topCorner;
  }

  virtual public Vector2Int GetSize()
  {
    return size;
  }

  public Vector2Int GetCenter()
  {
    return position + (GetSize() / 2);
  }

  public Vector3 GetCenterWorld()
  {
    return new Vector3(GetCenter().y, 0, GetCenter().x);
  }

  public Vector2Int GetBottomRight()
  {
    return position + GetSize() - Vector2Int.one;
  }

  public Vector2Int GetBottomLeft()
  {
    return position + new Vector2Int(0, GetSize().y - 1);
  }

  public void AddConnection(Room room)
  {
    if (connections.Contains(room) || room == this) return;
    connections.Add(room);
  }

  public void AddConnections(List<Room> rooms)
  {
    foreach (var room in rooms)
    {
      AddConnection(room);
    }
  }


}