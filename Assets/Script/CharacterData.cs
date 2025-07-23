using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Talent
{
    public string name;
    public int id;
    public TalentType type;
    public int level;
    public void LevelUp()
    {
        level ++;
    }
    
}

public enum TalentType{
    none = 0,
    information,
    implement,
    mental,
}

// Buff基类
public abstract class Buff
{
    public string Name { get; protected set; }
    public float Duration { get; protected set; }
    public float RemainingTime { get; protected set; }
    public bool IsPermanent { get; protected set; }
    public bool IsStackable { get; protected set; }
    public int StackCount { get; protected set; } = 1;
    public int MaxStacks { get; protected set; } = 1;

    protected Character target; // 应用到的目标

    public Buff(string name, float duration = -1, bool isStackable = false, int maxStacks = 1)
    {
        Name = name;
        Duration = duration;
        RemainingTime = duration;
        IsPermanent = duration <= 0;
        IsStackable = isStackable;
        MaxStacks = maxStacks;
    }
    public Buff(Buff targetBuff)
    {
        Name = targetBuff.Name;
        Duration = targetBuff.Duration;
        RemainingTime = targetBuff.RemainingTime;
        IsPermanent = targetBuff.IsPermanent;
        IsStackable = targetBuff.IsStackable;
        MaxStacks = targetBuff.MaxStacks;
        target = targetBuff.target;
    }

    // 应用Buff效果
    public virtual void Apply(Character target)
    {
        this.target = target;
        OnApply();
        if(!IsPermanent)
            this.target.StartCoroutine(BuffTimer());
    }

    // 移除Buff效果
    public virtual void Remove()
    {
        OnRemove();
    }

    // 刷新Buff（如延长持续时间或叠加层数）
    public virtual void Refresh(float newDuration = -1)
    {
        if (IsStackable && StackCount < MaxStacks)
        {
            StackCount++;
            OnStack();
        }
        
        if (newDuration > 0)
        {
            Duration = newDuration;
            if (!IsPermanent)
                RemainingTime = newDuration;
        }
        else if (!IsPermanent)
        {
            RemainingTime = Duration;
        }
    }

    // 每帧更新（处理持续时间）
    public virtual IEnumerator BuffTimer()
    {
        while(true)
        {
            RemainingTime -= Timer.DeltaTime;;
            if (RemainingTime <= 0)
            {
                Remove();
                break;
            }
            yield return null;
        }
    }

    // 子类可重写的回调方法
    protected virtual void OnApply() { }
    protected virtual void OnRemove() { }
    protected virtual void OnStack() { }
}

// 属性修改Buff
public class StatModifierBuff : Buff
{
    public enum ModifierType { Add, Multiply }
    
    private string statName;
    private float value;
    private float modifier;
    private float level;
    private ModifierType type;

    public StatModifierBuff(string name, string statName, float value, float modifier,
                            ModifierType type = ModifierType.Add, int level = 1, float duration = -1, bool isStackable = false) 
        : base(name, duration, isStackable)
    {
        this.statName = statName;
        this.value = value;
        this.modifier = modifier;
        this.type = type;
        this.level = level;
    }
    public StatModifierBuff(StatModifierBuff targetBuff): base(targetBuff.Name,targetBuff.Duration,targetBuff.IsStackable)
    {
        statName = targetBuff.statName;
        value = targetBuff.value;
        modifier = targetBuff.modifier;
        type = targetBuff.type;
        level = targetBuff.level;
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }

    protected override void OnApply()
    {
        LifeController stats = target.lifeController;
        Debug.Log("to "+ target +": "+ statName + " <=" + (value + modifier * level) * StackCount);
        if (type == ModifierType.Add)
            stats.AddModifier(statName, (value + modifier * level ) * StackCount);
        else
            stats.MultiplyModifier(statName, 1 + ((value + modifier * level ) * StackCount));
        
        Debug.Log(stats.statsPairs[statName]);
    }

    protected override void OnRemove()
    {
        LifeController stats = target.lifeController;
        if (type == ModifierType.Add)
            stats.AddModifier(statName, -(value + modifier * level ) * StackCount);
        else
            stats.MultiplyModifier(statName, 1 / (1 + ((value + modifier * level ) * StackCount)));
    }

    protected override void OnStack()
    {
        // 重新应用效果以更新堆叠值
        OnRemove();
        OnApply();
    }
}

public class Skill
{
    public string name;
    public List<Buff> buffs;
    private int level = 1;
    protected Character target; // 应用到的目标

    public Skill(string name,int[] buffIndexArrey,int level = 1)
    {
        this.name = name;
        this.level = level;
        buffs = new();
        foreach(int index in buffIndexArrey)
        {
            buffs.Add(DataSetting.Instance.buffs[index]); //引用而非创建，共用buff表
        }
    }
    public Skill(Skill targetSkill)
    {
        name = targetSkill.name;
        buffs = targetSkill.buffs;
        level = targetSkill.level;
        target = targetSkill.target;
    }

    public void Apply(Character target)
    {
        this.target = target;
        foreach(StatModifierBuff buff in buffs)
        {
            buff.SetLevel(level);
            buff.Apply(target);
        }
    }

    public void Remove()
    {
        foreach(StatModifierBuff buff in buffs)
        {
            buff.Remove();
        }
    }
    
    public void LevelUp()
    {
        Remove();
        level ++;
        Apply(target);
    }
}

public class AffinityEffect : Skill
{
    private int affinityThreshold;
    public bool isActive;
    public Character owner;
    public AffinityEffect(AffinityEffectArgs affinityArgs,Character owner, Character target) : base(affinityArgs.name, affinityArgs.buffIndexArrey)
    {
        affinityThreshold = affinityArgs.affinityThreshold;
        this.owner = owner;
        base.target = target;
        TryUpdate();
    }

    public void TryUpdate()
    {
        float affinityValue = Community.affinity.GetAfinity(owner,target);
        if (affinityValue >= affinityThreshold && isActive == false)
            Apply(target);
        else if(affinityValue < affinityThreshold && isActive == true)
            Remove();
    }

    new public void Apply(Character target)
    {
        Debug.Log("Apply");
        isActive = true;
        base.Apply(target);
    }
    new public void Remove()
    {
        isActive = false;
        base.Remove();
    }
}

[Serializable]
public struct AffinityEffectArgs{
    public string name;
    public int[] buffIndexArrey;
    public int affinityThreshold;
}
