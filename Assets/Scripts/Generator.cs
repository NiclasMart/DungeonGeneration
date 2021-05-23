using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
  [SerializeField] int tileSize;  //how big the prefab tiles are

  [SerializeField] int dungeonSize;
  [SerializeField] float roomReductionOverTime = 0.001f;
  [SerializeField] Vector2Int roomDimensionX;
  [SerializeField] Vector2Int roomDimensionY;
  [SerializeField] int pathWidth;

  [SerializeField] GameObject groundPrefab;
  [SerializeField] GameObject wallPrefab;
  [SerializeField] GameObject debugCube;

  BitMatrix matrix;
  List<Room> rooms = new List<Room>();

  float tileOffset;

  private void Awake()
  {
    tileOffset = tileSize / 2f;
    matrix = new BitMatrix(dungeonSize);
  }

  private void Start()
  {
    StartGeneration();
    PlaceFloor();
  }



  public void StartGeneration()
  {
    Room newRoom = GenerateRoom();
    int positionX = Random.Range(1, matrix.size - newRoom.size.x - 1);
    int positionY = Random.Range(1, matrix.size - newRoom.size.y - 1);
    newRoom.SetPosition(new Vector2Int(positionX, positionY));

    SaveRoom(newRoom);
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
    Stack<Room> newRooms = new Stack<Room>();
    for (int i = 0; i < 4; i++)
    {
      if (Random.Range(0f, 1f) > roomProbebility) continue;

      int axis = (i >> 1) & 1; //if x (axis == 0) or y (axis == 1)
      int direction = (i & 1) == 1 ? -1 : 1; //if positive or negative
      int useParentRoomOffset = (i ^ 1) & 1; //use room size as offset parameter

      Room newRoom = GenerateRoom();

      //calculate distance offset parameters
      Vector2Int minOffset = (parentRoom.size * useParentRoomOffset) + (newRoom.size * (useParentRoomOffset ^ 1));
      int randomDistanceOffset = Random.Range(0, 5);
      Vector2Int axisOffsetVector = new Vector2Int((randomDistanceOffset + minOffset.x) * (axis ^ 1), (randomDistanceOffset + minOffset.y) * axis);

      //calculate room offset Position
      Vector2Int newPos = parentRoom.position + direction * axisOffsetVector;

      //calculate room side shift
      int xSideOffset = Random.Range(-newRoom.size.x + pathWidth, parentRoom.size.x - pathWidth) * axis;
      int ySideOffset = Random.Range(-newRoom.size.y + pathWidth, parentRoom.size.y - pathWidth) * (axis ^ 1);
      Vector2Int randomSideOffset = new Vector2Int(xSideOffset, ySideOffset);

      //calculate final room position
      newPos += randomSideOffset;

      newRoom.SetPosition(newPos);
      if (SaveRoom(newRoom))
      {
        newRooms.Push(newRoom);
        rooms.Add(newRoom);
        if (randomDistanceOffset == 0) continue;

        //calculate path

        //calculate path start position
        int adjustPathOffset = (randomSideOffset.x + randomSideOffset.y) < 0 ? 0 : 1;
        Vector2Int minPathOffset = (useParentRoomOffset == 1) ? minOffset * new Vector2Int(axis ^ 1, axis) : new Vector2Int(axis ^ 1, axis) * -1;
        Vector2Int pathOrigin = parentRoom.position + randomSideOffset * adjustPathOffset + minPathOffset;

        //calculate path random offset
        int pathXOffset = 0, pathYOffset = 0;
        if (axis == 1)
        {
          int pathXOffsetRange = Mathf.Min(parentRoom.GetTopRight().x, newRoom.GetTopRight().x) - pathOrigin.x;
          pathXOffset = Random.Range(0, pathXOffsetRange - (pathWidth - 1));
        }
        else
        {
          int pathYOffsetRange = Mathf.Min(parentRoom.GetBottomLeft().y, newRoom.GetBottomLeft().y) - pathOrigin.y;
          pathYOffset = Random.Range(0, pathYOffsetRange - (pathWidth - 1));
        }
        Vector2Int randomPathOffset = new Vector2Int(pathXOffset, pathYOffset);

        pathOrigin += randomPathOffset;

        //set path
        SavePath(pathOrigin, randomDistanceOffset, direction, axis);
      }
    }
    roomProbebility -= roomReductionOverTime;
    while (newRooms.Count > 0)
    {
      GenerateRecursivly(newRooms.Pop());
    }
  }

  int matrixOffset => matrix.size / 2;
  bool SaveRoom(Room room)
  {
    if (room.position.x < 1 || room.position.x + room.size.x > matrix.size - 2) return false;
    if (room.position.y < 1 || room.position.y + room.size.y > matrix.size - 2) return false;

    if (!CheckIfPositionIsFree(room)) return false;

    for (int i = room.position.x; i < room.position.x + room.size.x; i++)
    {
      for (int j = room.position.y; j < room.position.y + room.size.y; j++)
      {
        matrix.SetValue(i, j, true);
      }
    }
    return true;
  }

  void SavePath(Vector2Int startPos, int length, int direction, int axis)
  {
    for (int i = 0; i < length; i++)
    {
      for (int j = 0; j < pathWidth; j++)
      {
        Vector2Int pathTile = startPos + (direction * new Vector2Int((axis ^ 1), axis) * i) + new Vector2Int(axis, (axis ^ 1)) * j;
        matrix.SetValue(pathTile.x, pathTile.y, true);
      }
    }
  }

  private bool CheckIfPositionIsFree(Room room)
  {
    for (int i = room.position.x; i < room.position.x + room.size.x; i++)
    {
      for (int j = room.position.y; j < room.position.y + room.size.y; j++)
      {
        if (matrix.GetValue(i, j)) return false;
      }
    }
    return true;
  }

  public void PlaceFloor()
  {
    for (int i = 1; i < matrix.size - 1; i++)
    {
      for (int j = 1; j < matrix.size - 1; j++)
      {
        if (matrix.GetValue(i, j))
        {
          Instantiate(groundPrefab, new Vector3(j * tileSize, 0, i * tileSize), Quaternion.identity);
          CheckForWallPlacement(i, j);
        }
      }
    }
  }

  void CheckForWallPlacement(int x, int y)
  {
    if (!matrix.GetValue(x, y - 1))
    {
      GameObject wall = Instantiate(wallPrefab, new Vector3(y * tileSize - tileSize / 2, 0, x * tileSize), Quaternion.identity);
      wall.transform.LookAt(new Vector3(y * tileSize, 0, x * tileSize));
    }
    if (!matrix.GetValue(x - 1, y))
    {
      GameObject wall = Instantiate(wallPrefab, new Vector3(y * tileSize, 0, x * tileSize - tileSize / 2), Quaternion.identity);
      wall.transform.LookAt(new Vector3(y * tileSize, 0, x * tileSize));
    }
    if (!matrix.GetValue(x + 1, y))
    {
      GameObject wall = Instantiate(wallPrefab, new Vector3(y * tileSize, 0, x * tileSize + tileSize / 2), Quaternion.identity);
      wall.transform.LookAt(new Vector3(y * tileSize, 0, x * tileSize));
    }
    if (!matrix.GetValue(x, y + 1))
    {
      GameObject wall = Instantiate(wallPrefab, new Vector3(y * tileSize + tileSize / 2, 0, x * tileSize), Quaternion.identity);
      wall.transform.LookAt(new Vector3(y * tileSize, 0, x * tileSize));
    }
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.red;
    int size = (dungeonSize - 1) * tileSize;
    Vector3 topRight = new Vector3(0, 0, size);
    Vector3 bottomLeft = new Vector3(size, 0, 0);
    Vector3 bottomRight = new Vector3(size, 0, size);

    Gizmos.DrawLine(Vector2.zero, topRight);
    Gizmos.DrawLine(topRight, bottomRight);
    Gizmos.DrawLine(bottomRight, bottomLeft);
    Gizmos.DrawLine(bottomLeft, Vector3.zero);
  }
}
