using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[Serializable]
public class Talent:Attribute
{
    public Talent(AttributeID id, float value, Character character)
    {
        this.id = id;
        this.value = value;
        owner = character;
    }
}

[Serializable]
public class Action
{
    public string Name {get { return theme.ToString();}}
    public AttributeID theme;
    public List<AttributeID> checkAttributeList;
    public float output;
    public InteractableData interactableData;
    public Interactable target;
    public List<Interactable> interactables = new();
    public Character owner;
    public Need need;
    public Hobby hobby;
    public Progress progress;
    public float efficiency;
    public float PriorityWork {
        get {
                if(theme == AttributeID.work)
                    return UpdatePriorityWork();
                else
                    return 0;
            }
        }
    public float PriorityHobby => hobby == null ? 0 : hobby.value * 0.1f;

    public float priority;
    public float Priority
    {
        get 
        {
            if (interactableData == null)
                priority = 0;
            else
                priority = interactableData.Priority;
            return priority;
        }
    }

    public List<InteractableData> InteractableDataList = new();

    public class InteractableData
    {
        public Action action;
        public Interactable interactable;
        public float ProgressEfficiency => action.efficiency * (interactable as Device).efficiencyMultiplier;//每游戏秒钟推条进度
        public float OutputEfficiency => output * ProgressEfficiency; //每游戏秒产出指标
        public float Duration => 100 / ProgressEfficiency; //完成所需时间，单位：游戏秒
        public float output;
        public float priorityNeed;
        public float priorityTask;
        public float Priority => action.PriorityWork + action.PriorityHobby + priorityNeed + priorityTask;
        public bool isFull;
        public InteractableData(Action action, Interactable interactable)
        {
            this.action = action;
            this.interactable = interactable;
            SetupOutputs();
            if(action.need != null)
            {
                UpdatePriorityNeed(action.need);//初始化需求优先级
                EventManager.Instance.OnNeedStateChange += UpdatePriorityNeed;//订阅需求状态变化事件
            }    
        }
        //设备占用，等会回来看

        public IEnumerator WaitForAvailableCoroutin()
        {
            float waitingTime = 10;//等10游戏分钟
            Timer.Date beginTime = Timer.Time;
            while (Timer.Time - beginTime < waitingTime)
            {
                yield return null;
            }
            isFull = false;
        }
        public void SetupOutputs()
        {
            if(interactable is Device device)
                output = action.output * device.outputMultiplier; 
        }

        public void UpdatePriorityNeed(Need need)
        {
            Debug.Log($"UpdatePriorityNeed, charactor:{need.owner}, need:{need.owner}'s {need.id}, action.need:{action.need.owner}'s {action.need.id} AttributeState:{need.State}");
            if(need != action.need)
                return;
            if(need.id == AttributeID.social && interactable is Character character)
            {
                priorityNeed = need.State switch
                {
                    Attribute.AttributeState.high => 0,
                    Attribute.AttributeState.middle => action.owner.socialManager.GetAttitudeTowards(character).ValuesAlignment*0.1f,
                    Attribute.AttributeState.low => action.owner.socialManager.GetAttitudeTowards(character).ValuesAlignment*0.2f,
                    _ => 0,
                };
            }
            else
            {
                priorityNeed = need.State switch
                {
                    Attribute.AttributeState.high => 0,
                    Attribute.AttributeState.middle => ProgressEfficiency,
                    Attribute.AttributeState.low => OutputEfficiency,
                    _ => 0,
                };
            }
            Debug.Log($"UpdatePriorityNeed changed, charactor:{need.owner}, priorityNeed:{priorityNeed}");
        }

        /*
        public void PriorityNeed(Need need)
        {
            VFM = action.target.cost.value < 1 ? action.target.gain.value - action.target.cost.value : action.target.gain.value / action.target.cost.value;
            efficiency = action.target.gain.value / action.target.duration;
            {
                VFM = action.target.gain.value < 1 ? -action.target.cost.value + action.target.gain.value : -action.target.cost.value / action.target.gain.value;
                efficiency = -action.target.cost.value / action.target.duration;
                
            }
        }
        */
    }

