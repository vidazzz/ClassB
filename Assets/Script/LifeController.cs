using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Character))]
public class LifeController : MonoBehaviour
{
    [SerializeField]
    private List<Stat> stats = new();
    [SerializeField]
    private List<Need> needs = new();
    private Character character;
    private bool waitForSettleAccountsAffterWork = false;

    public void SettleAccountsPerHour()
    {
    }
    public float GetStatValue(string statsName)
    {
        Stat stat = stats.Find(a => a.name == statsName);
        if (stat == null)
        {
            Debug.LogError($"Character {character.name}'s Stat {statsName} not found!");
            return 0;
        }
        return stat.value;
    }
    public void SetStatValue(string statsName, float value)
    {
        Stat stat = stats.Find(a => a.name == statsName);
        if (stat == null)
        {
            Debug.LogError($"Stat {statsName} not found!");
            return;
        }
        stat.value = value;
        if(character is Hero)
            Hero.Instance.DisplayStatsValue();
    }
    
    public Stat GetStat(string statsName)
    {
        Stat stat = stats.Find(a => a.name == statsName);
        if (stat == null)
        {
            Debug.LogError($"Stat {statsName} not found!");
            return null;
        }
        return stat;
    }
    public Need GetNeed(string needName)
    {
        Need need = needs.Find(a => a.name == needName);
        if (need == null)
        {
            Debug.LogError($"Need {needName} not found!");
            return null;
        }
        return need;
    }
    
    public bool TryMotifyStat(string statsName, float value, StatModifierBuff.ModifierType modifierType = StatModifierBuff.ModifierType.Add)
    {
        switch (modifierType)
        {
            case StatModifierBuff.ModifierType.Add:
                return AddModifier(statsName, value);
            case StatModifierBuff.ModifierType.Multiply:
                MultiplyModifier(statsName, value);
                return true;
            default:
                return AddModifier(statsName, value);
        }
    }
    public bool AddModifier(string statsName, float value)
    {
        if(statsName == "")
            return false;
        Stat stat = stats.Find(a => a.name == statsName);
        if (stat == null)
        {
            Debug.LogError($"Stat {statsName} not found!");
            return false;
        }
        if (statsName.Equals("preesure") && value > 0)
            if (Random.Range(0, 100) < GetStatValue("preesureResistance")) //概率免疫压力
            {
                PopUp.Instance.ShowPopUp("触发压力免疫了");
                return false; //免疫压力
            }
        if (statsName.Equals("kpi") && value > 0)
            if (Random.Range(0, 100) < GetStatValue("kpiBouesPossibility")) //概率提升kip收益
            {
                PopUp.Instance.ShowPopUp("触发额外kpi收益了");
                value *= GetStatValue("kpiBouesMultiplier");
            }
        float newValue = stat.value + value;
        Stat.State originStatState = stat.GetStatState();
        if (newValue >= 0)
        {
            SetStatValue(statsName, newValue);
            if (originStatState != stat.GetStatState())
            {
                //UpdatePriorityNeed(); //状态变化时更新行动优先级
            }
        }
        else
        {
            //PopUp.Instance.ShowPopUp($"{statsName} is not enough!");
            return false; //如果不足则不修改
        }
        Hero.Instance.DisplayStatsValue();
        EventManager.Instance.MutifyStats();
        return true;
    }

    public void MultiplyModifier(string statsName, float value)
    {
        float newValue = GetStatValue(statsName) * value;
        if (newValue >= 0)
        {
            SetStatValue(statsName,newValue);
        }
        Hero.Instance.DisplayStatsValue();
        EventManager.Instance.MutifyStats();
    }
    
