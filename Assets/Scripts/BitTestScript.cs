using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BitTestScript : MonoBehaviour
{
  // Start is called before the first frame update
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
  }


}
