using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Talent
{
    public string TalentName;
    public int id;
    public TalentType type;
}

public enum TalentType
{
    none = 0,
    information,
    implement,
    mental,
}

[Serializable]
public class Action
{
    public ActionType type;
    public Interactable target;
    public bool isWaiting;
    public float priorityJob;
    public float priorityHobby;
    public float priorityStat;
    public float priorityTask;
    public float Priority { get { return priorityJob + priorityHobby + priorityStat + priorityTask; } }

    public IEnumerator Waiting()
    {
        isWaiting = true;
        Timer.Date beginTime = Timer.Time;
        while (Timer.GetPassedMinutes(beginTime) < target.duration)
        {
            yield return null;
        }
        isWaiting = false;
    }
}

[Serializable]
public enum ActionType{
    none = 0,
    use,
    meeting,
}

public enum PreesureLevel {
    none = 0,
    low,
    normal,
    high,
    varyHigh,
    overCome,
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
        //Debug.Log("to "+ target +": "+ statName + " <=" + (value + modifier * level) * StackCount);
        if (type == ModifierType.Add)
            stats.TryMotifyStat(statName, (value + modifier * level ) * StackCount);
        else
            stats.MultiplyModifier(statName, 1 + ((value + modifier * level ) * StackCount));
        
        //Debug.Log(stats.GetStatValue(statName));
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
    public int level = 1;
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
        float affinityValue = Community.affinity.GetAffinity(owner,target);
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
public struct AffinityEffectArgs
{
    public string name;
    public int[] buffIndexArrey;
    public int affinityThreshold;
}

[Serializable]
public class Hobby
{
    public string hobbyName;
    public int id;
    public List<Talent> talents;
    public List<Device> devices;
}
[Serializable]
public class Stat
{
    public string name;
    public float value;
    public float expectation = 100;

    public State StatState { get { return GetStatState(); } }

    public Stat(string name, float value, float expectation = 100)
    {
        this.name = name;
        this.value = value;
        this.expectation = expectation;
    }

    public State GetStatState()
    {
        if (value < expectation * 0.3f)
            return State.low;
        else if (value < expectation * 0.8f)
            return State.middle;
        else
            return State.high;
    }

    [Serializable]
    public enum State
    {
        none = 0,
        low,
        middle,
        high,
    }
}

[Serializable]
public class Need
{
    private static WaitForSeconds _waitForSeconds1 = new(1f);
    public string name;
    public Character owner;
    public float value;
    public float decay;//每10游戏分钟自然衰减值
    public float expectation;
    public bool isChanging;

    public List<PriorityNeed> priorityNeedList = new();// 与该属性相关的行为优先级
    public State NeedState { get { return GetNeedState(); } }

    public Need(Character character, string name, float value, float decay = 1, float expectation = 100)
    {
        owner = character;
        this.name = name;
        this.value = value;
        this.decay = decay;
        this.expectation = expectation;
        if (character is NPC)
        {
            priorityNeedList = SetUpActionPriorities((character as NPC).actions);
        }
        owner.StartCoroutine(ProcessDecay());
    }
    
    public State GetNeedState()
    {
        if (value < expectation * 0.3f)
            return State.dangerous;
        else if (value < expectation * 0.8f)
            return State.warning;
        else
            return State.normal;
    }

    public IEnumerator ProcessDecay()
    {
        Timer.Date beginTime = Timer.Time;
        while (true)
        {
            if (isChanging)//正在进行相关的活动，暂停衰减
            {
                beginTime = Timer.Time;
                yield return _waitForSeconds1;
                continue;
            }
            if (Timer.GetPassedMinutes(beginTime) >= 10)
                {
                    int n = Timer.GetPassedMinutes(beginTime) / 10; //有可能过去了n个10游戏分钟
                    TryMotifyValue(-decay * n);
                    beginTime = Timer.Time;
                }
            yield return _waitForSeconds1;
        }
    }

    public List<PriorityNeed> SetUpActionPriorities(List<Action> actions)
    {
        List<PriorityNeed> actionPriorities = new();
        foreach (Action action in actions)
        {
            //Debug.Log("stateName:" + name + " target.name:" + action.target.name + " gain:" + action.target.gain.needName + " cost:" + action.target.cost.needName);
            if (action.target.gain.needName == name) // 只考虑与该需求相关的行为
            {
                PriorityNeed actionPriority = new()
                {
                    action = action,
                    need = this,
                    VFM = action.target.cost.value < 1 ? action.target.gain.value - action.target.cost.value : action.target.gain.value / action.target.cost.value,
                    efficiency = action.target.gain.value / action.target.duration
                };
                actionPriorities.Add(actionPriority);
            }
            else if (action.target.cost.needName == name)
            {
                PriorityNeed actionPriority = new()
                {
                    action = action,
                    need = this,
                    VFM = action.target.gain.value < 1 ? - action.target.cost.value + action.target.gain.value : - action.target.cost.value / action.target.gain.value,
                    efficiency = -action.target.cost.value / action.target.duration
                };
                actionPriorities.Add(actionPriority);
            }
        }
        if (actionPriorities.Count == 0)
            Debug.LogWarning($"{name}需求没有相关行为");
        return actionPriorities;
    }

    public bool TryMotifyValue(float motifier, StatModifierBuff.ModifierType modifierType = StatModifierBuff.ModifierType.Add)
    {
        switch (modifierType)
        {
            case StatModifierBuff.ModifierType.Add:
                return AddModifier(motifier);
            case StatModifierBuff.ModifierType.Multiply:
                MultiplyModifier(motifier);
                return true;
            default:
                return AddModifier(motifier);
        }
    }
    public bool AddModifier(float modifier)
    {
        float newValue = value + modifier;
        State originStatState = NeedState;
        if (newValue >= 0)
        {
            value = newValue;
            if (originStatState != NeedState)
            {
                owner.lifeController.UpdatePriorityNeed(); //状态变化时更新行动优先级
            }
        }
        else
        {
            //PopUp.Instance.ShowPopUp($"{name} is not enough!");
            return false; //如果不足则不修改
        }
        EventManager.Instance.MutifyNeeds();
        owner.lifeController.UpdatePriorityNeed();
        return true;
    }

    public void MultiplyModifier(float modifier)
    {
        float newValue = value * modifier;
        if (newValue >= 0)
        {
            value = newValue;
        }
        EventManager.Instance.MutifyNeeds();
        owner.lifeController.UpdatePriorityNeed();
    }

    [Serializable]
    public class PriorityNeed
    {
        public Action action;
        public Need need;
        public float VFM; // 性价比 gain / cost
        public float efficiency; // 效率 gain / time
        // 优先级计算公式
        public float Priority
        {
            get
            {
                return need.NeedState switch
                {
                    State.dangerous => efficiency,
                    State.warning => VFM,
                    State.normal => 0,// normal状态时需求相关行为的优先级归零
                    _ => 0,
                };
            }
        }
    }

    [Serializable]
    public enum State
    {
        none = 0,
        dangerous,
        warning,
        normal,
    }

}

