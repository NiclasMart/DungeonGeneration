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

  bool finishedTestRun, finishedEvaluation;

  private void Update()
  {
    if (currentRun == 0) StartCoroutine(RunTest());
    if (finishedTestRun && !finishedEvaluation)
    {
      Generator gen = GetComponent<Generator>();
      gen.CalculateDebugInformation();
      double averageRuntime = runTime / runAmount;
      Debug.Log("Average Runtime: " + averageRuntime / 1000 + "s (" + averageRuntime + "ms)");
      finishedEvaluation = true;
    }
  }

  int currentRun = 0;
  double startTime, endTime, runTime = 0;
  IEnumerator RunTest()
  {
    while (currentRun < runAmount)
    {

      Generator gen = GetComponent<Generator>();
      startTime = System.Environment.TickCount;
      gen.StartGeneration();
      endTime = System.Environment.TickCount;
      runTime += endTime - startTime;
      currentRun++;
      if (currentRun != runAmount) gen.Reset();
      Debug.Log(endTime - startTime);
      yield return new WaitForEndOfFrame();
    }

    finishedTestRun = true;
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
