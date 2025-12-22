using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(WorkManager))]
[RequireComponent(typeof(DialogueController))]
[RequireComponent(typeof(ActionManager))]
[RequireComponent(typeof(SocialManager))]
[RequireComponent(typeof(LifeController))]
public class Character : Interactable
{
    public float fSpeed;
    [HideInInspector] public LifeController lifeController;
    [HideInInspector] public DialogueController dialogueController;
    [HideInInspector] public SocialManager socialManager;
    [HideInInspector] public WorkManager workManager;
    [HideInInspector] public ActionManager actionManager;
    [HideInInspector] public Vector3 spawnPosition;
    public Sprite profilePic;


    public List<TalentSkill> talentSkills = new();
    public List<Buff> buffs;
    public List<Skill> skills;
    public List<AffinityEffectArgs> affinityEffectArgsList; //好感度效果参数
    public List<AffinityEffect> affinityEffects; 
    public bool IsMoving;
    public bool IsBusy;
    [HideInInspector] public Animator popUpAnimator;

    public List<Group> hoppyGroups;
    public List<Company.Group> jobGroups;

    public Vector2 personalValues; //个人价值观（保守-开放，自我-超越）

    public override IEnumerator Interact(Character interactor)
    {
        throw new NotImplementedException();
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






    private void TalentSkillInitalize()
    {
        int[] cost = { 1, 1 };
        talentSkills = new()
        {
            new("TalentSkill1","AddDice",(Talent)lifeController.talentListManager.attributes[0],1,cost),
            new("TalentSkill2","AddPoint",(Talent)lifeController.talentListManager.attributes[1],2,cost),
            new("TalentSkill3","ReduceTime",(Talent)lifeController.talentListManager.attributes[2],3,cost),
        };
    }

    /*
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
    }
    */

    // Start is called before the first frame update
    protected new void Awake()
    {
        base.Awake();
        socialManager = GetComponent<SocialManager>();
        lifeController = GetComponent<LifeController>();
        Debug.Assert(lifeController != null, "lifecontruller 不可为空");
        dialogueController = GetComponent<DialogueController>();
        Debug.Assert(dialogueController != null, "dialogueController 不可为空");
        workManager = GetComponent<WorkManager>();
        Debug.Assert(workManager != null, "workManager 不可为空");
        actionManager = GetComponent<ActionManager>();
        popUpAnimator = transform.Find("Character_PopUp").GetComponent<Animator>();
        Debug.Assert(popUpAnimator != null, "popUpAnimator 不可为空");

        //TalentSkillInitalize();
    }

    protected void Start()
    {
        spawnPosition = transform.position;
        buffs = new();
        skills = new();
        affinityEffects = new();
        //InitializeActions();
    }

}

