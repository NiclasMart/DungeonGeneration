using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
  [SerializeField] int tileSize;  //how big the prefab tiles are

  // parameter for dungeon size
  [Header("Dungeon Parameters")]
  [SerializeField] int dungeonSize = 100;
  [SerializeField, Min(1)] int roomCount = 50;
  [SerializeField, Range(0, 1)] float compressFactor = 0.1f;
  [SerializeField, Range(0, 1)] float connectionDegree = 0.3f;
  [SerializeField, Range(0, 1)] float shape = 0;
  //----------
  [Header("Room parameters")]
  [SerializeField] Vector2Int roomDimensionX = new Vector2Int(5, 10);
  [SerializeField] Vector2Int roomDimensionY = new Vector2Int(5, 10);
  [SerializeField] Vector2Int roomDistance = new Vector2Int(0, 10);

  [Header("Other")]
  [Tooltip("Defines up to which ratio a room is categorized as squared. The value must be larger than 1 ( 1 = square).")]
  [SerializeField] float minColumnRoomSize = 5f;
  [SerializeField] float columnProbability = 0.3f;
  [SerializeField] Texture2D columnBluePrint;
  [SerializeField] int pathWidth = 2;

  [Header("Tile Prefabs")]
  [SerializeField] GameObject groundPrefab;
  [SerializeField] GameObject wallPrefab;
  [SerializeField] GameObject debugCube;

  [SerializeField] bool generateColumns = true;

  BitMatrix roomMatrix, pathMatrix;
  List<Room> rooms = new List<Room>();
  List<Path> paths = new List<Path>();
  int currentRoomCount = 1;
  Vector2Int shapeArea;

  private void Awake()
  {
    //pre calculate shape area
    float shapeAreaSize = Mathf.Max(dungeonSize - (shape * dungeonSize), 2 * roomDimensionX.x);
    float leftBorder = dungeonSize / 2 - shapeAreaSize / 2;
    float rightBorder = dungeonSize / 2 + shapeAreaSize / 2;
    shapeArea = new Vector2Int((int)leftBorder, (int)rightBorder);

    roomMatrix = new BitMatrix(dungeonSize);
    pathMatrix = new BitMatrix(dungeonSize);
  }

  private void Start()
  {
    StartGeneration();
    StartIterativeImproving();
    GenerateAdditionalConnections();
    PlaceTiles();

    DebugInformation();

    if (generateColumns) GenerateColumns();
  }

  public void StartGeneration()
  {
    Room newRoom = GenerateRoom(Mathf.Min(roomDimensionX.y, shapeArea.y - dungeonSize / 2));
    newRoom.SetPosition(new Vector2Int(roomMatrix.size / 2, roomMatrix.size / 2));

    SetDebugBlock(new Vector2Int(roomMatrix.size / 2, roomMatrix.size / 2));

    SaveRoomToBitMatrix(newRoom);
    rooms.Add(newRoom);

    GenerateRecursivly(newRoom);
  }

  Queue<Room> newRoomsQueue = new Queue<Room>();
  void GenerateRecursivly(Room parentRoom)
  {
    for (int i = 0; i < 4; i++)
    {
      if (currentRoomCount >= roomCount) continue;

      //generate room with path in several attempts
      for (int attempt = 0; attempt < 1 + compressFactor * 20; attempt++)
      {
        if (Generate(parentRoom, i)) break;
      }
    }

    //call generation recursively for each generated room
    while (newRoomsQueue.Count > 0)
    {
      GenerateRecursivly(newRoomsQueue.Dequeue());
    }
  }

  private bool Generate(Room parentRoom, int i)
  {
    int axis = (i >> 1) & 1; //if x (axis == 0) or y (axis == 1)
    int direction = (i & 1) == 1 ? -1 : 1; //if positive or negative
    int useParentRoomOffset = (i ^ 1) & 1; //use room size as offset parameter

    Room newRoom = GenerateRoom();

    //calculate distance offset parameters
    Vector2Int minOffset = (parentRoom.size * useParentRoomOffset) + (newRoom.size * (useParentRoomOffset ^ 1));
    int randomRoomDistanceOffset = Random.Range(roomDistance.x, roomDistance.y);
    Vector2Int axisOffsetVector = new Vector2Int((randomRoomDistanceOffset + minOffset.x) * (axis ^ 1), (randomRoomDistanceOffset + minOffset.y) * axis);

    //calculate room offset Position
    Vector2Int newPos = parentRoom.position + direction * axisOffsetVector;

    //calculate room side shift
    int xSideOffset = Random.Range(-newRoom.size.x + pathWidth, parentRoom.size.x - pathWidth) * axis;
    int ySideOffset = Random.Range(-newRoom.size.y + pathWidth, parentRoom.size.y - pathWidth) * (axis ^ 1);
    Vector2Int randomSideOffset = new Vector2Int(xSideOffset, ySideOffset);

    //calculate final room position
    newPos += randomSideOffset;
    newRoom.SetPosition(newPos);

    if (!RoomPositionIsValid(newRoom)) return false;

    //calculate path
    if (randomRoomDistanceOffset != 0)
    {
      //calculate path start position
      int adjustPathOffset = (randomSideOffset.x + randomSideOffset.y) < 0 ? 0 : 1;
      Vector2Int minPathOffset = (useParentRoomOffset == 1) ? minOffset * new Vector2Int(axis ^ 1, axis) : new Vector2Int(axis ^ 1, axis) * -1;
      Vector2Int pathOrigin = parentRoom.position + randomSideOffset * adjustPathOffset + minPathOffset;

      //calculate path random offset
      int pathXOffset = 0, pathYOffset = 0;
      if (axis == 1)
      {
        int pathXOffsetRange = Mathf.Min(parentRoom.GetBottomRight().x, newRoom.GetBottomRight().x) - pathOrigin.x;
        pathXOffset = Random.Range(0, pathXOffsetRange - (pathWidth - 1));
      }
      else
      {
        int pathYOffsetRange = Mathf.Min(parentRoom.GetBottomRight().y, newRoom.GetBottomRight().y) - pathOrigin.y;
        pathYOffset = Random.Range(0, pathYOffsetRange - (pathWidth - 1));
      }
      Vector2Int randomPathOffset = new Vector2Int(pathXOffset, pathYOffset);
      pathOrigin += randomPathOffset;

      //create new path and check if its valid
      Path newPath = CreatePath(pathOrigin, randomRoomDistanceOffset, direction, axis);
      if (!PathPositionIsValid(newPath, axis)) return false;

      //finalizing path generation
      newPath.AddConnections(parentRoom, newRoom);
      SavePathToBitMatrix(newPath);
      paths.Add(newPath);
    }
    SaveNewRoom(parentRoom, newRoom);
    return true;
  }

  Room GenerateRoom()
  {
    int sizeX = Random.Range(roomDimensionX.x, roomDimensionX.y);
    int sizeY = Random.Range(roomDimensionY.x, roomDimensionY.y);
    return new Room(new Vector2Int(sizeX, sizeY));
  }
  Room GenerateRoom(int xMaxSize)
  {
    int sizeX = Random.Range(roomDimensionX.x, xMaxSize);
    int sizeY = Random.Range(roomDimensionY.x, roomDimensionY.y);
    return new Room(new Vector2Int(sizeX, sizeY));
  }

  private Path CreatePath(Vector2Int pathOrigin, int length, int direction, int axis)
  {
    int xSize, ySize;
    (xSize, ySize) = axis == 0 ? (length, pathWidth) : (pathWidth, length);
    Path newPath = new Path(new Vector2Int(xSize, ySize));
    //sets the start point of the path so that it always is in the top left corner
    newPath.position = direction == -1 ? pathOrigin - new Vector2Int((axis ^ 1), axis) * (newPath.size - Vector2Int.one) : pathOrigin;
    return newPath;
  }

  //finalizes the room generation step
  //saves room and sets up connections
  private void SaveNewRoom(Room parentRoom, Room newRoom)
  {
    SaveRoomToBitMatrix(newRoom);
    currentRoomCount++;
    parentRoom.AddConnection(newRoom);
    newRoom.AddConnection(parentRoom);
    newRoomsQueue.Enqueue(newRoom);
    rooms.Add(newRoom);
  }

  void SaveRoomToBitMatrix(Room room)
  {
    for (int i = room.position.x; i < room.position.x + room.size.x; i++)
    {
      for (int j = room.position.y; j < room.position.y + room.size.y; j++)
      {
        roomMatrix.SetValue(i, j, true);
      }
    }
  }

  void SavePathToBitMatrix(Path path)
  {
    //iterate over all path tiles
    for (int i = 0; i < path.size.x; i++)
    {
      for (int j = 0; j < path.size.y; j++)
      {
        Vector2Int pathTileIndex = path.position + new Vector2Int(i, j);

        //handle path crossing within node system
        if (pathMatrix.GetValue(pathTileIndex.x, pathTileIndex.y))
        {
          Path crossPath = GetPathAtPosition(pathTileIndex);
          if (crossPath == null) break;

          AddCrossingConnections(path, crossPath);
        }
        pathMatrix.SetValue(pathTileIndex.x, pathTileIndex.y, true);
      }
    }
  }

  //sets all requiered connections for the node system 
  private void AddCrossingConnections(Path path, Path crossPath)
  {
    foreach (var room in crossPath.connections)
    {
      foreach (var newRoom in path.connections)
      {
        if (Vector3.Distance(room.GetCenterWorld(), newRoom.GetCenterWorld()) > 2 * roomDistance.y) continue;

        room.AddConnection(newRoom);
        newRoom.AddConnection(room);
      }
    }
    crossPath.AddConnections(path.connections);
    path.AddConnections(crossPath.connections);
  }

  //searches for existing path on given position
  private Path GetPathAtPosition(Vector2Int gridPos)
  {
    return paths.Find(i => (i.position.x <= gridPos.x
                        && (i.position + i.size).x >= gridPos.x
                        && i.position.y <= gridPos.y
                        && (i.position + i.size).y >= gridPos.y));
  }

  bool RoomPositionIsValid(Room room)
  {
    //check if room position is within the allowed grid zone
    if (room.position.x < shapeArea.x + 1 || room.position.x + room.size.x > shapeArea.y) return false;
    if (room.position.y < 1 || room.position.y + room.size.y > roomMatrix.size - 1) return false;

    if (!EnoughSpaceForRoomPlacement(room)) return false;

    return true;
  }

  /* checks if space for path is free
  the path should not collide with any room
  validation space is one block wider than the path size itself to avoid contact points */
  bool PathPositionIsValid(Path path, int axis)
  {
    int length = axis == 0 ? path.size.x : path.size.y;
    for (int i = 0; i < length; i++)
    {
      for (int j = -1; j < pathWidth + 1; j++)
      {
        Vector2Int pathTileIndex = path.position + new Vector2Int((axis ^ 1), axis) * i + new Vector2Int(axis, (axis ^ 1)) * j;
        if (pathTileIndex.x < shapeArea.x || pathTileIndex.x >= shapeArea.y || pathTileIndex.y < 0 || pathTileIndex.y >= pathMatrix.size) continue;
        if (roomMatrix.GetValue(pathTileIndex.x, pathTileIndex.y)) return false;

        //decide whether path crossing is allowed depending on the connection degree
        if (pathMatrix.GetValue(pathTileIndex.x, pathTileIndex.y))
        {
          bool crossingAllowed = Random.Range(0, 1f) < connectionDegree;
          if (!crossingAllowed) return false;
        }
      }
    }
    return true;
  }

  /* checks if space for the room is free
  the room should not collide with any room or path
  validation space is one block wider than the room size itself to avoid contact points */
  private bool EnoughSpaceForRoomPlacement(Room room)
  {
    int roomSaveSpace = 1;
    for (int i = room.position.x - roomSaveSpace; i < room.position.x + room.size.x + roomSaveSpace; i++)
    {
      for (int j = room.position.y - roomSaveSpace; j < room.position.y + room.size.y + roomSaveSpace; j++)
      {
        if (i < shapeArea.x || i >= shapeArea.y || j < 0 || j >= roomMatrix.size) continue;
        if (roomMatrix.GetValue(i, j)) return false;
        if (pathMatrix.GetValue(i, j)) return false;
      }
    }
    return true;
  }

  private void StartIterativeImproving()
  {
    for (int attempts = 0; attempts < compressFactor * 10; attempts++)
    {
      //iterate over each room and try to generate additional rooms
      for (int i = 0; i < rooms.Count; i++)
      {
        Room startRoom = rooms[i];
        GenerateRecursivly(startRoom);
      }
    }
  }

  private void GenerateAdditionalConnections()
  {
    List<Room[]> roomTuples = GraphProcessor.GenerateAdditionalConnections(rooms, roomDistance.y, connectionDegree);

    foreach (var tuple in roomTuples)
    {
      TryConnectingRooms(tuple[0], tuple[1]);
    }
  }

  private void TryConnectingRooms(Room leafNode, Room node)
  {
    if (leafNode.connections.Contains(node) || node.connections.Contains(leafNode)) return;

    Path newPath = null;
    //generate path ehich is oriented in y direction
    if (node.GetBottomRight().x >= leafNode.position.x + pathWidth && node.position.x + pathWidth <= leafNode.GetBottomRight().x)
    {
      int minXOffset = Mathf.Max(0, node.position.x - leafNode.position.x);
      int maxXOffset = Mathf.Min(leafNode.size.x - pathWidth, node.GetBottomRight().x - leafNode.position.x - pathWidth);
      int xOffset = UnityEngine.Random.Range(minXOffset, maxXOffset);

      int direction = node.position.y - leafNode.position.y < 0 ? -1 : 1;
      int yOffset = direction == -1 ? -1 : leafNode.size.y;
      int distanceOffset = direction == -1 ? node.size.y : leafNode.size.y;

      Vector2Int offset = new Vector2Int(xOffset, yOffset);
      Vector2Int pathOrigin = leafNode.position + offset;

      newPath = CreatePath(pathOrigin, Mathf.Abs(leafNode.position.y - node.position.y) - distanceOffset, direction, 1);
      if (!PathPositionIsValid(newPath, 1)) newPath = null;
    }
    //generate path which is oriented in x direction
    else if (node.GetBottomRight().y >= leafNode.position.y + pathWidth && node.position.y + pathWidth <= leafNode.GetBottomRight().y)
    {
      int minYOffset = Mathf.Max(0, node.position.y - leafNode.position.y);
      int maxYOffset = Mathf.Min(leafNode.size.y - pathWidth, node.GetBottomRight().y - leafNode.position.y - pathWidth);
      int yOffset = UnityEngine.Random.Range(minYOffset, maxYOffset);

      int direction = node.position.x - leafNode.position.x < 0 ? -1 : 1;
      int xOffset = direction == -1 ? -1 : leafNode.size.x;
      int distanceOffset = direction == -1 ? node.size.x : leafNode.size.x;

      Vector2Int offset = new Vector2Int(xOffset, yOffset);
      Vector2Int pathOrigin = leafNode.position + offset;

      newPath = CreatePath(pathOrigin, Mathf.Abs(leafNode.position.x - node.position.x) - distanceOffset, direction, 0);
      if (!PathPositionIsValid(newPath, 0)) newPath = null;
    }

    //if no connection is possible delete connection
    if (newPath == null)
    {
      leafNode.connections.Remove(node);
      node.connections.Remove(leafNode);
      return;
    }

    //save created path
    newPath.AddConnections(leafNode, node);
    node.AddConnection(leafNode);
    leafNode.AddConnection(node);
    SavePathToBitMatrix(newPath);
    paths.Add(newPath);
  }

  //iterates over filled matrix and places tiles accordingly into the world
  public void PlaceTiles()
  {
    BitMatrix combinedMatrix = roomMatrix + pathMatrix;
    for (int i = 1; i < combinedMatrix.size - 1; i++)
    {
      for (int j = 1; j < combinedMatrix.size - 1; j++)
      {
        if (combinedMatrix.GetValue(i, j))
        {
          Instantiate(groundPrefab, new Vector3(j * tileSize, 0, i * tileSize), Quaternion.identity);
          CheckForWallPlacement(i, j);
        }
      }
    }
  }

  void CheckForWallPlacement(int x, int y)
  {
    if (!roomMatrix.GetValue(x, y - 1))
    {
      PlaceWall(x * tileSize, y * tileSize - tileSize / 2, x, y);
    }
    if (!roomMatrix.GetValue(x - 1, y))
    {
      PlaceWall(x * tileSize - tileSize / 2, y * tileSize, x, y);
    }
    if (!roomMatrix.GetValue(x + 1, y))
    {
      PlaceWall(x * tileSize + tileSize / 2, y * tileSize, x, y);
    }
    if (!roomMatrix.GetValue(x, y + 1))
    {
      PlaceWall(x * tileSize, y * tileSize + tileSize / 2, x, y);
    }
  }

  void PlaceWall(int x, int y, int directionX, int directionY)
  {
    GameObject wall = Instantiate(wallPrefab, new Vector3(y, 0, x), Quaternion.identity);
    wall.transform.LookAt(new Vector3(directionY * tileSize, 0, directionX * tileSize));
  }

  private void GenerateColumns()
  {
    // Texture2D workingCopy = new Texture2D(8, 8);
    // Graphics.CopyTexture(columnBluePrint, workingCopy)
    foreach (Room room in rooms)
    {
      if (room.size.x < columnBluePrint.height || room.size.y < columnBluePrint.height) continue;
      //if (room.size.x % 2 == 1 || room.size.y % 2 == 1) continue;
      if (Random.value > columnProbability) continue;
      //columnBluePrint.Resize(room.size.x, room.size.y);
      for (int i = 0; i < columnBluePrint.width; i++)
      {
        for (int j = 0; j < columnBluePrint.height; j++)
        {
          if (columnBluePrint.GetPixel(i, j) == Color.white) continue;
          else
          {
            int x = (int)Mathf.Round((float)(room.size.x * j) / (columnBluePrint.width) + (j * 0.1f));
            int y = (int)Mathf.Round((float)(room.size.y * i) / (columnBluePrint.height) + (i * 0.1f));
            Vector2Int position = room.GetBottomLeft() + new Vector2Int(x, -y);
            SetDebugBlock(position);
          }
        }
      }
    }
  }

  private void DebugInformation()
  {
    float shapeAreaSize = Mathf.Max(dungeonSize - (shape * dungeonSize), 2 * roomDimensionX.x);
    float ratio = dungeonSize / shapeAreaSize;
    Debug.Log("Space Ratio: 1 : " + ratio);
    Debug.Log("Placed " + currentRoomCount + " rooms.");

    float connectionCount = 0;
    foreach (var room in rooms)
    {
      connectionCount += room.connections.Count;
    }
    Debug.Log("Average Connection Count per room: " + connectionCount / rooms.Count);
  }

  void SetDebugBlock(Vector2Int pos)
  {
    SetDebugBlock(pos.x, pos.y);
  }

  void SetDebugBlock(int x, int y)
  {
    Instantiate(debugCube, new Vector3(y * tileSize, 0, x * tileSize), Quaternion.identity);
  }

  private void OnDrawGizmos()
  {
    DrawDungeonAreaOutline();
    DrawDungeonTree();
  }

  private void DrawDungeonTree()
  {
    Gizmos.color = Color.blue;
    foreach (var room in rooms)
    {
      Gizmos.DrawSphere(room.GetCenterWorld() * tileSize, 3f);
      foreach (var childRoom in room.connections)
      {
        Gizmos.DrawLine(room.GetCenterWorld() * tileSize, childRoom.GetCenterWorld() * tileSize);
      }
    }
  }

  private void DrawDungeonAreaOutline()
  {
    Gizmos.color = Color.red;

    float size = (dungeonSize - 1) * tileSize;
    Vector3 topRight = new Vector3(0, 0, size);
    Vector3 bottomLeft = new Vector3(size, 0, 0);
    Vector3 bottomRight = new Vector3(size, 0, size);

    Gizmos.DrawLine(Vector2.zero, topRight);
    Gizmos.DrawLine(topRight, bottomRight);
    Gizmos.DrawLine(bottomRight, bottomLeft);
    Gizmos.DrawLine(bottomLeft, Vector3.zero);

    Gizmos.color = Color.green;

    float shapeAreaSize = Mathf.Max(dungeonSize - (shape * dungeonSize), 2 * roomDimensionX.x);
    float leftBorder = dungeonSize / 2 - shapeAreaSize / 2;
    float rightBorder = dungeonSize / 2 + shapeAreaSize / 2;

    Gizmos.DrawLine(new Vector3(-10, 0, 4 * leftBorder), new Vector3(4 * dungeonSize + 10, 0, 4 * leftBorder));
    Gizmos.DrawLine(new Vector3(-10, 0, 4 * rightBorder), new Vector3(4 * dungeonSize + 10, 0, 4 * rightBorder));
  }
}
