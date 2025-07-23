using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : Interactable
{
    [SerializeField] protected float fSpeed;
    [SerializeField] protected PopUp popUp;
    [HideInInspector] public LifeController lifeController;
    [HideInInspector] public Vector3 spawnPosition;
    public Dictionary<string,Talent> talents;
    public List<Buff> buffs;
    public List<Skill> skills;
    public List<AffinityEffectArgs> affinityEffectArgsList; //好感度效果参数
    public List<AffinityEffect> affinityEffects; 
    public bool IsMoving;
    public override IEnumerator Interact(Character interactor)
    {
        throw new NotImplementedException();
    }
    public IEnumerator ShowPopUp(string reactionType,float duration)
    {
        popUp.animator.SetInteger(reactionType,1);
        //Debug.Log(gameObject + " Interacting "+target.gameObject);
        yield return new WaitForSeconds(duration);
        popUp.animator.SetInteger(reactionType,0);
    }

    public void FaceTheTarget(GameObject target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        animator.SetFloat("X",Mathf.RoundToInt(direction.x));
        animator.SetFloat("Y",Mathf.RoundToInt(direction.y));
    }

    //学习技能
    //目前游戏里只有被动效果技能，基本上等于加永久buff
    //没有目标技能时，创建技能1级
    //已有目标技能时，升一级
    public void LearnSkill(int index)
    {
        string skillName = DataSetting.Instance.skills[index].name;
        Skill targetSkill = null;
        foreach(Skill skill in skills)
        {
            if(skill.name.Equals(skillName))
            {
                targetSkill = skill;
                break;
            }
        }
        if(targetSkill == null)
        {
           targetSkill = CreateSkill(index);
           targetSkill.Apply(this);
        }
        else
            targetSkill.LevelUp();
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
        Debug.Log("TryUpdateAffinityEffect");
        foreach(AffinityEffect affinityEffect in affinityEffects)
        {
            Debug.Log(affinityEffect.owner == target);
            if(affinityEffect.owner == target)
                affinityEffect.TryUpdate();
        }
    }
    
    protected void DataInitialize()
    {
        int i = 0;
        foreach(var name in DataSetting.Instance.talentNames)
        {
            Talent newTalent = new()
            {
                id = i,
                name = name,
                level = 2,
            };
            talents.Add(newTalent.name,newTalent);
            i ++;
        }
    }

    // Start is called before the first frame update
    new protected void Awake()
    {
        base.Awake();
        lifeController = GetComponent<LifeController>();
        Debug.Assert(lifeController != null,"lifecontruller 不可为空");
        spawnPosition = transform.position;
        talents = new();
        buffs = new();
        skills = new();
        affinityEffects =new();
    }

    protected void Start()
    {
        DataInitialize(); 
    }

}
