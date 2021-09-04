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
  [SerializeField] DeadEndSO deadEndSO;

  bool finishedTestRun, finishedEvaluation;
  List<double> timeList = new List<double>();
  List<int> roomList = new List<int>();

  private void Update()
  {
    if (currentRun == 0) StartCoroutine(RunTest());
    if (finishedTestRun && !finishedEvaluation)
    {
      Generator gen = GetComponent<Generator>();
      gen.CalculateDebugInformation();
      //average Runtime
      double averageRuntime = runTime / runAmount;
      Debug.Log("Average Runtime: " + averageRuntime / 1000 + "s (" + averageRuntime + "ms)");
      //standartdeviation
      double variance = 0;
      double varianceSum = 0;
      foreach (var elem in timeList)
      {
        varianceSum += ((elem - averageRuntime) * (elem - averageRuntime));
      }
      variance = varianceSum / timeList.Count;
      double standardDeviation = Mathf.Sqrt((float)variance);
      Debug.Log("Standard Deviation: " + standardDeviation + "ms ");

      //room count 
      double averageRoomCount = roomCount / runAmount;
      Debug.Log("Average Room Count: " + averageRoomCount);
      //standartdeviation
      variance = 0;
      varianceSum = 0;
      foreach (var elem in roomList)
      {
        varianceSum += ((elem - averageRoomCount) * (elem - averageRoomCount));
      }
      variance = varianceSum / roomList.Count;
      standardDeviation = Mathf.Sqrt((float)variance);
      Debug.Log("Standard Deviation: " + standardDeviation + "rooms ");
      finishedEvaluation = true;
    }
  }

  int currentRun = 0;
  double startTime, endTime, runTime = 0, roomCount = 0;
  int deadEndNumber = 0;
  IEnumerator RunTest()
  {
    while (currentRun < runAmount)
    {
      Generator gen = GetComponent<Generator>();
      startTime = System.Environment.TickCount;
      gen.StartGeneration();
      endTime = System.Environment.TickCount;
      runTime += endTime - startTime;
      //evaluation
      timeList.Add(endTime - startTime);
      roomCount += gen.GetRoomCount();
      roomList.Add(gen.GetRoomCount());

      currentRun++;
      // //deadend counter
      // deadEndNumber += gen.CountDeadEnds();
      // if (currentRun % 10 == 0)
      // {
      //   deadEndSO.AddCount(deadEndNumber / 10);
      //   deadEndNumber = 0;
      //   gen.IncreaseConnectionDegree();
      // }

      
      if (currentRun != runAmount) gen.Reset();
      Debug.Log(currentRun);
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
