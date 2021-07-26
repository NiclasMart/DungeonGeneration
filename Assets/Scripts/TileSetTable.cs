using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "newTileSetTable", menuName = "New Tile Set Table")]
public class TileSetTable : ScriptableObject
{
  [SerializeField] List<Set> roomSets = new List<Set>();
  [SerializeField] List<Set> pathSets = new List<Set>();

  int roomTotalFrequency, pathTotalFrequency;

  public void Initialize()
  {
    roomTotalFrequency = pathTotalFrequency = 0;
    foreach (var set in roomSets)
    {
      set.tileSet.Initialize();
      roomTotalFrequency += set.frequency;
    }
    foreach (var set in pathSets)
    {
      set.tileSet.Initialize();
      pathTotalFrequency += set.frequency;
    }
  }

  public TileSet GetRoomSet()
  {
    int rng = Random.Range(1, roomTotalFrequency + 1);
    int frequencySum = 0;
    foreach (var set in roomSets)
    {
      frequencySum += set.frequency;
      if (frequencySum >= rng) return set.tileSet;
    }
    return null;
  }

  public TileSet GetPathSet()
  {
    int rng = Random.Range(1, pathTotalFrequency + 1);
    int frequencySum = 0;
    foreach (var set in pathSets)
    {
      frequencySum += set.frequency;
      if (frequencySum >= rng) return set.tileSet;
    }
    return null;
  }
}

[System.Serializable]
public class Set
{
  public TileSet tileSet;
  [Min(1)] public int frequency;
}