    public Action(Action action,Character owner)
    {
        theme = action.theme;
        this.owner = owner;
        SetUpInteractableList(action);
        checkAttributeList = action.checkAttributeList;
        CaculateEfficiency();
        output = action.output;
        SetUpNeed();
        SetUpHobby();
        SetUpInteractableDataList();
        ChooseInteractable();
    }

    public void SetUpInteractableList(Action action)
    {
        foreach(Interactable interactable in action.interactables)
        {
            if(interactable is Device device)
            {
                if(device.owner != null && device.owner != owner) //设备被占用且不是自己的，跳过
                    continue;     
            }
            if(interactable is Character character)
            {
                if(character == owner) //不能选择自己作为互动对象
                    continue;     
            }
            interactables.Add(interactable);
        }
    }

    public void WaitForAvailable()
    {
        interactableData.isFull = true;
        owner.actionManager.StartCoroutine(interactableData.WaitForAvailableCoroutin());
    }

    //根据检查的属性给予效率加成
    public void CaculateEfficiency()
    {
        foreach(AttributeID AttributeID in checkAttributeList)
        {
           efficiency += owner.lifeController.TryFindValueByEnum(AttributeID);
           Debug.Log($"AttributeID:{AttributeID} efficiency:{efficiency}");
        }
    }
    public void SetUpOutputs()
    {
        
    }
    public void SetUpInteractableDataList()
    {
        foreach(Interactable interactable in interactables)
        {
            InteractableData newDeviceData = new(this, interactable);
            InteractableDataList.Add(newDeviceData);
        }
    }
    public bool ChooseInteractable()
    {
        interactableData = InteractableDataList .Where(a => a.isFull == false)
                                    .OrderByDescending(a => a.Priority)
                                    .FirstOrDefault();
        if(interactableData == null)
        {
            target = null;
            return false;
        }
        else
        {
            target = interactableData.interactable;
            return true;
        }
            

        //Debug.Log(target.name);
    }

    public float UpdatePriorityWork()
    {
        Company.Project.Task task = owner.workManager.CurrentTask;
        //Debug.Log($"UpdatePriorityWork for {owner.name},task:{task?.taskName}");
        float result;
        if (task != null)
        {
            // 根据任务进度和截止时间计算优先级
            if (Timer.Time >= task.deadLine)
                result = 999; // 任务已过截止时间，优先级最高
            else
                result = (task.maxProgress - task.currentProgress) * 10 / (task.deadLine - Timer.Time);
            Debug.Log($"(maxProgress:{task.maxProgress} - currentProgress:{task.currentProgress}) * 10 / (deadLine:{task.deadLine.ToMinutes()} - Time:{Timer.Time.ToMinutes()})");
        }
        else
        {
            result = 0;
        }
        //Debug.Log($"{owner.name}'s work priority: {result}");
        return result;
    }

    public void SetUpHobby()
    {
        //Debug.Log(Name);
        if (owner.lifeController.hobbyListManager.attributes.Find(h => h.id == theme) is Hobby hobby)
            this.hobby = hobby;
    }

    public void SetUpNeed()
    {
        if (owner.lifeController.needListManager.attributes.Find(n => n.id == theme) is Need need)       
            this.need = need;
    }

    public IEnumerator ProcessHero()
    {
        if (need != null)
            need.isChanging = true; //将需求标记为正在操作

        if(progress == null || progress.isCompleted)
            progress = new(this);             
        yield return progress.ProcessHero(interactableData.ProgressEfficiency);

        if (need != null)
            need.isChanging = false; //进程结束，将正在操作标记恢复
    }

    public IEnumerator ProcessNPC()
    {
        if (need != null)
            need.isChanging = true; //将需求标记为正在操作

        if(progress == null || progress.isCompleted)
            progress = new(this); 
        Debug.Log("interactableData.interactable: " + interactableData.interactable + " " + target);
        Debug.Log($"interactable: {interactableData.interactable}  progressEfficiency:{interactableData.ProgressEfficiency}");
        yield return progress.ProcessNPC(interactableData.ProgressEfficiency);

        if (need != null)
            need.isChanging = false; //进程结束，将正在操作标记恢复
    }

