using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public enum PreesureLevel {
    none = 0,
    low,
    normal,
    high,
    varyHigh,
    overCome,
}

public class LifeController : MonoBehaviour
{
    public Dictionary<string,float> statsPairs;
    public Dictionary<string,float> deltaStatsPairs;//记录参数的变化值
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
    public void SettleAccountsPerDay()
    {
        SalarySettleAccounts();
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
        deltaStatsPairs[statsName] = value;
        Hero.Instance.DisplayLifeValue();
    }

    public void MultiplyModifier(string statsName,float value)
    {
        if(statsPairs[statsName] * value >= 0)
        {
            statsPairs[statsName] *= value;
            deltaStatsPairs[statsName] = value;
        }
        Hero.Instance.DisplayLifeValue();
    }
    void SalarySettleAccounts()
    {
        float bonus = 0;
        if(Random.Range(0f,1f)<(statsPairs["requiredKPI"] - statsPairs["kpi"])/statsPairs["kpi"]) //发奖金的概率
            bonus = statsPairs["kpi"]*1f;
        float salary = statsPairs["kpi"] + bonus;
        AddModifier("money",salary);
        statsPairs["kpi"] = 0; //清kpi
        statsPairs["requiredKPI"] += (float)Math.Ceiling(statsPairs["requiredKPI"]*Random.Range(0,0.5f)); //KPI门槛上调
    }

    public bool Check(string statsName,float value)
    {
        bool result = value >= statsPairs[statsName];
        return result;
    }
    void Awake()
    {
        statsPairs = new()
        {      
            {"preesure",25},
            {"preesureResistance",0}, //压力抗性，概率不会产生压力
            {"money",99999999},
            {"kpi",0},
            {"kpiBouesPossibility",0},//概率提升收益
            {"kpiBouesMultiplier",1},//概率提升收益
            {"requiredKPI",5},
            {"kpiMultiplier",1},//加buff时生效的kpi倍率
            {"energy",100},
            {"salary",50},
            {"energyPerHour",-1},
            {"timeMultiplier",1},//时间感，每秒时间乘数
        };

        deltaStatsPairs = new()
        {
            {"preesure",0},
            {"preesureResistance",0},
            {"money",0},
            {"kpi",0},
            {"kpiBouesPossibility",0},
            {"kpiBouesMultiplier",0},
            {"requiredKPI",0},
            {"kpiMultiplier",0},
            {"energy",0},
            {"salary",0},
            {"energyPerHour",0},
            {"timeMultiplier",0},
        };

        Timer.onHourEnd += SettleAccountsPerHour;
        Timer.onDayEnd += SettleAccountsPerDay;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }
}
