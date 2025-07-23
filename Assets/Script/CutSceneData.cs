using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CutSceneData", menuName = "CutScene/CutScene Data")]
public class CutSceneData : ScriptableObject{
    public List<Task> cutSceneList;
}
[Serializable]
public class Task{
    public string objName;
    public string destinationObjName;
    [HideInInspector]
    public GameObject obj;
    public CoroutineType coroutineType;
    public List<string> varList;
    public DialogueData dialogueData;
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