    public void MultiplyOutput()
    {
        if(theme == AttributeID.work)
        {
            //Debug.Log("owner.workManager.CurrentTask: " + owner.workManager.CurrentTask);
            owner.workManager.CurrentTask.Process(output);
        }
        else
            owner.lifeController.AddAttributeByID(theme,output);
    }

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
    public AttributeID statId;
    private string statName;
    private float value;
    private float modifier;
    private float level;
    private ModifierType type;

    public StatModifierBuff(string name, AttributeID statId, float value, float modifier,
                            ModifierType type = ModifierType.Add, int level = 1, float duration = -1, bool isStackable = false) 
        : base(name, duration, isStackable)
    {
        this.statId = statId;
        this.value = value;
        this.modifier = modifier;
        this.type = type;
        this.level = level;
    }
    public StatModifierBuff(StatModifierBuff targetBuff): base(targetBuff.Name,targetBuff.Duration,targetBuff.IsStackable)
    {
        statId = targetBuff.statId;
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
        LifeController LC = target.lifeController;
        Attribute v = LC.statListManager.GetAttributeByEnum(statId);
        if (type == ModifierType.Add)
            v.AddModifier((value + modifier * level ) * StackCount);
        else
            v *= 1 + ((value + modifier * level ) * StackCount); 
    }

    protected override void OnRemove()
    {
        LifeController LC = target.lifeController;
        Attribute v = LC.statListManager.GetAttributeByEnum(statId);
        if (type == ModifierType.Add)
            v.AddModifier(-(value + modifier * level ) * StackCount);
        else
            v.MultiplyModifier(1 / (1 + ((value + modifier * level ) * StackCount)));
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
public class Hobby : Attribute
{
    public List<Talent> talents;
    public List<Device> devices;
    public Hobby(AttributeID id, float value, Character character)
    {
        this.id = id;
        this.value = value;
        owner = character;
        talents = new List<Talent>();
        devices = new List<Device>();
        expectation = 100;
    }
}
[Serializable]
public class Stat : Attribute
{
    public Stat(AttributeID id,float value)
    {
        this.id = id;
        this.value = value;
        expectation = 100;
    }
}

[Serializable]
public class Need : Attribute
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);
    public float decay;//每1游戏分钟自然衰减值
    public bool isChanging;

    public Need(AttributeID id, float value, Character character, float decay = 1)
    {
        this.id = id;
        owner = character;
        this.value = value;
        this.decay = decay;
        expectation = 100;
        owner.StartCoroutine(ProcessDecay());
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
            if (Timer.Time - beginTime >= 1)
            {
                int n = Timer.Time - beginTime; //有可能过去了n游戏分钟
                AddModifier(-decay * n);
                beginTime = Timer.Time;
            }
            yield return _waitForSeconds1;
        }
    }


    //重写增加对状态转变的检查
    public override void AddModifier(float modifier)
    {
        AttributeState originStatState = State;
        base.AddModifier(modifier);
        //状态变化时更新行动优先级
        if (originStatState != State)
        {
            EventManager.Instance.NeedStateChange(this);
        }
        EventManager.Instance.MutifyNeeds();
    }
    //重写增加对状态转变的检查
    public override void MultiplyModifier(float modifier)
    {
        AttributeState originStatState = State;
        base.MultiplyModifier(modifier);
        //状态变化时更新行动优先级
        if (originStatState != State)
        {
            EventManager.Instance.NeedStateChange(this);
        }
        EventManager.Instance.MutifyNeeds();
    }
}

public class Progress
{
    public Action action;
    public bool isCompleted;
    public float max = 100;
    public float value;
    public ProgressBar progressBar;
    public float Percentage { get { return value / max; } }
    public Progress(Action action)
    {
        this.action = action;
        progressBar = action.owner.actionManager.progressBar;
    }
    public IEnumerator ProcessHero(float progressEfficiency)
    {
        //CaculateEfficiency();
        float originTimeScale = Time.timeScale;
        Time.timeScale *= 20;//加速

        Timer.Date beginTime = Timer.Time;
        while (Input.GetKey(KeyCode.E)) //持续操作
        {
            yield return null;
            if (Timer.Time - beginTime >= 1)
            {
                value += progressEfficiency;
                beginTime = Timer.Time;
                if (value >= max)
                {
                    value = max;
                    yield return null;
                    Completed();
                    break;
                }
            }
        }
        Time.timeScale = originTimeScale;//恢复时间流逝速度
    }
    public IEnumerator ProcessNPC(float progressEfficiency)
    {
        progressBar.percentage = 0; 
        progressBar.gameObject.SetActive(true);
        while (value < max) //直到本次进度完成
        {
            yield return null;

            value += progressEfficiency * Timer.DeltaTime;//每帧加进度
            progressBar.percentage = Percentage;
                   
        }
        value = max;
        yield return null;
        Completed();
        progressBar.percentage = 1;   
        progressBar.gameObject.SetActive(false);
    }
    protected void Completed()
    {
        isCompleted = true;
        action.MultiplyOutput();
        Debug.Log($"{action.owner.name} completed {action.Name} action");
    }
}

