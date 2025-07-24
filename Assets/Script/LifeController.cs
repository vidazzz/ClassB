using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class LifeController : MonoBehaviour
{
    public Dictionary<string,float> statsPairs;
    private Character character;
    private bool waitForSettleAccountsAffterWork = false;
    public float KpiMultiplier{
        get {return statsPairs["kpiMultiplier"];}
    }
    public float TimeMultiplier{
        get {return statsPairs["timeMultiplier"];}
    }
    public PreesureLevel PreesureLevel{
        get {
            if(statsPairs["preesure"] <= 10)
                return PreesureLevel.low;
            else if(statsPairs["preesure"] <= 50)
                return PreesureLevel.normal;
            else if(statsPairs["preesure"] <= 60)
                return PreesureLevel.high;
            else if(statsPairs["preesure"] <= 70)
                return PreesureLevel.varyHigh;
            else
                return PreesureLevel.overCome;
            }
    }

    public void SettleAccountsPerHour()
    {
        AddModifier("energy",statsPairs["energyPerHour"]);
    }

    public void SettleAccountsImmediately()
    {
        
    }
    public void AddModifier(string statsName,float value)
    {
        if(statsName.Equals("preesure") && value > 0)
            if(Random.Range(0,100) < statsPairs["preesureResistance"]) //概率免疫压力
                {
                    Debug.Log("触发压力免疫了");
                    return;
                }
        if(statsName.Equals("kpi") && value > 0)
            if(Random.Range(0,100) < statsPairs["kpiBouesPossibility"]) //概率提升kip收益
                {
                    Debug.Log("触发额外kpi收益了");
                    value *= statsPairs["kpiBouesMultiplier"];
                }
        if(statsPairs[statsName] + value >= 0)
            statsPairs[statsName] += value;
        else
            Debug.Log($"{statsName} is not enough!");
        //deltaStatsPairs[statsName] = value;
        Hero.Instance.DisplayStatsValue();
    }

    public void MultiplyModifier(string statsName,float value)
    {
        if(statsPairs[statsName] * value >= 0)
        {
            statsPairs[statsName] *= value;
            //deltaStatsPairs[statsName] = value;
        }
        Hero.Instance.DisplayStatsValue();
    }
    public void SalarySettleAccounts()
    {
        float bonus = 0;
        if(Random.Range(0f,1f)<(statsPairs["requiredKPI"] - statsPairs["kpi"])/statsPairs["kpi"]) //发奖金的概率
            bonus = statsPairs["kpi"]*1f;
        float salary = statsPairs["kpi"] + bonus;
        AddModifier("money",salary);
        statsPairs["kpi"] = 0; //清kpi
        if(character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with {bonus} Bonus)");
        float newRequiredKPI = statsPairs["kpi"]+ (float)Math.Ceiling(statsPairs["requiredKPI"]*Random.Range(0,0.5f)); //计算新的KPI
        if(statsPairs["kpi"] >= statsPairs["requiredKPI"])
        {
            statsPairs["lastRequiredKPI"] = statsPairs["requiredKPI"]; //数值存档
            statsPairs["requiredKPI"] = newRequiredKPI; //如果完成了今天的KPI则KPI门槛上调
            statsPairs["haveFinishWork"] = 1;
        }  
        Hero.Instance.DisplayStatsValue();
    }
    public void Try_KPI_SettleAccounts()
    {
        float newRequiredKPI = statsPairs["kpi"]+ (float)Math.Ceiling(statsPairs["requiredKPI"]*Random.Range(0,0.5f)); //计算新的KPI
        if(statsPairs["kpi"] >= statsPairs["requiredKPI"])
        {
            statsPairs["lastRequiredKPI"] = statsPairs["requiredKPI"]; //数值存档
            statsPairs["requiredKPI"] = newRequiredKPI; //如果完成了今天的KPI则KPI门槛上调
            statsPairs["haveFinishWork"] = 1;
        }
        else
            statsPairs["haveFinishWork"] = 0;
        Hero.Instance.DisplayStatsValue();
    }
    public void TrySalarySettleAccounts()
    {
        if(statsPairs["haveFinishWork"] == 0)
        {
            waitForSettleAccountsAffterWork = true;
            return;
        }
        waitForSettleAccountsAffterWork = false;
        float bonus = 0;
        if(Random.Range(0f,1f)<(statsPairs["requiredKPI"] - statsPairs["kpi"])/statsPairs["kpi"]) //发奖金的概率
            bonus = statsPairs["kpi"]*1f;
        float salary = statsPairs["kpi"] + bonus;
        AddModifier("money",salary);
        statsPairs["kpi"] = 0; //清kpi
        if(character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with {bonus} Bonus)");
        statsPairs["haveFinishWork"] = 0;    
        Hero.Instance.DisplayStatsValue();
    }

    public void TrySalarySettleAccountsAffterWork()
    {
        if(!waitForSettleAccountsAffterWork)
            return;
        float salary = statsPairs["kpi"];
        AddModifier("money",salary);
        statsPairs["kpi"] = 0; //清kpi
        if(character is Hero)
            PopUp.Instance.ShowPopUp($"Get Salary: {salary} (with No Bonus)");
        waitForSettleAccountsAffterWork = false;
        Hero.Instance.DisplayStatsValue();
    }

    public bool Check(string statsName,float value)
    {
        bool result = statsPairs[statsName] >= value;
        return result;
    }
    void Awake()
    {
        character = GetComponent<Character>();
        statsPairs = new()
        {      
            {"preesure",25},
            {"preesureResistance",0}, //压力抗性，概率不会产生压力
            {"money",99999999},
            {"kpi",10},
            {"kpiBouesPossibility",0},//概率提升收益
            {"kpiBouesMultiplier",1},//概率提升收益
            {"requiredKPI",5},
            {"lastRequiredKPI",0},//上一次的kpi指标
            {"kpiMultiplier",1},//加buff时生效的kpi倍率
            {"energy",100},
            {"salary",50},
            {"energyPerHour",-1},
            {"timeMultiplier",1},//时间感，每秒时间乘数
            {"haveFinishWork",0},//结算时是否完成了当天工作
        };

        Timer.onHourEnd += SettleAccountsPerHour;
        Timer.onDayEnd += SalarySettleAccounts;
    }

    void Start()
    {

    }
    void OnDestroy()
    {   
        Timer.onHourEnd -= SettleAccountsPerHour;
        Timer.onDayEnd -= SalarySettleAccounts;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
