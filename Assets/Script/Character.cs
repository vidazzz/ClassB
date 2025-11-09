using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(WorkManager))]
[RequireComponent(typeof(DialogueController))]
public class Character : Interactable
{
    [SerializeField] protected float fSpeed;
    [HideInInspector] public LifeController lifeController;
    [HideInInspector] public DialogueController dialogueController;

    [HideInInspector]
    public WorkManager workManager;
    [HideInInspector] public Vector3 spawnPosition;
    public Sprite profilePic;
    public float conversationAffinityThreshold;
    public List<TalentData> talentDataList;
    [Serializable]
    public struct TalentData
    {
        public Talent talent;
        public int talentIndex;
        public int value;
    }
    public List<TalentSkill> talentSkills = new();
    public Action action;
    public List<Action> actions;
    public List<HobbyData> hobbyDataList;

    [Serializable]
    public struct HobbyData
    {
        public Hobby hobby;
        public int hobbyIndex;
        public float passion;
    }
    public List<Buff> buffs;
    public List<Skill> skills;
    public List<AffinityEffectArgs> affinityEffectArgsList; //好感度效果参数
    public List<AffinityEffect> affinityEffects; 
    public bool IsMoving;
    protected Animator popUpAnimator;
    public Conversation conversation;

    [SerializeField]
    protected Hobby topicHobby;
    public List<Group> hoppyGroups;
    public List<Company.Group> jobGroups;
    //数值
    [Serializable]
    public struct Value
    {
        public string valueName;
        public float value;
 
    }

    public override IEnumerator Interact(Character interactor)
    {
        throw new NotImplementedException();
    }
    public IEnumerator ShowPopUp(string reactionType,float duration)
    {
        popUpAnimator.SetInteger(reactionType,1);
        //Debug.Log(gameObject + " Interacting "+target.gameObject);
        yield return new WaitForSeconds(duration);
        popUpAnimator.SetInteger(reactionType,0);
    }

    public void FaceTheTarget(GameObject target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        animator.SetFloat("X",Mathf.RoundToInt(direction.x));
        animator.SetFloat("Y",Mathf.RoundToInt(direction.y));
    }

    public object GetTalent(string talentName)
    {
        TalentData talentData = talentDataList.Find(a => a.talent.TalentName == talentName);
        if (talentData.talent == null)
        {
            Debug.LogWarning($"{gameObject} Talent {talentName} not find!");
            return null;
        }
        else
        {
            return talentData;
        }
    }
    
    //修改数值
    public void MotifyValue(Value value)
    {
        object obj = TryGetValueObj(value.valueName);
        switch (obj)
        {
            case Stat stat:
                stat += value.value;
                break;
            case TalentData talentData:
                talentData.value += (int)value.value;
                break;
            case HobbyData hobbyData:
                hobbyData.passion += value.value;
                break;
            case Need need:
                need.MotifyValue(value.value);
                break;
            case Company.Project.Task task:
                task.currentProgress += value.value;
                break;
            default:
                Debug.LogError("invalid checkObj type!");
                break;
        }
    }

    //学习技能
    //目前游戏里只有被动效果技能，基本上等于加永久buff
    //没有目标技能时，创建技能1级
    //已有目标技能时，升一级
    public void LearnSkill(int index)
    {
        string skillName = DataSetting.Instance.skills[index].name;
        Skill targetSkill = null;
        foreach (Skill skill in skills)
        {
            if (skill.name.Equals(skillName))
            {
                targetSkill = skill;
                break;
            }
        }
        if (targetSkill == null)
        {
            targetSkill = CreateSkill(index);
            targetSkill.Apply(this);
            Debug.Log($"Learned new skill: {targetSkill.name}");
            PopUp.Instance.ShowPopUp($"Learned new skill: {targetSkill.name}");
            // 这里可以添加技能学习的特效或音效
        }
        else
        {
            targetSkill.LevelUp();
            Debug.Log($"Skill {targetSkill.name} leveled up to level {targetSkill.level}");
            PopUp.Instance.ShowPopUp($"Skill {targetSkill.name} leveled up to level {targetSkill.level}");
            // 这里可以添加技能升级的特效或音效
        }

    }
    protected Skill CreateSkill(int index)
    {
        Skill targetSkill = DataSetting.Instance.skills[index];
        Skill newSkill = new(targetSkill);
        newSkill.buffs = CreateStatModifierBuffs(newSkill.buffs);
        skills.Add(newSkill);
        return newSkill;
    }

    protected List<Buff> CreateStatModifierBuffs(List<Buff> targetBuffs)
    {
        List<Buff> newSkillBuffs = new();
        foreach(Buff buff in targetBuffs)
        {
            StatModifierBuff newSkillBuff = new(buff as StatModifierBuff);
            newSkillBuffs.Add(newSkillBuff);
            buffs.Add(newSkillBuff);
        }
        return newSkillBuffs;
    }