[Serializable]
public enum AttributeID
    {
        none = 0,
        //stat
        level = 101,exp = 102,money = 103,preesure = 104,
        //talent
        point = 200,speak = 201,see = 202,act = 203,work =204,
        //need
        eat = 301,sleep = 302,toilet = 303,fun = 304,social = 305,
        //hobby
        gaming = 401,reading = 402,sports = 403,music = 404,art = 405,coffee = 406,
        //work
        kpi = 501,requiredKPI=502,haveFinishWork=503,lastRequiredKPI=504,kpiBouesPossibility=505,kpiBouesMultiplier=506,timeMultiplier=507,preesureResistance=508,kpiMultiplier=509,
        //affinity
        affinity = 601,
        //task
        task = 701,
        //political
        political = 801,
    }

[Serializable]
public class Attribute
{
    public string Name{get {return id.ToString(); }}
    public AttributeID id;
    public float value;
    [HideInInspector]
    public Character owner;
    protected float expectation = 100;

    public AttributeState State { get { return GetState(); } }

    // Parameterless constructor so subclasses can initialize without needing a copy-attribute
    public Attribute()
    {
    }

    // Convenience constructor to create an Attribute with id and value
    public Attribute(AttributeID id, float value, Character owner = null)
    {
        this.id = id;
        this.value = value;
        this.owner = owner;
    }

    public Attribute(Attribute attribute)
    {
        id = attribute.id;
        value = attribute.value;
    }

    public virtual void AddModifier(float modifier)
    {
        float newValue = value + modifier;
        if (newValue >= 0)
        {
            value = newValue;
            EventManager.Instance.MutifyStats();
        }
        else
            value = 0;
    }
    public static Attribute operator +(Attribute a, float b)
    {
        a.AddModifier(b);
        return a;
    }
    public static Attribute operator -(Attribute a, float b)
    {
        a.AddModifier(-b);
        return a;
    }
    public virtual void MultiplyModifier(float modifier)
    {
        float newValue = value * modifier;
        if (newValue >= 0)
        {
            value = newValue;
            EventManager.Instance.MutifyStats();
        }
    }
    public static Attribute operator *(Attribute a, float b)
    {
        a.MultiplyModifier(b);
        return a;
    }
    
    public AttributeState GetState()
    {
        if (value < expectation * 0.3f)
            return AttributeState.low;
        else if (value < expectation * 0.8f)
            return AttributeState.middle;
        else
            return AttributeState.high;
    }

    public void SetExpectation(float value = 100)
    {
        expectation = value;
    }

    public enum AttributeState
    {
        none = 0,
        low,
        middle,
        high,
    }
}

public class AttributeListManager
{
    public List<Attribute> attributes;
    public AttributeListManager(List<Attribute> attributes)
    {
        this.attributes = attributes;
    }
    public Attribute GetAttributeByEnum(AttributeID id)
    {
        Attribute attribute = attributes.Find(v => v.id == id);
        if (attribute == null)
        {
            Debug.LogWarning($"value {id} not found!");
            return null;
        }
        else
            return attribute;
    }
    public void SetAttributeByEnum(AttributeID id, float value)
    {
        Attribute v = GetAttributeByEnum(id);
        if (v == null)
            return;
        else
            v.value = value;
        if(v.owner is Hero)
            Hero.Instance.DisplayStatsValue();
    }
    public void AddByEnum(AttributeID id, float modifier)
    {
        Attribute attribute = GetAttributeByEnum(id);
        if (attribute == null)
            return;
        else
        {
            attribute += modifier;
            if(attribute.owner is Hero)
                Hero.Instance.DisplayStatsValue();
        }   
    }
    public void MultiplyByEnum(AttributeID id, float modifier)
    {
        Attribute v = GetAttributeByEnum(id);
        if (v == null)
            return;
        else
        {
            v *= modifier;
            if(v.owner is Hero)
                Hero.Instance.DisplayStatsValue();
        }  
    }
}
[Serializable]
public class Attitude
{
    public Character from;
    public Character to;

