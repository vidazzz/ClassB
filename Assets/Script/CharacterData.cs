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
    public Character owner;
    public bool isWaiting;
    public float priorityWork;
    public float priorityHobby;
    public float priorityStat;
    public float priorityTask;
    public float Priority { get { return priorityWork + priorityHobby + priorityStat + priorityTask; } }

    public Action(ActionType type, Interactable target, Character owner)
    {
        this.type = type;
        this.target = target;
        this.owner = owner;
        isWaiting = false;
        priorityWork = 0;
        priorityHobby = 0;
        priorityStat = 0;
        priorityTask = 0;
        UpdatePriorityWork();
        InitializePriorityHobby();
    }

    public IEnumerator Waiting()
    {
        isWaiting = true;
        Timer.Date beginTime = Timer.Time;
        while (Timer.Time - beginTime < target.duration)
        {
            yield return null;
        }
        isWaiting = false;
    }

    public void UpdatePriorityWork()
    {
        if (target is not WorkPC)
            return;
        Company.Project.Task task = owner.workManager.CurrentTask;
        if (task != null)
        {
            // 根据任务进度和截止时间计算优先级
            if( Timer.Time >= task.deadLine)
                priorityWork = 999; // 任务已过截止时间，优先级最高
            else
                priorityWork = (task.maxProgress - task.currentProgress) / (task.deadLine - Timer.Time);
        }
        else
        {
            priorityWork = 0;
        }
    }

    public void InitializePriorityHobby()
    {
        foreach (Character.HobbyData hobbyData in owner.hobbyDataList)
        {
            if (target is Device)
            {
                if ((target as Device).hobby == DataSetting.Hobbies[hobbyData.hobbyIndex])
                {
                    priorityHobby += hobbyData.passion * 0.1f;
                }
            }
        }
    }

    public void UpdatePriorityNeed(Need need)
    {
        //Debug.Log($"Updating action priority for {target} based on stat {need.name}");
        Need.PriorityNeed PN = need.priorityNeedList.Find(a => a.action == this);
        if (PN != null)
            priorityStat = PN.Priority;
    }
    public void UpdatePriorityNeed(Need.PriorityNeed priorityNeed)
    {
        priorityStat = priorityNeed.Priority;
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
        //Debug.Log("Apply");
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

    public static Stat operator +(Stat a, float b)
    {
        a.value += b;
        return a;
    }

    public static Stat operator -(Stat a, float b)
    {
        a.value -= b;
        return a;
    }

    public static Stat operator *(Stat a, float b)
    {
        a.value *= b;
        return a;
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
            foreach (Action action in character.actions)
            {
                PriorityNeed priorityNeed = new(action, this);
                priorityNeedList.Add(priorityNeed);
                action.UpdatePriorityNeed(priorityNeed); //初始化需求优先级
            }
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
            if (Timer.Time - beginTime >= 10)
            {
                int n = (Timer.Time - beginTime) / 10; //有可能过去了n个10游戏分钟
                MotifyValue(-decay * n);
                beginTime = Timer.Time;
            }
            yield return _waitForSeconds1;
        }
    }

    public void MotifyValue(float motifier, StatModifierBuff.ModifierType modifierType = StatModifierBuff.ModifierType.Add)
    {
        State originStatState = NeedState;
        switch (modifierType)
        {
            case StatModifierBuff.ModifierType.Add:
                AddModifier(motifier);
                break;
            case StatModifierBuff.ModifierType.Multiply:
                MultiplyModifier(motifier);
                break;
            default:
                AddModifier(motifier);
                break;
        }
        //状态变化时更新行动优先级
        if (originStatState != NeedState)
        {
            foreach (Action action in owner.actions)
            {
                action.UpdatePriorityNeed(this);
            }   
        }
        EventManager.Instance.MutifyNeeds();
    }
    public void AddModifier(float modifier)
    {
        float newValue = value + modifier;
        if (newValue >= 0)
            value = newValue;
        else
            value = 0;
    }
    public static Need operator +(Need a, float b)
    {
        a.MotifyValue(b);
        return a;
    }
    public void MultiplyModifier(float modifier)
    {
        float newValue = value * modifier;
        if (newValue >= 0)
        {
            value = newValue;
        }
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

        public PriorityNeed(Action action, Need need)
        {
            this.action = action;
            this.need = need;
            //Debug.Log("stateName:" + name + " target.name:" + action.target.name + " gain:" + action.target.gain.needName + " cost:" + action.target.cost.needName);
            if (action.target.gain.needName == need.name) // 只考虑与该需求相关的行为
            {
                
                VFM = action.target.cost.value < 1 ? action.target.gain.value - action.target.cost.value : action.target.gain.value / action.target.cost.value;
                efficiency = action.target.gain.value / action.target.duration;
            }
            else if (action.target.cost.needName == need.name)
            {
                VFM = action.target.gain.value < 1 ? -action.target.cost.value + action.target.gain.value : -action.target.cost.value / action.target.gain.value;
                efficiency = -action.target.cost.value / action.target.duration;
                
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

public class Operation
{
    public Character owner;
    public List<string> checkItems = new();
    public List<Character.Value> outputs = new(); 
    public bool isCompleted;
    public float maxProgress;
    public float currentProgress;
    public float Progress { get { return currentProgress / maxProgress; } }
    public float efficiency;
    public Operation(Character owner, List<string> checkItems, List<Character.Value> outputs, float maxProgress)
    {
        this.owner = owner;
        this.checkItems = checkItems;
        this.outputs = outputs;
        this.maxProgress = maxProgress;
        currentProgress = 0;
    }
    public Operation()
    {
    }
    public void CaculateEfficiency()
    {
        efficiency = 0;
        foreach (string input in checkItems)
        {
            efficiency += owner.TryFindValue(input);
        }
    }
    public IEnumerator ProcessHero()
    {
        CaculateEfficiency();
        float originTimeScale = Time.timeScale;
        Time.timeScale *= 20;//加速

        Timer.Date beginTime = Timer.Time;
        while (Input.GetKey(KeyCode.E)) //持续操作
        {
            yield return null;
            if (Timer.Time - beginTime >= 1)
            {           
                currentProgress += efficiency;
                beginTime = Timer.Time;
                if (currentProgress >= maxProgress)
                {
                    currentProgress = maxProgress;
                    yield return null;
                    Completed();
                    break;
                }
            }
        }
        Time.timeScale = originTimeScale;//恢复时间流逝速度
    }
    public IEnumerator ProcessNPC()
    {
        CaculateEfficiency();
        Timer.Date beginTime = Timer.Time;
        while (Timer.Time - beginTime <= 10) //持续操作10游戏分钟
        {
            yield return null;
            if (Timer.Time - beginTime >= 1)
            {
                Debug.Log(efficiency);
                currentProgress += efficiency;
                beginTime = Timer.Time;
                if (currentProgress >= maxProgress)
                {
                    currentProgress = maxProgress;
                    yield return null;
                    Completed();
                    break;
                }
            }
        }
    }
    protected void Completed()
    {
        isCompleted = true;
        foreach (Character.Value output in outputs)
        {
            owner.MotifyValue(output);
        }
    }
}
