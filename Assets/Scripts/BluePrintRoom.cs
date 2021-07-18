using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluePrintRoom : Room
{
  Texture2D blueprint;
  public float rotation;

  public BluePrintRoom(Vector2Int size, Texture2D blueprint) : base(size)
  {
    base.size = new Vector2Int(blueprint.width, blueprint.height);
    this.blueprint = blueprint;
    rotation = UnityEngine.Random.Range(-1, 3) * 90;
  }

  public override Vector2Int GetSize()
  {
    return (rotation == 90 || rotation == -90) ? new Vector2Int(base.size.y, base.size.x) : base.size;
  }

  public bool GetBlueprintPixel(int x, int y)
  {
    y = GetSize().y - y - 1;
    (x, y) = TransformCoordinats(x, y, GetSize().x - 1, GetSize().y - 1);
    return blueprint.GetPixel(x, y) == Color.black;
  }

  private (int x, int y) TransformCoordinats(float x, float y, float sizeX, float sizeY)
  {
    float x2, y2;
    x -= sizeX / 2f;
    y -= sizeY / 2f;
    x2 = x * Mathf.Cos(Mathf.Deg2Rad * rotation) - y * Mathf.Sin(Mathf.Deg2Rad * rotation);
    y2 = x * Mathf.Sin(Mathf.Deg2Rad * rotation) + y * Mathf.Cos(Mathf.Deg2Rad * rotation);
    bool swapOffset = (rotation == 90 || rotation == -90);
    x2 += swapOffset ? sizeY / 2f : sizeX / 2f;
    y2 += swapOffset ? sizeX / 2f : sizeY / 2f;
    return (Mathf.RoundToInt(x2), Mathf.RoundToInt(y2));
  }
}
