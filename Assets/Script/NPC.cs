using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class NPC : Character
{
    [SerializeField] private AStar astar;
    public List<List<Action>> Schedule;//
    NPC interlocutor;
    public bool isMeeting;

    public override IEnumerator Interact(Character interactor)
    {
        if (interactor is Hero) //对象是主角
        {
            Timer.Pause();
            StopAllCoroutines();
            //Hero.Instance.canActive = false;
            if (isMeeting) //多人对话
            {
                //yield return StartCoroutine(dialogueController.GraphDisplayDialogue(dialogueController.multiPersonDialogueGraphs[scheduleItem.multiPersonDialoguesIndex], scheduleItem.targetList));
            }
            else if (dialogueController.dialogueGraph != null) //单人对话
            {
                yield return StartCoroutine(dialogueController.GraphDisplayDialogue(dialogueController.dialogueGraph));
            }
            else
                Debug.LogWarning("No dialogue has been set");
            yield return null;
            StartCoroutine(ProsessSchedule(action));//继续执行日程
            Timer.Resume();
        }
        else //interactor is NPC
        {
            StartCoroutine(Reaction(interactor.gameObject));
            interlocutor = interactor.GetComponent<NPC>();
            yield return ShowPopUp("Result", 2f);

            interlocutor = null;
        }
    }
 
    IEnumerator MoveToObj(GameObject targetObj)
    {
        Interactable target = targetObj.GetComponent<Interactable>();
        //Debug.Log("MoveToObj: "+target);
        List<List<AStar.Node>> listPath = new();
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
            for (int i = 0; i < path.Count - target.interactDisdence; i++)
            {
                AStar.Node node = path[i];
                Debug.DrawLine(node.worldPosition, node.parent.worldPosition, Color.red, 99f);
                Vector3 direction = (node.worldPosition - transform.position).normalized;
                while (transform.position != node.worldPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position, node.worldPosition, Time.deltaTime * fSpeed);
                    AnimateMove(direction);
                    do
                        yield return null;
                    while (Timer.hasPaused); //timer暂停时禁止移动 
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
            popUpAnimator.SetInteger("Result",0); //关掉表情气泡
        }
    }

    public IEnumerator Reaction(GameObject interrupter)
    {
        StopAllCoroutines();
        AnimateStopMove();
        FaceTheTarget(interrupter);
        yield return ShowPopUp("Result",2f);
        if (interrupter == action.target) //the person I was supposed to find
            StartCoroutine(ProsessSchedule());
        else
            StartCoroutine(ProsessSchedule(action));
    }

    public IEnumerator MoveTo(GameObject gameObj)
    {
        StopAllCoroutines();
        AnimateStopMove();
        Interactable target = gameObj.GetComponent<Interactable>();
        List<AStar.Node> path = astar.FindPath(transform.position, gameObj.transform.position);
        if (path != null)
        {
            for (int i = 0; i < path.Count - target.interactDisdence; i++)
            {
                AStar.Node node = path[i];
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

    //AI行为协程
    IEnumerator ProsessSchedule(Action item = null)
    {
        //Debug.Log($"NPC {name} ProsessSchedule ACTION");
        yield return new WaitForSeconds(1);
        if(actions.Count == 0)
            yield break;
        
        
        item ??= ChooseAction();
        action = item;
        switch (item.type)
        {
            case ActionType.meeting:
                yield return StartCoroutine(TakeMeeting(item));//参数决定了npc想聊什么，以后有需要再实现传参方式
                break;
            case ActionType.use:
                yield return StartCoroutine(UseDevice(item));
                break;
            default:
                yield return null;
                break;                            
        } 
        StartCoroutine(ProsessSchedule());
    }



    Action ChooseAction()
    {
        Action maxPriorityAction = actions //.OrderByDescending(a => a.Priority).FirstOrDefault();
                                    .Where(a => !a.isWaiting)
                                    .OrderByDescending(a => a.Priority)
                                    .FirstOrDefault();
        return maxPriorityAction;
    }

    void NotifyTheTarget(NPC target)
    {
        target.StopAllCoroutines();
        target.AnimateStopMove();
    }
    public IEnumerator UseDevice(Action scheduleItem)
    {
        Device device = scheduleItem.target as Device;
        CheckSocialNeedAndPostTask(device);//检查社交需求
        //设置topicHobby
        topicHobby = device.hobby;
        yield return StartCoroutine(MoveToObj(device.gameObject));

        
        FaceTheTarget(device.gameObject);

        if (device.IsInUse)
        {
            StartCoroutine(scheduleItem.Waiting());
            yield break;
        }
            

        if (device.hobby.hobbyName != "")//如果是爱好就聊
            {
                OpenRadder();
                if (!FindConversation())
                    CreatConversation();
            }
        
        StartCoroutine(ShowPopUp("Result", device.duration));
        yield return StartCoroutine(device.Interact(this));

        QuitConversation();
        CloseRadder();
    }
    public void CheckSocialNeedAndPostTask(Device device)
    {
        Need soialNeed = lifeController.GetNeed("SocialNeed");
        if (soialNeed.value < 20)
        {
            PostGroupTask(device);
        }
    }
    private void PostGroupTask(Device device)
    {
        foreach (Group group in groups)
        {
            if (group.hobby.devices.Contains(device))
            {
                YouChat.Instance.PostTask(new TaskManager.TaskData
                {
                    taskName = $"{group.groupName}圈 {Timer.Time.dd}/{Timer.Time.hh}/{Timer.Time.mm} 冲{device.name}活动",
                    needName = device.gain.needName,
                    targetValue = 50,//属性振幅目标
                    interactables = new List<Interactable> { device },
                    duration = 60 // 默认60分钟
                }, group, this);
                //taskManager.AcceptTask(group.activeTask);
                break;
            }
        }  
    }
    
    //检查是否有任务需要接受
    public void CheckTask()
    {
        if (groups.Count == 0)
            return;
        foreach (Group group in groups)
        {
            if (group.activeTask != null)
                if (group.activeTask.taskName != "")
                    if (!taskManager.tasks.Exists(a => a.taskName == group.activeTask.taskName))
                    {
                        taskManager.AcceptTask(group.activeTask);
                        break;
                    }
        }
    }

    public IEnumerator TakeMeeting(Action item)
    {

        NPC npc = item.target as NPC;
        if (npc.users.Count == 0) //npc未被占用
        {
            NotifyTheTarget(npc); //叫住对面NPC
            if (!npc.users.Contains(this))
            {
                yield return StartCoroutine(MoveToObj(npc.gameObject));
                FaceTheTarget(npc.gameObject);
                isMeeting = true;
                StartCoroutine(npc.Interact(this));
                yield return StartCoroutine(ShowPopUp("Result", 1f));
                isMeeting = false;
                npc.users = null;
            }
        }
    }

    void OnEnable()
    {
        StartCoroutine(ProsessSchedule());
    }
    // Start is called before the first frame update
    new void Awake()
    {
        base.Awake();
    }

    new void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