    //价值观契合度，100完全一致，-100完全冲突
    public float ValuesAlignment => ValuesAlignmentBase + valuesAlignmentDelta;
    //基础价值观契合度，取决于双方价值观差异
    public float ValuesAlignmentBase => CaculatValuesAlignment();
    //价值观契合度增量，可以由社交影响，随时间衰减
    public float valuesAlignmentDelta = 0;

    //业务能力印象，最高100，最低-100
    public float CompetenceImpression => CompetenceImpressionBase + competenceImpressionDelta;
    //业务能力印象基础值，取决于工作产出
    public float CompetenceImpressionBase => (Math.Min(to.lifeController.talentListManager.GetAttributeByEnum(AttributeID.work).value/10*100,100) - 50)/50*100;
    //业务能力印象增量，可以由社交影响，随时间衰减
    public float competenceImpressionDelta;

    //利益一致度，最高100，最低-100
    public float InterestsAlignment => interestsAlignmentBase + interestsAlignmentDelta;
    public float interestsAlignmentBase = 0;
    public float interestsAlignmentDelta;
    //态度三维度
    public Vector3 Value => CaculatAttitudeValue();
    public AttitudeType Type => CaculateAttitudeType();

    public Attitude(Character from, Character to)
    {
        this.from = from;
        this.to = to;
        //订阅每日结束事件，进行价值观契合度增量衰减
        EventManager.Instance.OnDayEnd += ValuesAlignmentDeltaDecay;
        //订阅每日结束事件，进行业务能力印象衰减
        EventManager.Instance.OnDayEnd += CompetenceImpressionDecay;
    }
    public enum AttitudeType
    {
        colleague = 0,
        brother,
        competitor,
        ally,
        friend,
        threat,
        minor,
        chatBuddy,
        fuckAway,
    }
    public float CaculatValuesAlignment()
    {
        // 使用欧式距离计算两人的个人价值观差异（personalValues 是 Vector2）
        Vector2 a = from.personalValues;
        Vector2 b = to.personalValues;
        // 价值观的两个维度最大正负50，所以价值观差异最大140，以70为中位数转化为相对值
        return (70 - Vector2.Distance(a, b))/70*100;
    }
    //价值观契合度增量衰减
    public void ValuesAlignmentDeltaDecay()
    {
        if(valuesAlignmentDelta > 0)
        {
            valuesAlignmentDelta -= 3;
        }
        if(valuesAlignmentDelta < 0)
        {
            valuesAlignmentDelta =0;
        }
    }
    //业务能力印象衰减
    public void CompetenceImpressionDecay()
    {
        if(competenceImpressionDelta > 0)
        {
            competenceImpressionDelta -= 3;
        }
        if(competenceImpressionDelta < 0)
        {
            competenceImpressionDelta =0;
        }
    }
    public Vector3 CaculatAttitudeValue()
    {
        float x = CompetenceImpression;
        float y = ValuesAlignment;
        float z = InterestsAlignment;
        return new Vector3(x,y,z);
    }
    public AttitudeType CaculateAttitudeType()
    {
        Vector3 attitudeType = Value;
        float c = attitudeType.x;
        float v = attitudeType.y;
        float i = attitudeType.z;
        if(c >= 30 && v >= 30 && i >= 30)
            return AttitudeType.brother;
        else if(c <= -30 && v >= 30 && i >= 30)
            return AttitudeType.friend;
        else if(c >= 30 && v <= -30 && i >= 30)
            return AttitudeType.ally;
        else if(c >= 30 && v >= 30 && i <= -30)
            return AttitudeType.competitor;
        else if(c >= 30 && v <= -30 && i <= -30)
            return AttitudeType.threat;
        else if(c <= -30 && v >= 30 && i <= -30)
            return AttitudeType.chatBuddy;
        else if(c <= -30 && v <= -30 && i >= 30)
            return AttitudeType.minor;
        else if(c <= -30 && v <= -30 && i <= -30)
            return AttitudeType.fuckAway;
        else
            return AttitudeType.colleague;          
    }
    public void AddValue(Vector3 value)
    {
        competenceImpressionDelta += value.x;
        valuesAlignmentDelta += value.y;
        interestsAlignmentDelta += value.z;
    }
}

