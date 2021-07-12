using System;
using System.Collections;
using UnityEngine;

class BitMatrix
{
  public BitArray[] data; // an array of arrays
  public int size; // dimension

  public BitMatrix(int n)
  {
    if (n == 0) throw new Exception("Cant Create BitMatrix of size 0.");

    size = n;
    this.data = new BitArray[n];
    for (int i = 0; i < data.Length; ++i)
    {
      this.data[i] = new BitArray(n);
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

    for (int i = 0; i < a.size; i++)
    {
      a.data[i].Or(b.data[i]);
    }
    return a;
  }

}