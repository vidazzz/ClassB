using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneController : MonoBehaviour
{
    private static CutSceneController _instance; //单例
    public static CutSceneController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CutSceneController>();
            }
            return _instance;
        }
    }
    public List<CutSceneData> cutSceneDataList;
    public List<CutSceneGraph> cutSceneGraphList;

    void RegisterEvents()
    {
        foreach (var cutSceneData in cutSceneDataList)
        {
            switch (cutSceneData.invokTime)
            {
                case InvokTime.TheVaryBegining:
                    EventManager.Instance.onTheVaryBegining += () => Timer.Instance.StartCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.theVaryBeginingQueue:
                    EventManager.Instance.onTheVaryBegining += () => CoroutineQueueManager.theVaryBeginingCoroutineQueue.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.DayBeginQueue:
                    EventManager.Instance.onDayBegin += () => CoroutineQueueManager.dayBeginCoroutineQueue.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.DayBegin2Queue:
                    EventManager.Instance.onDayBegin2 += () => CoroutineQueueManager.dayBeginCoroutineQueue2.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.onDayN_Begin:
                    EventManager.Instance.onDayN_Begin += (dayNumber) => CoroutineQueueManager.dayBeginCoroutineQueue.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.HourEndQueue:
                    EventManager.Instance.onHourEnd += () => CoroutineQueueManager.onHourEndCoroutineQueue.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.OffWorkQueue:
                    EventManager.Instance.onOffWork += () => CoroutineQueueManager.offWorkCoroutineQueue.AddCoroutine(ExecuteCoroutines(cutSceneData)); 
                    break;
                case InvokTime.OffWork2Queue:
                    EventManager.Instance.onOffWork2 += () => CoroutineQueueManager.offWorkCoroutineQueue2.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.DayEndQueue:
                    EventManager.Instance.onDayEnd += () => CoroutineQueueManager.dayEndCoroutineQueue.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.DayEnd2Queue:
                    EventManager.Instance.onDayEnd2 += () => CoroutineQueueManager.dayEndCoroutineQueue.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                case InvokTime.FirstQuitTypingGameQueue:
                    EventManager.Instance.onFirstQuitTypingGame += () => CoroutineQueueManager.firstQuitTypingGameCoroutineQueue.AddCoroutine(ExecuteCoroutines(cutSceneData));
                    break;
                // 可以添加更多触发时机的事件注册...
                default:
                    break;
            }
        }
    }
    public IEnumerator ExecuteCoroutines(CutSceneData cutSceneData)
    {
        List<Task> cutSceneList = cutSceneData.cutSceneList;
        Timer.Pause();
        foreach (var task in cutSceneList)
        {
            // 根据配置名称选择要执行的协程
            switch (task.coroutineType)
            {
                case CoroutineType.NPCMoves:
                    yield return StartCoroutine(NPCMoves(task.obj.GetComponent<NPC>(), task.destination));
                    break;
                case CoroutineType.NPCDialogue:
                    yield return StartCoroutine(Graph_NPC_Dialogue(task.obj.GetComponent<NPC>(), task.dialogueGraph));
                    break;
                case CoroutineType.interactWith:
                    yield return StartCoroutine(InterateWith(task.obj.GetComponent<Interactable>()));
                    break;
                case CoroutineType.SetActiveTrue:
                    SetObjActiveTrue(task.obj);
                    break;
                case CoroutineType.SetActiveFalse:
                    SetObjActiveFalse(task.obj);
                    break;
                case CoroutineType.Blackout:
                    yield return StartCoroutine(BlackOutCoroutine(task.varList.ToArray()));
                    break;
                case CoroutineType.ResetDialuoge:
                    //ResetDialuoge(task.dialogueGraph);
                    break;
                case CoroutineType.Pusse:
                    Timer.Pause();
                    break;
                case CoroutineType.Resume:
                    Timer.Resume();
                    break;
                // 可以添加更多协程类型...
                default:
                    yield return null;
                    break;
            }
        }
        Timer.Resume();
    }
    public IEnumerator GraghExecuteCoroutines(int cutSceneGraphIndex)
    {
        CutSceneGraph cutSceneGraph = cutSceneGraphList[cutSceneGraphIndex];
        Timer.Pause();
        CutSceneNode node = cutSceneGraph.startNode;
        // 根据配置名称选择要执行的协程
        switch (node.coroutineType)
        {
            case CoroutineType.NPCMoves:
                yield return StartCoroutine(NPCMoves(node.obj.GetComponent<NPC>(), node.destination));
                break;
            case CoroutineType.NPCDialogue:
                yield return StartCoroutine(Graph_NPC_Dialogue(node.obj.GetComponent<NPC>(), node.dialogueGraph));
                break;
            case CoroutineType.interactWith:
                yield return StartCoroutine(InterateWith(node.obj.GetComponent<Interactable>()));
                break;
            case CoroutineType.SetActiveTrue:
                SetObjActiveTrue(node.obj);
                break;
            case CoroutineType.SetActiveFalse:
                SetObjActiveFalse(node.obj);
                break;
            case CoroutineType.Blackout:
                yield return StartCoroutine(BlackOutCoroutine(node.args.ToArray()));
                break;
            case CoroutineType.Pusse:
                Timer.Pause();
                break;
            case CoroutineType.Resume:
                Timer.Resume();
                break;
            // 可以添加更多协程类型...
            default:
                yield return null;
                break;
        }
        Timer.Resume();
    }
    IEnumerator NPCMoves(NPC actor,GameObject destination)
    {
        yield return StartCoroutine(actor.MoveTo(destination));
    }

    //已被Graph_NPC_Dialogue替代
    //NPC发起对话
    /*
    IEnumerator  NPCDialogue(NPC actor,DialogueData dialogue)
    {
        actor.dialogueController.Initialize(dialogue);
        yield return StartCoroutine(actor.dialogueController.DisplayDialogue());
    }
    */

    IEnumerator  Graph_NPC_Dialogue(NPC actor,DialogueGraph dialogueGraph)
    {
        yield return StartCoroutine(actor.dialogueController.GraphDisplayDialogue(dialogueGraph));
    }
    IEnumerator  InterateWith(Interactable interactable)
    {
        yield return StartCoroutine(interactable.Interact(Hero.Instance));
    }
    IEnumerator  BlackOutCoroutine(string[] sentances)
    {
        yield return StartCoroutine(Blackout.Instance.FadeInOrOutCoroutine());
        yield return StartCoroutine(Blackout.Instance.DisplayText(sentances));
    }
    void  SetObjActiveFalse(GameObject obj)
    {
        obj.SetActive(false);
    }

    void  ResetDialuoge(DialogueGraph dialogueGraph)
    {
        dialogueGraph.Reset();
    }
    void  SetObjActiveTrue(GameObject obj)
    {
        obj.SetActive(true);
    }

    // Start is called before the first frame update
    void Awake()
    {
        RegisterEvents();
    }
    void Start()
    {
        foreach(var cutSceneData in cutSceneDataList) //在这里处理cutSceneDataList中的gameobject引用
        {
            Debug.Log(cutSceneData);
            foreach(var cutScene in cutSceneData.cutSceneList)
            {
                cutScene.obj = GameObject.Find(cutScene.objName);
                cutScene.destination = GameObject.Find(cutScene.destinationObjName);
            }
        }
    }

    void OnDestroy()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
