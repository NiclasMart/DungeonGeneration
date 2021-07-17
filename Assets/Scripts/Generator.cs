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
  [SerializeField] int pathWidth = 2;
  [SerializeField, Range(0, 1)] float compressFactor = 0.1f;
  [SerializeField, Range(0, 1)] float connectionDegree = 0.3f;
  [SerializeField, Range(0, 1)] float shape = 0;

  [Header("Room parameters")]
  [SerializeField] Vector2Int roomDimensionX = new Vector2Int(5, 10);
  [SerializeField] Vector2Int roomDimensionY = new Vector2Int(5, 10);
  [SerializeField] Vector2Int roomDistance = new Vector2Int(0, 10);
  [SerializeField] Texture2D roomBluePrint;

  enum RoomPosition { Center, Edge, Random };
  [Header("Path Parameters")]
  [SerializeField] RoomPosition startRoomPosition;
  [SerializeField] RoomPosition endRoomPosition;
  [SerializeField, Min(0)] int minPathLength;
  [SerializeField, Min(1)] int maxPathLength;
  [SerializeField] bool useFullDungeonSize;

  [Header("Event Room Parameters")]
  [SerializeField] int amount;
  [SerializeField] int minimalDistanceFromStartRoom;

  [Header("Other")]
  [Tooltip("Defines up to which ratio a room is categorized as squared. The value must be larger than 1 ( 1 = square).")]
  [SerializeField] float minColumnRoomSize = 5f;
  [SerializeField] float columnProbability = 0.3f;
  [SerializeField] Texture2D columnBluePrint;

  [Header("Tile Prefabs")]
  [SerializeField] GameObject groundPrefab;
  [SerializeField] GameObject wallPrefab;
  [SerializeField] GameObject debugCube;
  [SerializeField] bool generateColumns = true;

  BitMatrix roomMatrix, pathMatrix;
  Graph roomsGraph;
  List<Path> paths = new List<Path>();
  List<Room> eventRooms = new List<Room>();
  Room startRoom, endRoom;
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

    roomsGraph = new Graph();

    BluePrintRoom testRoom = new BluePrintRoom(Vector2Int.one, roomBluePrint);
  }

  private void Start()
  {
    StartGeneration();
    StartIterativeImproving();
    GenerateAdditionalConnections();
    GeneratePath();
    GenerateSpecialRooms();
    PlaceTiles();

    DebugInformation();

    if (generateColumns) GenerateColumns();
  }

  private void GenerateSpecialRooms()
  {
    eventRooms = GraphProcessor.GetRandomNodesWithDistanceFromOrigin(roomsGraph, startRoom, amount, minimalDistanceFromStartRoom);
  }

  public void StartGeneration()
  {
    Room newRoom = GenerateRoom(Mathf.Min(roomDimensionX.y, shapeArea.y - dungeonSize / 2));
    newRoom.SetPosition(new Vector2Int(roomMatrix.size / 2, roomMatrix.size / 2));

    SetDebugBlock(new Vector2Int(roomMatrix.size / 2, roomMatrix.size / 2));

    SaveRoomToBitMatrix(newRoom);
    roomsGraph.AddNode(newRoom);

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
    //north 11 - 3, west 00 - 0, south 10 - 2, west 01 - 1
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
    Room newRoom;
    if (Random.Range(0, 1f) < 0.5f)
    {
      newRoom = new BluePrintRoom(Vector2Int.one, roomBluePrint);

    }
    else
    {
      int sizeX = Random.Range(roomDimensionX.x, roomDimensionX.y);
      int sizeY = Random.Range(roomDimensionY.x, roomDimensionY.y);
      newRoom = new Room(new Vector2Int(sizeX, sizeY));
    }
    return newRoom;
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
    roomsGraph.AddNode(newRoom);
  }

  void SaveRoomToBitMatrix(Room room)
  {
    for (int i = room.position.x; i < room.position.x + room.size.x; i++)
    {
      for (int j = room.position.y; j < room.position.y + room.size.y; j++)
      {
        if (room is BluePrintRoom)
        {
          if ((room as BluePrintRoom).GetBlueprintPixel(i - room.position.x, j - room.position.y) == Color.black) roomMatrix.SetValue(i, j, true);
        }
        else roomMatrix.SetValue(i, j, true);
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
    for (int i = room.position.x - 1; i < room.position.x + room.size.x + 1; i++)
    {
      for (int j = room.position.y - 1; j < room.position.y + room.size.y + 1; j++)
      {
        if (i < shapeArea.x || i >= shapeArea.y || j < 0 || j >= roomMatrix.size) continue;
        //if (room is BluePrintRoom && (room as BluePrintRoom).GetBlueprintPixel(i - room.position.x + 1, j - room.position.y + 1) != Color.black) continue;
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
      for (int i = 0; i < roomsGraph.Count; i++)
      {
        Room startRoom = roomsGraph[i];
        GenerateRecursivly(startRoom);
      }
    }
  }

  private void GenerateAdditionalConnections()
  {
    List<Room[]> roomTuples = GraphProcessor.GenerateAdditionalConnections(roomsGraph, roomDistance.y, connectionDegree);

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

  Room[] outerRooms;
  List<Room> edgeRooms;
  private void GeneratePath()
  {
    outerRooms = GraphProcessor.GetEdgeRooms(roomsGraph);
    if (useFullDungeonSize)
    {
      if (Vector2Int.Distance(outerRooms[0].GetCenter(), outerRooms[2].GetCenter()) > Vector2Int.Distance(outerRooms[1].GetCenter(), outerRooms[3].GetCenter()))
      {
        startRoom = outerRooms[0];
        endRoom = outerRooms[2];
      }
      else
      {
        startRoom = outerRooms[1];
        endRoom = outerRooms[3];
      }
    }
    else
    {
      //if both rooms should be within the center, no further calculation needed
      if (startRoomPosition == RoomPosition.Center && endRoomPosition == RoomPosition.Center)
      {
        startRoom = endRoom = roomsGraph[0];
        return;
      }

      //only calculate convex hull if needed
      if (startRoomPosition == RoomPosition.Edge || endRoomPosition == RoomPosition.Edge)
        edgeRooms = GraphProcessor.CalculateConvexHull(roomsGraph, outerRooms[0]);

      //calculation is started from the most specific to generic room
      //center > edge > random
      Room calculationOriginRoom = GetRoomForPathGeneration((RoomPosition)Mathf.Min((int)startRoomPosition, (int)endRoomPosition));
      bool endRoomLiesOnEdge = Mathf.Max((int)startRoomPosition, (int)endRoomPosition) == 1;
      List<Room> path = GraphProcessor.GeneratePath(roomsGraph, calculationOriginRoom, minPathLength, maxPathLength, endRoomLiesOnEdge);

      //depending on the initial room types choose start and end room from genereded path
      (startRoom, endRoom) = (int)startRoomPosition <= (int)endRoomPosition ? (path[0], path[path.Count - 1]) : (path[path.Count - 1], path[0]);

    }
  }

  private Room GetRoomForPathGeneration(RoomPosition type)
  {
    switch (type)
    {
      case RoomPosition.Edge:
        return edgeRooms[Random.Range(0, edgeRooms.Count)];
      case RoomPosition.Center:
        return roomsGraph[0];
      default:
        return roomsGraph[Random.Range(0, roomsGraph.Count)];
    }
  }

  private void GenerateColumns()
  {
    // Texture2D workingCopy = new Texture2D(8, 8);
    // Graphics.CopyTexture(columnBluePrint, workingCopy)
    foreach (Room room in roomsGraph.nodes)
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



  //---------------------------------------------------------------------------------------------------
  // DEBUG FUNCTIONALITY
  //---------------------------------------------------------------------------------------------------

  List<Room> debugPath;
  private void DebugInformation()
  {
    float shapeAreaSize = Mathf.Max(dungeonSize - (shape * dungeonSize), 2 * roomDimensionX.x);
    float ratio = dungeonSize / shapeAreaSize;
    Debug.Log("Space Ratio: 1 : " + ratio);
    Debug.Log("Placed " + currentRoomCount + " rooms.");

    float connectionCount = 0;
    foreach (var room in roomsGraph.nodes)
    {
      connectionCount += room.connections.Count;
    }
    Debug.Log("Average Connection Count per room: " + connectionCount / roomsGraph.Count);

    //calculate debug path
    debugPath = GraphProcessor.GetShortestPathBetweenNodes(roomsGraph, startRoom, endRoom);

    Debug.Log("Path Length: " + (debugPath.Count - 1));
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
    DrawShapeArea();
    DrawPath();
    DrawEventRooms();
  }

  private void DrawEventRooms()
  {
    Gizmos.color = Color.cyan;
    foreach (var room in eventRooms)
    {
      Gizmos.DrawSphere(room.GetCenterWorld() * tileSize, 5f);
    }
  }

  private void DrawDungeonTree()
  {
    if (roomsGraph == null) return;

    Gizmos.color = Color.blue;
    foreach (var room in roomsGraph.nodes)
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
  }

  private void DrawShapeArea()
  {
    Gizmos.color = Color.green;

    float shapeAreaSize = Mathf.Max(dungeonSize - (shape * dungeonSize), 2 * roomDimensionX.x);
    float leftBorder = dungeonSize / 2 - shapeAreaSize / 2;
    float rightBorder = dungeonSize / 2 + shapeAreaSize / 2;

    Gizmos.DrawLine(new Vector3(-10, 0, 4 * leftBorder), new Vector3(4 * dungeonSize + 10, 0, 4 * leftBorder));
    Gizmos.DrawLine(new Vector3(-10, 0, 4 * rightBorder), new Vector3(4 * dungeonSize + 10, 0, 4 * rightBorder));
  }

  private void DrawPath()
  {
    if (debugPath == null) return;
    Gizmos.color = Color.red;
    for (int i = 1; i < debugPath.Count; i++)
    {
      Gizmos.DrawLine(debugPath[i - 1].GetCenterWorld() * tileSize, debugPath[i].GetCenterWorld() * tileSize);
    }

    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(startRoom.GetCenterWorld() * tileSize, 8f);
    Gizmos.color = Color.magenta;
    Gizmos.DrawSphere(endRoom.GetCenterWorld() * tileSize, 8f);
  }
}
