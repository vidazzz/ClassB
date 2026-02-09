using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Sources;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Character))]
public class LifeController : MonoBehaviour
{
    public AttributeListManager statListManager;
    public AttributeListManager needListManager;
    public AttributeListManager talentListManager;
    public AttributeListManager hobbyListManager;
    [SerializeField]
    private List<Stat> statList = new();
    public List<Need> needList = new();
    public List<Talent> talentList = new();
    public List<Hobby> hobbyList = new();
    private int expToNextLevel = 100;
    private Character character;
    private bool waitForSettleAccountsAffterWork = false;

    public void SettleAccountsPerHour()
    {
    }
    

    public void MultiplyModifier(AttributeID id, float value)
    {
        float newValue = statListManager.GetAttributeByEnum(id).value * value;
        if (newValue >= 0)
        {
            statListManager.SetAttributeByEnum(id,newValue);
        }
        Hero.Instance.DisplayStatsValue();
        EventManager.Instance.MutifyStats();
    }

    public void GainExp(float exp)
    {
        statListManager.AddByEnum(AttributeID.exp, exp);
        CheckLevelUp();
    }
    
    // 检查是否升级
    private void CheckLevelUp()
    {
        // 若当前经验超过下一级所需，触发升级
        if (statListManager.GetAttributeByEnum(AttributeID.exp).value >= expToNextLevel)
        {
            statListManager.AddByEnum(AttributeID.level, 1);
            statListManager.SetAttributeByEnum(AttributeID.exp, 0);
            statListManager.AddByEnum(AttributeID.point, 1);
            EventManager.Instance.LevelUp(); // 触发升级事件
        }
    }

    public void UsePoint(Talent talent)
    {
        talent.value++;
        talentListManager.AddByEnum(AttributeID.point, -1);
    }
    
    public void SalarySettleAccounts()
    {
        float bonus = 0;
        if (Random.Range(0f, 1f) < (statListManager.GetAttributeByEnum(AttributeID.requiredKPI).value - statListManager.GetAttributeByEnum(AttributeID.kpi).value) / statListManager.GetAttributeByEnum(AttributeID.kpi).value) //发奖金的概率
            bonus = statListManager.GetAttributeByEnum(AttributeID.kpi).value * 1f;
        float salary = statListManager.GetAttributeByEnum(AttributeID.kpi).value + bonus;
        statListManager.AddByEnum(AttributeID.money, salary);
        statListManager.SetAttributeByEnum(AttributeID.kpi, 0); //清kpi
        if (character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with {bonus} Bonus)");
        float newRequiredKPI = statListManager.GetAttributeByEnum(AttributeID.kpi).value + (float)Math.Ceiling(statListManager.GetAttributeByEnum(AttributeID.requiredKPI).value * Random.Range(0, 0.5f)); //计算新的KPI
        if (statListManager.GetAttributeByEnum(AttributeID.kpi).value >= statListManager.GetAttributeByEnum(AttributeID.requiredKPI).value)
        {
            statListManager.SetAttributeByEnum(AttributeID.lastRequiredKPI, statListManager.GetAttributeByEnum(AttributeID.requiredKPI).value); //数值存档
            statListManager.SetAttributeByEnum(AttributeID.requiredKPI, newRequiredKPI); //如果完成了今天的KPI则KPI门槛上调
            statListManager.SetAttributeByEnum(AttributeID.haveFinishWork, 1);
        }
        Hero.Instance.DisplayStatsValue();
    }
    public void Try_KPI_SettleAccounts()
    {
        float newRequiredKPI = statListManager.GetAttributeByEnum(AttributeID.kpi).value+ (float)Math.Ceiling(statListManager.GetAttributeByEnum(AttributeID.requiredKPI).value*Random.Range(0,0.5f)); //计算新的KPI
        if(statListManager.GetAttributeByEnum(AttributeID.kpi).value >= statListManager.GetAttributeByEnum(AttributeID.requiredKPI).value)
        {
            statListManager.SetAttributeByEnum(AttributeID.lastRequiredKPI, statListManager.GetAttributeByEnum(AttributeID.requiredKPI).value); //数值存档
            statListManager.SetAttributeByEnum(AttributeID.requiredKPI, newRequiredKPI); //如果完成了今天的KPI则KPI门槛上调
            statListManager.SetAttributeByEnum(AttributeID.haveFinishWork, 1);
        }
        else
            statListManager.SetAttributeByEnum(AttributeID.haveFinishWork, 0);
        Hero.Instance.DisplayStatsValue();
    }
    public void TrySalarySettleAccounts()
    {
        if(statListManager.GetAttributeByEnum(AttributeID.haveFinishWork).value == 0)
        {
            waitForSettleAccountsAffterWork = true;
            return;
        }
        waitForSettleAccountsAffterWork = false;
        float bonus = 0;
        if(Random.Range(0f,1f)<(statListManager.GetAttributeByEnum(AttributeID.requiredKPI).value - statListManager.GetAttributeByEnum(AttributeID.kpi).value)/statListManager.GetAttributeByEnum(AttributeID.kpi).value) //发奖金的概率
            bonus = statListManager.GetAttributeByEnum(AttributeID.kpi).value*1f;
        float salary = statListManager.GetAttributeByEnum(AttributeID.kpi).value + bonus;
        statListManager.AddByEnum(AttributeID.money,salary);
        statListManager.SetAttributeByEnum(AttributeID.kpi, 0); //清kpi
        if(character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with {bonus} Bonus)");
        statListManager.SetAttributeByEnum(AttributeID.haveFinishWork, 0);    
        Hero.Instance.DisplayStatsValue();
    }

