using UnityEngine;

class Room
{
  public Vector2Int size;
  public Vector2Int topCorner;
  public Room[] connections = new Room[4];

  public Room(Vector2Int size)
  {
    this.size = size;
  }

  public void SetPosition(Vector2Int topCorner)
  {
    this.topCorner = topCorner;
  }

  public Vector2 GetCenter(){
    return topCorner + (size/2);
  }


}