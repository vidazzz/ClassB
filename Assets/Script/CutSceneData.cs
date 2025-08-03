using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CutSceneData", menuName = "CutScene/CutScene Data")]
public class CutSceneData : ScriptableObject
{
    public List<Task> cutSceneList;
    public InvokTime invokTime; //触发时机
}
[Serializable]
public class Task{
    public string objName;
    public string destinationObjName;
    [HideInInspector]
    public GameObject obj;
    public CoroutineType coroutineType;
    public List<string> varList;
    public DialogueGraph dialogueGraph;
    [HideInInspector]
    public GameObject destination;
}

public enum CoroutineType
{
    none,
    NPCMoves,
    NPCDialogue,
    interactWith,
    SetActiveTrue,
    SetActiveFalse,
    Blackout,
    ResetDialuoge,
    Pusse,
    Resume,
}

public enum InvokTime
{
    TheVaryBegining,
    theVaryBeginingQueue,
    NextFrame,
    DayBeginQueue,
    DayBegin2Queue,
    onDayN_Begin,
    HourEndQueue,
    OffWorkQueue,
    OffWork2Queue,
    DayEndQueue,
    DayEnd2Queue,
    FirstQuitTypingGameQueue,
}
