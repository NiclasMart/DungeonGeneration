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
    StartGeneration();
    Place();
  }

  BitMatrix matrix = new BitMatrix(100);
  List<Room> rooms = new List<Room>();

  public void StartGeneration()
  {
    Room newRoom = GenerateRoom();
    int positionX = Random.Range(0, matrix.size - newRoom.topCorner.x);
    int positionY = Random.Range(0, matrix.size - newRoom.topCorner.y);
    newRoom.SetPosition(new Vector2Int(positionX, positionY));

    FillMatrix(newRoom);
    rooms.Add(newRoom);
    GenerateRecursivly(newRoom);

  }

  Room GenerateRoom()
  {
    int sizeX = Random.Range(roomDimensionX.x, roomDimensionX.y);
    int sizeY = Random.Range(roomDimensionY.x, roomDimensionY.y);
    return new Room(new Vector2Int(sizeX, sizeY));
  }

  float roomProbebility = 1f;
  void GenerateRecursivly(Room parentRoom)
  {
    //if (currentIterration >= iterations) return;
    //currentIterration++;
    Stack<Room> newRooms = new Stack<Room>();
    for (int i = 0; i < 4; i++)
    {
      if (Random.Range(0f, 1f) > roomProbebility) continue;

      int axis = (i >> 1) & 1; //if x (axis == 0) or y (axis == 1)
      int direction = (i & 1) == 1 ? -1 : 1; //if positive or negative
      int useOffset = (i ^ 1) & 1; //use room size as offset parameter

      Room newRoom = GenerateRoom();
      Vector2Int minOffset = (parentRoom.size * useOffset) + (newRoom.size * (useOffset ^ 1));
      int randomOffset = Random.Range(0, 5);
      Vector2Int axisOffsetVector = new Vector2Int((randomOffset + minOffset.x) * (axis ^ 1), (randomOffset + minOffset.y) * axis);
      Vector2Int newPos = parentRoom.topCorner + direction * axisOffsetVector;

      newRoom.SetPosition(newPos);
      if (FillMatrix(newRoom))
      {
        newRooms.Push(newRoom);
        rooms.Add(newRoom);
      }
    }
    roomProbebility -= 0.01f;
    while (newRooms.Count > 0)
    {
      GenerateRecursivly(newRooms.Pop());
    }
  }

  int matrixOffset => matrix.size / 2;
  bool FillMatrix(Room room)
  {
    if (room.topCorner.x < 0 || room.topCorner.x + room.size.x > matrix.size - 2) return false;
    if (room.topCorner.y < 0 || room.topCorner.y + room.size.y > matrix.size - 2) return false;

    if (!CheckIfPositionIsFree(room)) return false;

    for (int i = room.topCorner.x; i < room.topCorner.x + room.size.x; i++)
    {
      for (int j = room.topCorner.y; j < room.topCorner.y + room.size.y; j++)
      {
        matrix.SetValue(i, j, true);
      }
    }
    return true;
  }

  private bool CheckIfPositionIsFree(Room room)
  {
    for (int i = room.topCorner.x; i < room.topCorner.x + room.size.x; i++)
    {
      for (int j = room.topCorner.y; j < room.topCorner.y + room.size.y; j++)
      {
        if (matrix.GetValue(i, j)) return false;
      }
    }
    return true;
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
