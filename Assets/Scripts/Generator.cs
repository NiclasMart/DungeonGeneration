using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
  [SerializeField] int tileSize;  //how big the prefab tiles are

  [SerializeField] int roomCount;
  [SerializeField] Vector2Int roomDimensionX;
  [SerializeField] Vector2Int roomDimensionY;

  [SerializeField] GameObject groundPrefab;

  float tileOffset;

  private void Awake()
  {
    tileOffset = tileSize / 2f;

  }

  private void Start()
  {
    Generate();
    Place();
  }

  BitMatrix matrix = new BitMatrix(100);
  List<Room> rooms = new List<Room>();

  public void Generate()
  {
    int sizeX = Random.Range(roomDimensionX.x, roomDimensionX.y);
    int sizeY = Random.Range(roomDimensionY.x, roomDimensionY.y);
    int positionX = Random.Range(0, matrix.size - sizeX);
    int positionY = Random.Range(0, matrix.size - sizeY);
    Room newRoom = new Room(new Vector2Int(sizeX, sizeY), new Vector2Int(positionX, positionY));
    FillMatrix(newRoom);
    rooms.Add(newRoom);
  }

  int matrixOffset => matrix.size / 2;
  void FillMatrix(Room room)
  {
    for (int i = room.topCorner.x; i < room.topCorner.x + room.size.x; i++)
    {
      for (int j = room.topCorner.y; j < room.topCorner.y + room.size.y; j++)
      {
        matrix.SetValue(i, j, true);
      }
    }
  }

  public void Place()
  {
    for (int i = 0; i < matrix.size; i++)
    {
      for (int j = 0; j < matrix.size; j++)
      {
        if (matrix.GetValue(i, j)) Instantiate(groundPrefab, new Vector3(j * tileSize, 0, i * tileSize), Quaternion.identity);
      }
    }
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.red;
    int size = (matrix.size - 1) * tileSize;
    Vector3 topRight = new Vector3(0, 0, size);
    Vector3 bottomLeft = new Vector3(size, 0, 0);
    Vector3 bottomRight = new Vector3(size, 0, size);

    Gizmos.DrawLine(Vector2.zero, topRight);
    Gizmos.DrawLine(topRight, bottomRight);
    Gizmos.DrawLine(bottomRight, bottomLeft);
    Gizmos.DrawLine(bottomLeft, Vector3.zero);
  }
}