    public void SalarySettleAccounts()
    {
        float bonus = 0;
        if (Random.Range(0f, 1f) < (GetStatValue("requiredKPI") - GetStatValue("kpi")) / GetStatValue("kpi")) //发奖金的概率
            bonus = GetStatValue("kpi") * 1f;
        float salary = GetStatValue("kpi") + bonus;
        AddModifier("money", salary);
        SetStatValue("kpi",0); //清kpi
        if (character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with {bonus} Bonus)");
        float newRequiredKPI = GetStatValue("kpi") + (float)Math.Ceiling(GetStatValue("requiredKPI") * Random.Range(0, 0.5f)); //计算新的KPI
        if (GetStatValue("kpi") >= GetStatValue("requiredKPI"))
        {
            SetStatValue("lastRequiredKPI", GetStatValue("requiredKPI")); //数值存档
            SetStatValue("requiredKPI", newRequiredKPI); //如果完成了今天的KPI则KPI门槛上调
            SetStatValue("haveFinishWork", 1);
        }
        Hero.Instance.DisplayStatsValue();
    }
    public void Try_KPI_SettleAccounts()
    {
        float newRequiredKPI = GetStatValue("kpi")+ (float)Math.Ceiling(GetStatValue("requiredKPI")*Random.Range(0,0.5f)); //计算新的KPI
        if(GetStatValue("kpi") >= GetStatValue("requiredKPI"))
        {
            SetStatValue("lastRequiredKPI", GetStatValue("requiredKPI")); //数值存档
            SetStatValue("requiredKPI", newRequiredKPI); //如果完成了今天的KPI则KPI门槛上调
            SetStatValue("haveFinishWork", 1);
        }
        else
            SetStatValue("haveFinishWork", 0);
        Hero.Instance.DisplayStatsValue();
    }
    public void TrySalarySettleAccounts()
    {
        if(GetStatValue("haveFinishWork") == 0)
        {
            waitForSettleAccountsAffterWork = true;
            return;
        }
        waitForSettleAccountsAffterWork = false;
        float bonus = 0;
        if(Random.Range(0f,1f)<(GetStatValue("requiredKPI") - GetStatValue("kpi"))/GetStatValue("kpi")) //发奖金的概率
            bonus = GetStatValue("kpi")*1f;
        float salary = GetStatValue("kpi") + bonus;
        AddModifier("money",salary);
        SetStatValue("kpi", 0); //清kpi
        if(character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with {bonus} Bonus)");
        SetStatValue("haveFinishWork", 0);    
        Hero.Instance.DisplayStatsValue();
    }

    public void TrySalarySettleAccountsAffterWork()
    {
        if(!waitForSettleAccountsAffterWork)
            return;
        float salary = GetStatValue("kpi");
        AddModifier("money",salary);
        SetStatValue("kpi", 0); //清kpi
        if(character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with No Bonus)");
        waitForSettleAccountsAffterWork = false;
        Hero.Instance.DisplayStatsValue();
    }

    public void DisplayStatsValue(TextMeshProUGUI LifeValueText)
    {
        LifeValueText.text = "";
        foreach (var stat in stats)
        {
            LifeValueText.text += $"{stat.name} : {stat.value}\n";
        }
    }

    public void UpdatePriorityNeed()
    {
        if (character is not NPC)
            return;
        NPC npc = character as NPC;
        foreach (Action action in npc.actions)
        {
            action.priorityStat = 0;
        }
        foreach (Action action in npc.actions)
        {
            foreach (Need need in needs)
            {
                //Debug.Log($"Updating action priority for {action.target} based on stat {need.name}");
                Need.PriorityNeed pn = need.priorityNeedList.Find(a => a.action == action);
                if (pn != null)
                    action.priorityStat += pn.Priority;
            }
        }
    }

    public void InitializeStats()
    {
        stats = new()
        {
            new("preesure",25), //压力值
            new("preesureResistance", 0), //压力抗性，概率不会产生压力
            new("money", 1000),
            new("kpi", 10),
            new("kpiBouesPossibility", 0),//概率提升收益
            new("kpiBouesMultiplier", 1),//概率提升收益
            new("requiredKPI", 10),
            new("lastRequiredKPI", 0),//上一次的kpi指标
            new("kpiMultiplier", 1),//加buff时生效的kpi倍率
            new("salary", 50),
            new("timeMultiplier", 1),//时间感，每秒时间乘数
            new("haveFinishWork", 0),//结算时是否完成了当天工作
        };
        needs = new()
        {
            new(character, "energy", 100),
            new(character, "SocialNeed", 100),//社交需求
            new(character, "Happiness", 100),//幸福感
            new(character, "Toilet", 100,2),
        };
        UpdatePriorityNeed();//初始化priorityStat
    }
    
    void Awake()
    {
        character = GetComponent<Character>();

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