public class Topic
{
    public AttributeID theme = 0;
    public TopicType Type => GetTopicType();
    public float completeness;
    public string description;
    public float attitudeValue;
    public float Output => attitudeValue * completeness;
    
    public Topic(Character discoverer,Character target)
    {
        if(target is NPC npc)
        {
            theme = npc.actionManager.CurrentAction.theme;
        }
        else if(target is Hero hero)
        {
            theme = hero.currenAction.theme;
        }
        else
        Debug.LogWarning($"{discoverer} {target} Topic has no theme");
        //在该主题上的数值越大，完整性就高，与目标相等时最大，最大值为1
        completeness = math.min(1,discoverer.lifeController.statListManager.GetAttributeByEnum(theme).value/target.lifeController.statListManager.GetAttributeByEnum(theme).value);
    }
    public enum TopicType
    {
        none = 0,
        life,
        work,
        political,
    }
    public TopicType GetTopicType()
    {
        return theme switch
        {
            AttributeID.coffee or AttributeID.sports or AttributeID.art or AttributeID.gaming or AttributeID.music => TopicType.life,
            AttributeID.work => TopicType.work,
            AttributeID.political => TopicType.political,
            _ => TopicType.none,
        };
    }
}

//对话情况
public class Situation
{
    public Action action;
    public Occasion occasion;
    public Situation(Action action)
    {
        this.action = action;
    }
}
//场合
public class Occasion
{
    
}

public class Conv
{
    public Character starter;
    public Character responder;
    public Situation situation;
    public int speakTimes;
    public Vector3 output;
    public Conv(Character starter,Character responder)
    {
        this.starter = starter;
        this.responder = responder;
        Action action = null;
        if(responder.IsBusy)
        {
            if(responder is NPC npc)
                action = npc.actionManager.CurrentAction;
            else if(responder is Hero hero)
                action = hero.currenAction;
        }
        situation = new(action);
        speakTimes = 1;
    }
    public float CaculateTopicScore(Topic topic)
    {
        return UnityEngine.Random.Range(0,100) ;
    }
    //发掘话题
    public Topic DiscoverNewTopic()
    {
        AttributeID theme;
        if(responder is NPC npc)
        {
            if(npc.IsBusy)
            {
                theme = responder.actionManager.CurrentAction.theme;
            }
            else
                return null;
        }
        else if(responder is Hero hero)
        {
            theme = hero.currenAction.theme;
        }
        else
            theme = 0;
        if(theme == 0)
                return null;
        else
            return new Topic(starter,responder); 
    }
    public Topic SelectTopicNPC()
    {
        AttributeID theme;
        if(situation.action != null)
            theme = situation.action.theme;
        else
            return null;
        Topic topic = starter.socialManager.topics.Find(a => a.theme == theme);
        CaculateTopicOutput(topic);
        return topic; 
    }
    public Topic CompleteTopic(Topic topic)
    {
        topic.completeness += 0.2f;
        if(topic.completeness >1)
            topic.completeness = 1;
        return topic;
    }
    public void IncreaseSpeakTimes()
    {
        speakTimes ++;
    }
    public void DecreaseSpeakTimes()
    {
        speakTimes ++;
        if(speakTimes <= 0)
            CloseConv();
    }
    public void CaculateTopicOutput(Topic topic)
    {
        if(topic == null)
            return;
        switch (topic.Type)
        {
            case Topic.TopicType.life:
                output.y += topic.Output;
                break;
            case Topic.TopicType.work:
                output.x += topic.Output;
                break;
            case Topic.TopicType.political:
                output.x += topic.Output;
                break;
            default:
                break;
        }
    }
    public void CloseConv()
    {
        Attitude attitude = starter.socialManager.attitudes.Find(a => a.to == responder);
        attitude.AddValue(output);
    }
}