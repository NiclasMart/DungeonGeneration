using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "newTileSet", menuName = "New Tile Set")]
public class TileSet : ScriptableObject
{
  [SerializeField] List<Tile> floorTiles = new List<Tile>();
  [SerializeField] List<Tile> wallTiles = new List<Tile>();
  [SerializeField] List<Tile> ceilingTiles = new List<Tile>();
  [SerializeField] List<Tile> pathTiles = new List<Tile>();

  List<GameObject> floorTile, wallTile, ceilingTile, pathTile;

  public void Initialize()
  {
    floorTile = BuildTileTable(floorTiles);
    wallTile = BuildTileTable(wallTiles);
    ceilingTile = BuildTileTable(ceilingTiles);
    pathTile = BuildTileTable(pathTiles);
  }

  List<GameObject> BuildTileTable(List<Tile> list)
  {
    List<GameObject> tmpList = new List<GameObject>();

    for (int i = 0; i < list.Count; i++)
    {
      for (int j = 0; j < list[i].frequency; j++)
      {
        tmpList.Add(list[i].tile);
      }
    }

    return tmpList;
  }

  public GameObject GetFloorTile()
  {
    int index = Random.Range(0, floorTile.Count);
    return floorTile[index];
  }

  public GameObject GetWallTile()
  {
    int index = Random.Range(0, wallTile.Count);
    return wallTile[index];
  }

  public GameObject GetCeilingTile()
  {
    int index = Random.Range(0, ceilingTile.Count);
    return ceilingTile[index];
  }

  public GameObject GetPathTiles()
  {
    int index = Random.Range(0, pathTile.Count);
    return pathTile[index];
  }
}

[System.Serializable]
class Tile
{
  public GameObject tile;
  [Min(1)] public int frequency = 1;
}
