using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NPC : Character
{
    [SerializeField] private AStar astar;
    SomeThing someThingsWorkingOn;
    public List<SomeThing> ThingsICanDo;
    public DialogueController dialogueController;
    NPC interlocutor;
    bool isMeeting;
    DialogueData currentTopic;
    [Serializable]
    public class SomeThing {
        public SomeThingType type;
        public List<GameObject> obgList;
        public int multiPersonDialoguesIndex;
    }
    [Serializable]
    public enum SomeThingType{
        none = 0,
        use,
        meeting,
    }
    public override IEnumerator Interact(Character interactor)
    {
        if(interactor is Hero) //对象是主角
        {
            Timer.Pause();
            Hero.Instance.canActive = false;
            int dialogueIndex = 0; //需要控制的时候找地方再赋值
            if(isMeeting && dialogueController.multiPersonDialogues.Count > 0) //多人对话
            {
                dialogueController.Initialize(dialogueController.multiPersonDialogues[someThingsWorkingOn.multiPersonDialoguesIndex],someThingsWorkingOn.obgList);
                yield return StartCoroutine(dialogueController.DisplayDialogue());
            }
            else if(dialogueController?.dialogues.Count > 0)
            {
                dialogueController.Initialize(dialogueController.dialogues[dialogueIndex]);
                yield return StartCoroutine(dialogueController.DisplayDialogue());
            }
            else
                Debug.LogWarning("No dialogue has been set");
            yield return null;
            Timer.Resume();
        }
        else //interactor is NPC
        {
            StartCoroutine(Reaction(interactor.gameObject));
            interlocutor = interactor.GetComponent<NPC>();
            yield return ShowPopUp("Result",interactTime);

            interlocutor = null;
        }
    }
 
    IEnumerator MoveToObj(GameObject targetObj)
    {
        Interactable target = targetObj.GetComponent<Interactable>();
        //Debug.Log("MoveToObj: "+target);
        List<List<AStar.Node>> listPath = new();
        target.interactPoint ??= new Vector3[]{Vector3.zero};
        if(target.interactPoint.Length == 0)
            target.interactPoint = new Vector3[]{Vector3.zero};
        foreach (Vector3 point in target.interactPoint)
        {
            listPath.Add(astar.FindPath(transform.position, target.transform.position + point));           
        }
        List<AStar.Node> path = new();
        int lestestCount = 999999;
        foreach (List<AStar.Node> pathForChoose in listPath)
        {
            if(pathForChoose!=null)
                if(pathForChoose.Count<=lestestCount)
                {
                    lestestCount = pathForChoose.Count;
                    path = pathForChoose;
                }
        }
        if (path != null)
        {
            //Debug.Log(path.Count);
            foreach (AStar.Node node in path)
            {
                Debug.DrawLine(node.worldPosition, node.parent.worldPosition, Color.red, 99f);
                Vector3 direction = (node.worldPosition - transform.position).normalized;
                while( transform.position != node.worldPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position,node.worldPosition,Time.deltaTime*fSpeed);
                    AnimateMove(direction);
                    do 
                        yield return null;
                    while(Timer.hasPaused); //timer暂停时禁止移动 
                }    
            }
            AnimateStopMove();
        }
        else
            Debug.Log("no path!"); 
    }

    void AnimateMove(Vector3 direction)
    {
        if(animator != null)
        {
            animator.SetBool("IsMoving",true);
            animator.SetFloat("X",direction.x);
            animator.SetFloat("Y",direction.y);
        }
    }
    void AnimateStopMove()
    {
        if(animator != null)
        {
            animator.SetBool("IsMoving",false);
            popUp.animator.SetInteger("Result",0); //关掉表情气泡
        }
    }

    public IEnumerator Reaction(GameObject interrupter)
    {
        StopAllCoroutines();
        AnimateStopMove();
        FaceTheTarget(interrupter);
        yield return ShowPopUp("Result",interactTime);
        if (interrupter == someThingsWorkingOn.obgList[0]) //the person I was supposed to find
            StartCoroutine(LetsDoSomeThing());
        else
            StartCoroutine(LetsDoSomeThing(someThingsWorkingOn));
    }

    public IEnumerator MoveTo(GameObject gameObj)
    {
        StopAllCoroutines();
        AnimateStopMove();
        List<AStar.Node> path = astar.FindPath(transform.position, gameObj.transform.position);
        if (path != null)
        {
            foreach (AStar.Node node in path)
            {
                Debug.DrawLine(node.worldPosition, node.parent.worldPosition, Color.red, 99f);
                Vector3 direction = (node.worldPosition - transform.position).normalized;
                while( transform.position != node.worldPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position,node.worldPosition,Time.deltaTime*fSpeed);
                    AnimateMove(direction);
                    yield return null;
                }    
            }
            AnimateStopMove();
        }
        FaceTheTarget(gameObj);
    }

    IEnumerator LetsDoSomeThing(SomeThing someThing = null)
    {
        yield return new WaitForSeconds(1);
        if(ThingsICanDo.Count == 0)
            yield break;
        if(someThing == null)
            someThing = ThingsICanDo[UnityEngine.Random.Range(0,ThingsICanDo.Count)];
        someThingsWorkingOn = someThing;
        switch(someThing.type)
        {
            case SomeThingType.meeting:
                yield return StartCoroutine(TakeMeeting(dialogueController.multiPersonDialogues[someThing.multiPersonDialoguesIndex],someThing.obgList));//参数决定了npc想聊什么，以后有需要再实现传参方式
                break;
            case SomeThingType.use:
                yield return StartCoroutine(UseFacility(someThing.obgList[0]));
                break;                                  
        } 
        StartCoroutine(LetsDoSomeThing());
    }

    void NotifyTheTarget(NPC target)
    {
        target.StopAllCoroutines();
        target.AnimateStopMove();
    }
    public IEnumerator UseFacility(GameObject facility)
    {
        Interactable interactable = facility.GetComponent<Interactable>();
        if(interactable.occupiedBy == this || interactable.occupiedBy == null) //未被占用
        {    
            yield return StartCoroutine(MoveToObj(facility));
            FaceTheTarget(facility);
            interactable.occupiedBy = this;
            StartCoroutine(ShowPopUp("Result",interactable.interactTime));
            yield return StartCoroutine(interactable.Interact(this));
            interactable.occupiedBy = null;
        }
    }
    public IEnumerator TakeMeeting(DialogueData topic,List<GameObject> interlocutors)
    {
        if(interlocutors.Count == 1) //与另一个你npc对话
        {
            NPC npc = interlocutors[0].GetComponent<NPC>();
            if(npc.occupiedBy == this || npc.occupiedBy == null) //npc未被占用
            {
                NotifyTheTarget(npc); //叫住对面NPC
                npc.occupiedBy = this;
                yield return StartCoroutine(MoveToObj(npc.gameObject));
                FaceTheTarget(npc.gameObject);
                isMeeting = true;
                StartCoroutine(npc.Interact(this));
                yield return StartCoroutine(ShowPopUp("Result",npc.interactTime));
                currentTopic = topic;
                isMeeting = false;
                npc.occupiedBy = null;
            }
        }
        else    //与多个npc对话
        {

        }
    }

    void OnEnable()
    {
        StartCoroutine(LetsDoSomeThing());
    }
    // Start is called before the first frame update
    new void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
