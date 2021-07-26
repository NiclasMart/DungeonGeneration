using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "newTileSet", menuName = "New Tile Set")]
public class TileSet : ScriptableObject
{
  [SerializeField] List<Tile> floorTiles = new List<Tile>();
  [SerializeField] List<Tile> wallTiles = new List<Tile>();
  [SerializeField] List<Tile> ceilingTiles = new List<Tile>();

  int floorTotalFrequency, wallTotalFrequency, ceilingTotalFrequency;

  public void Initialize()
  {
    floorTotalFrequency = wallTotalFrequency = ceilingTotalFrequency = 0;
    floorTotalFrequency = InitializeTileFrequencies(floorTiles);
    wallTotalFrequency = InitializeTileFrequencies(wallTiles);
    ceilingTotalFrequency = InitializeTileFrequencies(ceilingTiles);
  }

  int InitializeTileFrequencies(List<Tile> list)
  {
    int frequencySum = 0;
    foreach (var tile in list)
    {
      frequencySum += tile.frequency;
    }
    return frequencySum;
  }

  public GameObject GetFloorTile()
  {
    int rng = Random.Range(1, floorTotalFrequency + 1);
    int frequencySum = 0;
    foreach (var tile in floorTiles)
    {
      frequencySum += tile.frequency;
      if (frequencySum >= rng) return tile.tile;
    }
    return null;
  }

  public GameObject GetWallTile()
  {
    int rng = Random.Range(1, wallTotalFrequency + 1);
    int frequencySum = 0;
    foreach (var tile in wallTiles)
    {
      frequencySum += tile.frequency;
      if (frequencySum >= rng) return tile.tile;
    }
    return null;
  }

  public GameObject GetCeilingTile()
  {
    int rng = Random.Range(1, ceilingTotalFrequency + 1);
    int frequencySum = 0;
    foreach (var tile in ceilingTiles)
    {
      frequencySum += tile.frequency;
      if (frequencySum >= rng) return tile.tile;
    }
    return null;
  }
}

[System.Serializable]
class Tile
{
  public GameObject tile;
  [Min(1)] public int frequency = 1;
}
