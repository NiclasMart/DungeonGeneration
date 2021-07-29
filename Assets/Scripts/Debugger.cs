using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{
  [SerializeField] int runAmount = 1;

  [Header("Debug Display Settings")]
  [SerializeField] bool showDungeonArea;
  [SerializeField] bool showDungeonTree;
  [SerializeField] bool showShapeArea;
  [SerializeField] bool showPath;
  [SerializeField] bool showEventRooms;

  bool finishedEvaluation;

  private void Update()
  {
    if (finishedEvaluation) return;
    double startTime, endTime, runTime = 0;
    int count = 0;
    Generator gen = GetComponent<Generator>();
    do
    {
      startTime = System.Environment.TickCount;
      gen.StartGeneration();
      endTime = System.Environment.TickCount;
      runTime += endTime - startTime;
      count++;
    } while (count < runAmount);

    gen.CalculateDebugInformation();
    double averageRuntime = runTime / count;
    Debug.Log("Average Runtime: " + averageRuntime / 1000 + "s (" + averageRuntime + "ms)");
    finishedEvaluation = true;
  }

  private void OnDrawGizmos()
  {
    Generator gen = GetComponent<Generator>();
    if (gen == null) return;
    if (showDungeonArea) gen.DrawDungeonAreaOutline();
    if (showDungeonTree) gen.DrawDungeonTree();
    if (showShapeArea) gen.DrawShapeArea();
    if (showPath) gen.DrawPath();
    if (showEventRooms) gen.DrawEventRooms();
  }


}
