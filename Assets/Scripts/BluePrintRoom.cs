using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluePrintRoom : Room
{
  Texture2D blueprint;
  float rotation;
  new public Vector2Int size => GetSize();

  public BluePrintRoom(Vector2Int size, Texture2D blueprint) : base(size)
  {
    base.size = new Vector2Int(blueprint.width, blueprint.height);
    this.blueprint = blueprint;
    rotation = UnityEngine.Random.Range(-1, 3) * 90;
  }

  Vector2Int GetSize()
  {
    return (rotation == 90 || rotation == -90) ? new Vector2Int(base.size.y, base.size.x) : base.size;
  }

  public Color GetBlueprintPixel(int x, int y)
  {
    (x, y) = TransformCoordinats(x, y, blueprint.height);
    return blueprint.GetPixel(x, blueprint.height - y - 1);
  }

  private (int x, int y) TransformCoordinats(float x, float y, float size)
  {
    float x2, y2;
    x -= size / 2f;
    y -= size / 2f;
    x2 = x * Mathf.Cos(Mathf.Deg2Rad * rotation) - y * Mathf.Sin(Mathf.Deg2Rad * rotation);
    y2 = x * Mathf.Sin(Mathf.Deg2Rad * rotation) + y * Mathf.Cos(Mathf.Deg2Rad * rotation);
    x2 += size / 2f;
    y2 += size / 2f;
    return ((int)x2, (int)y2);
  }

  // public void AdjustRoomOrientation(Vector2Int direction, byte directionIndex)
  // {
  //   //rotation of room is already right
  //   if (freeWays[(int)directionIndex] == 1) return;


  // }

}