    public void TryUpdateAffinityEffect(Character target)
    {
        foreach(AffinityEffect affinityEffect in affinityEffects)
        {
            if(affinityEffect.owner == target)
                affinityEffect.TryUpdate();
        }
    }

    public void CreatConversation()
    {
        if (topicHobby.hobbyName == "")
        {
            Debug.LogError($"{name} try to creat conversation with no hobby!");
            return;
        }
        conversation = new(topicHobby, this);
    }


    public bool TryJoinConversation(Character target)
    {
        if (Community.affinity.GetAffinity(this, target) < conversationAffinityThreshold) //检查好感度
            return false;
        if (target.conversation != null)
        {
            conversation = target.conversation;
            conversation.Add(this);
            return true;
        }
        else
            return false;
    }

    public void QuitConversation()
    {
        conversation?.Remove(this);
        conversation = null;
    }

    //尝试根据给出的数值名从属性、天赋、爱好和好感度中查找数值
    public int TryFindValue(string objName, Character other = null)
    {
        object obj = TryGetValueObj(objName);
        float result = 0;
        switch (obj)
        {
            case Stat stat:
                result = stat.value;
                break;
            case TalentData talentData:
                result = talentData.value;
                break;
            case HobbyData hobbyData:
                result = hobbyData.passion;
                break;
            case Community.Affinity affinity:
                result = affinity.GetAffinity(this, other);
                break;
            case Need need:
                result = need.value;
                break;
            default:
                Debug.LogError("invalid checkObj type!");
                break;
        }
        return (int)result;
    }
    public object TryGetValueObj(string objName)
    {
        object obj;
        obj = lifeController.GetStat(objName);
        obj ??= GetTalent(objName);
        obj ??= GetHobbyData(objName);
        obj ??= lifeController.GetNeed(objName);
        if(obj == null)
        {
            if(objName == "task")
                obj = workManager.CurrentTask;
        }
        if (obj == null)
        {
            if (objName == "affinity")
                obj = Community.affinity;
        }
        if (obj == null)
            Debug.LogError("can't find checkObj!");
        return obj;
    }


    private void TalentDataInitialize()
    {
        for(int i = 0; i < talentDataList.Count; i++)
        {
            var talentData = talentDataList[i];
            talentData.talent = DataSetting.Instance.talents[talentData.talentIndex];
            talentDataList[i] = talentData;
        }
    }
    private void TalentSkillInitalize()
    {
        int[] cost = { 1, 1 };
        talentSkills = new()
        {
            new("TalentSkill1","AddDice",talentDataList[0].talent,1,cost),
            new("TalentSkill2","AddPoint",talentDataList[1].talent,2,cost),
            new("TalentSkill3","ReduceTime",talentDataList[2].talent,3,cost),

        };
    }
    private void HobbyDataInitialize()
    {
        for (int i = 0; i < hobbyDataList.Count; i++)
        {
            var hobbyData = hobbyDataList[i];
            hobbyData.hobby = DataSetting.Hobbies[hobbyData.hobbyIndex];
            hobbyDataList[i] = hobbyData;
        }
    }

    public object GetHobbyData(string hobbyName)
    {
        HobbyData result;
        result = hobbyDataList.Find(a => a.hobby.hobbyName == hobbyName);
        if (result.hobby == null)
            return null;
        else
            return result;
    }

    protected void InitializeActions()
    {
        foreach (Device device in DataSetting.Instance.devices)
        {
            if (device.owner != null)
                if (device.owner != this)
                    continue;
            Action useAction = new(ActionType.use, device, this);
            actions.Add(useAction);
        }
        lifeController.InitializeStats();
    }

    // Start is called before the first frame update
    new protected void Awake()
    {
        base.Awake();
        lifeController = GetComponent<LifeController>();
        Debug.Assert(lifeController != null, "lifecontruller 不可为空");
        dialogueController = GetComponent<DialogueController>();
        Debug.Assert(dialogueController != null, "dialogueController 不可为空");
        workManager = GetComponent<WorkManager>();
        Debug.Assert(workManager != null, "workManager 不可为空");
        popUpAnimator = transform.Find("Character_PopUp").GetComponent<Animator>();
        Debug.Assert(popUpAnimator != null, "popUpAnimator 不可为空");
        TalentDataInitialize();
        TalentSkillInitalize();
        HobbyDataInitialize();
        

    }

    protected void Start()
    {
        spawnPosition = transform.position;
        buffs = new();
        skills = new();
        affinityEffects = new();
        InitializeActions();
    }

}
