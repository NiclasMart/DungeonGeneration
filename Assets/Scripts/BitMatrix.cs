using System;
using System.Collections;
using UnityEngine;

class BitMatrix
{
  public BitArray[] data; // an array of arrays
  public Vector2Int size; // dimension

  public BitMatrix(int n, float ratio)
  {
    if (n == 0) throw new Exception("Cant Create BitMatrix of size 0.");

    size = new Vector2Int((int)(n / ratio), (int)(n * ratio));
    this.data = new BitArray[size.x];
    for (int i = 0; i < data.Length; ++i)
    {
      this.data[i] = new BitArray(size.y);
    }
  }

  public bool GetValue(int col, int row)
  {
    return data[col][row];
  }

  public void SetValue(int col, int row, bool value)
  {
    data[col][row] = value;
  }

  public string Display()
  {
    string s = "";
    for (int i = 0; i < data.Length; ++i)
    {
      for (int j = 0; j < data[i].Length; ++j)
      {
        if (data[i][j] == true) s += "1 "; else s += "0 ";
      }
      s += Environment.NewLine;
    }
    return s;
  }

  public static BitMatrix operator +(BitMatrix a, BitMatrix b)
  {
    if (a.size != b.size) throw new Exception("Adding two BitMatrices of diffeent sizes is not allowed!");

    for (int i = 0; i < a.size.x; i++)
    {
      a.data[i].Or(b.data[i]);
    }
    return a;
  }

}