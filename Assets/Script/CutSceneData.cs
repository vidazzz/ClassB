using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CutSceneData", menuName = "CutScene/CutScene Data")]
public class CutSceneData : ScriptableObject
{
    public List<Task> cutSceneList;
    public InvokTime invokeTime; //触发时机
    public int invokeDay = -1; //第几天触发
}
[Serializable]
public class Task{
    public string objName;
    public string destinationObjName;
    [HideInInspector]
    public GameObject obj;
    public CoroutineType coroutineType;
    public List<string> agrs;
    public Sprite[] blackOutSprites; // 用于黑幕的图片
    public List<string[]> blackOutLines; // 用于NPC对话的变量列表
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
    HourEndQueue,
    OffWorkQueue,
    OffWork2Queue,
    DayEndQueue,
    DayEnd2Queue,
    FirstQuitTypingGameQueue,
}