    public void TrySalarySettleAccountsAffterWork()
    {
        if(!waitForSettleAccountsAffterWork)
            return;
        float salary = statListManager.GetAttributeByEnum(AttributeID.kpi).value;
        statListManager.AddByEnum(AttributeID.money,salary);
        statListManager.SetAttributeByEnum(AttributeID.kpi, 0); //清kpi
        if(character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with No Bonus)");
        waitForSettleAccountsAffterWork = false;
        Hero.Instance.DisplayStatsValue();
    }

    //尝试根据给出的数值名从属性、天赋、爱好和好感度中查找数值
    public int TryGetValueByEnum(AttributeID id, out Type type,Character other = null)
    {
        object obj = TryGetValueObj(id);
        if(obj == null)
        {
            type = null;
            return 0;
        }
        type = obj.GetType();
        float result = 0;
        switch (obj)
        {
            case Attribute:
                result =(obj as Attribute).value;
                Debug.Log($"id:{id} is {result}");
                break;
            case Community.Affinity affinity:
                result = affinity.GetAffinity(character, other);
                break;
            default:
                Debug.LogWarning($"invalid AttributeID type: {obj}!");
                break;
        }
        return (int)result;
    }
    public object TryGetValueObj(AttributeID id)
    {
        object obj;
        obj = statListManager.GetAttributeByEnum(id);
        obj ??= talentListManager.GetAttributeByEnum(id);
        obj ??= hobbyListManager.GetAttributeByEnum(id);
        obj ??= needListManager.GetAttributeByEnum(id);
        if(obj == null)
        {
            if(id == AttributeID.task)
                obj = character.workManager.CurrentTask;
        }
        if (obj == null)
        {
            if (id == AttributeID.affinity)
                obj = Community.affinity;
        }
        if (obj == null)
            Debug.LogWarning($"can't find AttributeID:{id}!");
        return obj;
    }

    //修改数值
    public void AddAttributeByID(AttributeID id,float value)
    {
        switch (id)
        {
            case AttributeID.sleep:
            case AttributeID.eat:
            case AttributeID.fun:
            case AttributeID.toilet:
            case AttributeID.social:
                needListManager.AddByEnum(id,value);
                break;
            case AttributeID.level:
            case AttributeID.exp:
            case AttributeID.money:
                statListManager.AddByEnum(id,value);
                break;
            case AttributeID.art:
            case AttributeID.coffee:
            case AttributeID.sports:
            case AttributeID.gaming:
            case AttributeID.reading:
            case AttributeID.music:
                hobbyListManager.AddByEnum(id,value);
                break;
            case AttributeID.act:
            case AttributeID.speak:
            case AttributeID.see:
            case AttributeID.work:
                talentListManager.AddByEnum(id,value);
                break;
            default:
                Debug.LogError($"invalid checkObj type!{id}");
                break;
        }
    }


    public void DisplayStatsValue(TextMeshProUGUI LifeValueText)
    {
        LifeValueText.text = "";
        foreach (var stat in statList)
        {
            LifeValueText.text += $"{stat.Name} : {stat.value}\n";
        }
    }

    public void InitializeValues()
    {
        statList = new()
        {
            new(AttributeID.level,1),//等级
            new(AttributeID.exp,0),//经验值
            new(AttributeID.money, 1000),//金钱
        };
        statListManager = new AttributeListManager(statList.Cast<Attribute>().ToList());

        needList = new()
        {
            new(AttributeID.sleep, 80, character,Random.Range(5,20)),
            new(AttributeID.eat, 80, character,Random.Range(5,10)),
            new(AttributeID.social, 80, character,Random.Range(5,20)),//社交需求
            new(AttributeID.fun, 80, character,Random.Range(5,20)),
            new(AttributeID.toilet, 80, character,Random.Range(5,20))
        };
        needListManager = new AttributeListManager(needList.Cast<Attribute>().ToList());

        talentList = new()
        {
            new(AttributeID.see,Random.Range(10,30),character),
            new(AttributeID.speak,Random.Range(10,30),character),
            new(AttributeID.act,Random.Range(10,30),character),
            new(AttributeID.work,Random.Range(10,30),character),
        };
        talentListManager = new AttributeListManager(talentList.Cast<Attribute>().ToList());
        
        foreach(var hobby in hobbyList)
        {
            hobby.owner = character;
            hobby.SetExpectation();
        }
        hobbyListManager = new AttributeListManager(hobbyList.Cast<Attribute>().ToList());
    }

    
    void Awake()
    {
        character = GetComponent<Character>();
        InitializeValues();
        //订阅事件
        EventManager.Instance.OnHourEnd += SettleAccountsPerHour;
        EventManager.Instance.OnDayEnd += SalarySettleAccounts;
    }

    void Start()
    {

    }
    void OnDestroy()
    {   
        //EventManager.Instance.OnHourEnd -= SettleAccountsPerHour;
        //EventManager.Instance.OnDayEnd -= SalarySettleAccounts;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
