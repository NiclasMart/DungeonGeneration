using UnityEngine;

class Room
{
  public Vector2Int size;
  public Vector2Int position;
  public Room[] connections = new Room[4];

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

  public Vector2Int GetTopRight()
  {
    return position + new Vector2Int(size.x - 1, 0);
  }

  public Vector2Int GetBottomLeft()
  {
    return position + new Vector2Int(0, size.y - 1);
  }


}