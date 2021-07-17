using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitTestScript : MonoBehaviour
{
  [SerializeField] GameObject wallPrefab;
  [SerializeField] GameObject groundPrefab;

  [SerializeField] float rotaion;

  void Start()
  {
    for (int i = 0; i < 4; i++)
    {
      //if (Random.Range(0f, 1f) < 0.5f) continue;
      //if x or y
      int axis = (i >> 1) & 1;
      int direction = (i & 1) * -1;
      int useOffset = (i ^ 1) & 1;

      Debug.Log("Axis: " + i + " -> " + axis);
      Debug.Log("Direction: " + i + " -> " + direction);
      Debug.Log("Use offset: " + useOffset);
      Debug.Log("--------------");
    }
    float x = 0, y = 0;
    (x, y) = TransformCoordinats(x, y, 15f);
    Debug.Log("Rotaion Vector: " + x + " " + y);
  }

  private (int x, int y) TransformCoordinats(float x, float y, float size)
  {
    float x2, y2;
    x -= size/2f;
    y -= size/2f;
    x2 = x * Mathf.Cos(Mathf.Deg2Rad * rotaion) - y * Mathf.Sin(Mathf.Deg2Rad * rotaion);
    y2 = x * Mathf.Sin(Mathf.Deg2Rad * rotaion) + y * Mathf.Cos(Mathf.Deg2Rad * rotaion);
    x2 += size/2f;
    y2 += size/2f;
    return ((int)x2, (int)y2);
  }


}
