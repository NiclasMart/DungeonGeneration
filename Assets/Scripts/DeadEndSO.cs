using UnityEngine;

[CreateAssetMenu(fileName = "DeadEndSO", menuName = "DungeonGeneration/DeadEndSO", order = 0)]
public class DeadEndSO : ScriptableObject
{
  public string deadEndArray;

  public void AddCount(int number)
  {
    deadEndArray += (number.ToString() + ", ");
  }
}