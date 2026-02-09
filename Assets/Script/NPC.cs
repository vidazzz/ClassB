using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TaskManager))]
[RequireComponent(typeof(BehaviorManager))]
public class NPC : Character
{
    [SerializeField] private AStar astar;

    public List<List<Action>> Schedule;
    protected TaskManager taskManager;
    public BehaviorManager behaviorManager;
    [HideInInspector] public Rader rader;

    public override IEnumerator Interact(Character interactor)
    {
        if (interactor is Hero) //来的是hero
        {
            Timer.Pause();
            StopAllCoroutines();
            if (dialogueController.dialogueGraph != null) //单人对话
            {
                yield return StartCoroutine(dialogueController.GraphDisplayDialogue(dialogueController.dialogueGraph));
            }
            else
                Debug.LogWarning("No dialogue has been set");
            yield return null;
            StartCoroutine(behaviorManager.ProsessSchedule(actionManager.CurrentAction));//继续执行日程
            Timer.Resume();
        }
        else //interactor is NPC
        {

        }
    }

      public void CheckSocialNeedAndPostTask(Device device)
    {
        Need soialNeed = (Need)lifeController.needListManager.GetAttributeByEnum(AttributeID.social);
        if (soialNeed.value < 20)
        {
            PostGroupTask(device);
        }
    }
    private void PostGroupTask(Device device)
    {
        foreach (Group group in hoppyGroups)
        {
            List<Device> devices = DataSetting.Instance.actionList.Find(a => a.theme == group.hobbyId).interactables.Cast<Device>().ToList();
            if (devices.Contains(device))
            {
                YouChat.Instance.PostTask(new TaskManager.TaskData
                {
                    taskName = $"{group.groupName}圈 {Timer.Time.dd}/{Timer.Time.hh}/{Timer.Time.mm} 冲{device.name}活动",
                    interactable =  device,
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
        if (hoppyGroups.Count == 0)
            return;
        foreach (Group group in hoppyGroups)
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


    // Start is called before the first frame update
    new void Awake()
    {
        base.Awake();

        rader = GetComponentInChildren<Rader>(true);
        Debug.Assert(rader != null, $"{name}  缺失 rader 组件");
        taskManager = GetComponent<TaskManager>();
        behaviorManager = GetComponent<BehaviorManager>();
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
