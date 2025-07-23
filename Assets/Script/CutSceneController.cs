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

    public void AddVaringBeginingCoroutines()
    {
        //Timer.theVaryBeginingCoroutineQueueManager.AddCoroutine(ExecuteCoroutines(3));
    }

    public IEnumerator ExecuteCoroutines(int cutSceneDataIndex)
    {
        List<Task> cutSceneList = cutSceneDataList[cutSceneDataIndex].cutSceneList;
        Timer.Pause();
        foreach (var task in cutSceneList)
        {
            // 根据配置名称选择要执行的协程
            switch (task.coroutineType)
            {
                case CoroutineType.NPCMoves:
                    yield return StartCoroutine(NPCMoves(task.obj.GetComponent<NPC>(),task.destination));
                    break;
                case CoroutineType.NPCDialogue:
                    yield return StartCoroutine(NPCDialogue(task.obj.GetComponent<NPC>(),task.dialogueData));
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
                    yield return StartCoroutine(Blackout(task.obj.GetComponent<Blackout>()));
                    break;
                case CoroutineType.Pusse:
                    Pusse();
                    break;
                case CoroutineType.Resume:
                    Resume();
                    break;
                // 可以添加更多协程类型...
                default:
                    yield return null;
                    break;
            }
        }
        Timer.Resume();

    }
    IEnumerator NPCMoves(NPC actor,GameObject destination)
    {
        yield return StartCoroutine(actor.MoveTo(destination));
    }

    IEnumerator  NPCDialogue(NPC actor,DialogueData dialogue)
    {
        actor.dialogueController.Initialize(dialogue);
        yield return StartCoroutine(actor.dialogueController.DisplayDialogue());
    }
    IEnumerator  InterateWith(Interactable interactable)
    {
        yield return StartCoroutine(interactable.Interact(Hero.Instance));
    }
    IEnumerator  Blackout(Blackout blackout)
    {
        yield return StartCoroutine(blackout.FadeInOrOutCoroutine());
    }

    void  SetObjActiveFalse(GameObject obj)
    {
        obj.SetActive(false);
    }
    void  SetObjActiveTrue(GameObject obj)
    {
        obj.SetActive(true);
    }
    public void Pusse()
    {
        Timer.Pause();
    }
    public void Resume()
    {
        Timer.Resume();
    }
    // Start is called before the first frame update
    void Awake()
    {

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
