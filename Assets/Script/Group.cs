using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    [HideInInspector]
    public string groupName; // 群组名称
    public Sprite profilePic; // 群组头像
    public AttributeID hobbyId; // 主题（如“冒险”“美食”，决定活动属性）
    public int level; // 等级（初始1级）
    public float currentActive; // 当前活跃度
    public float decayValue; // 每日衰减值（= level * 0.5，随等级提高）
    public List<TaskManager.TaskData> activeTasks; // 可以发起的活动列表
    public TaskManager.TaskData activeTask; // 当前进行中的活动
    public List<Character> members; // 成员列表
    public List<Skill> Skills; // 提供增益
    public List<Skill> unlockedSkills; // 解锁技能列表

    public YouChat.Chat chat; // 关联的聊天群

    public void InitializeGroup(AttributeID hobbyId, List<Character> initialMembers, Sprite profilePic = null)
    {
        groupName = hobbyId.ToString() + "群";
        this.hobbyId = hobbyId;
        this.profilePic = profilePic;
        level = 1;
        currentActive = 0f;
        decayValue = level * 0.5f; // 初始衰减值
        members = initialMembers;
        Skills = new List<Skill>();
        unlockedSkills = new List<Skill>();
        chat = new YouChat.Chat
        {
            chatName = groupName,
            profilePic = this.profilePic,
            groupMembers = members,
            chatHistory = new List<YouChat.Message>(),
            messageItems = new List<GameObject>(),
            group = this
        };
        InitializeActiveTasks();
    }
    public void InitializeActiveTasks()
    {
        activeTasks = new List<TaskManager.TaskData>();
        foreach (Interactable interactable in DataSetting.Instance.actionList.Find(a => a.theme == hobbyId).interactables)
        {
            TaskManager.TaskData newTask = new()
            {
                taskName = $"{hobbyId}圈 {Timer.Time.dd}/{Timer.Time.hh}/{Timer.Time.mm} 冲{interactable.name}活动",
                interactable = interactable,
                duration = 60 // 默认截止时间1小时
            };
            activeTasks.Add(newTask);
        }
    }

    public void AddMember(Character character)
    {
        if (!members.Contains(character))
        {
            members.Add(character);
            Debug.Log($"Character {character.name} added to group {groupName}.");
        }
        else
        {
            Debug.LogWarning($"Character {character.name} is already a member of group {groupName}.");
        }
    }

    public void HangoutSettleAccounts()
    {
        currentActive += 10f; // 活跃度增加
        Debug.Log($"Group {groupName} is hanging out. Current active: {currentActive}");
    }

    public void DecayActive()
    {
        currentActive -= decayValue; // 应用衰减
        if (currentActive < 0) currentActive = 0; // 确保不为负
        Debug.Log($"Group {groupName} decayed. Current active: {currentActive}");
    }

    public bool TryPostTask(TaskManager.TaskData taskData)
    {
        if (activeTask != null)
        {
            if (activeTask.taskName != "")
            {
                Debug.Log($"Group {groupName} already has an active task: {activeTask.taskName}");
                return false;
            }        
        }
        activeTask = taskData;
        NotifyMembers();
        StartCoroutine(ClearActiveTaskWhenExpire());
        Debug.Log($"Group {groupName} posted a new task: {taskData.taskName}");
        Debug.Log($"Group {groupName} posted a new task: {activeTask.taskName}");
        return true;
    }
    public void NotifyMembers()
    {
        foreach (Character member in members)
        {
            if (member is NPC npc)
                npc.CheckTask();
        }
    }
    public IEnumerator ClearActiveTaskWhenExpire()
    {
        Timer.Date beginTime = Timer.Time;
        Timer.Date deadLine = beginTime + activeTask.duration;
        while (Timer.Time < deadLine) //截止时间未到
        {
            yield return null;
        }
        activeTask = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
