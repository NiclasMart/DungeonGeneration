using UnityEngine;

class Room
{
  public Vector2Int size;
  public Vector2Int topCorner;

  public Room(Vector2Int size, Vector2Int topCorner)
  {
    this.size = size;
    this.topCorner = topCorner;
  }
}