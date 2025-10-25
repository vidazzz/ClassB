using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentSkill
{
    public string name;
    string effectName;
    Talent talent;
    int level;
    int[] cost;
    Intelligence intelligence;
    // 定义字典：参数（如字符串/枚举）对应功能委托
    private Dictionary<string, Action<Check>> functionDict = new();
    public TalentSkill(string name, string effectName, Talent talent, int level, int[] cost)
    {
        this.name = name;
        this.effectName = effectName;
        this.talent = talent;
        this.level = level;
        this.cost = cost;
        DictInitiallize();
    }
    public void DictInitiallize()
    {
        // 初始化映射关系
        functionDict.Add("AddDice", AddDice);
        functionDict.Add("AddPoint", AddPoint);
        functionDict.Add("ReduceTime", ReduceTime);
        // 可动态添加新功能
    }

    public void Execute(Check check)
    {
        if (functionDict.TryGetValue(effectName, out Action<Check> action))
        {
            action.Invoke(check); // 执行对应的功能
        }
        else
        {
            Debug.Log("功能不存在：" + effectName);
        }
    }

    public void LevelUp(int point)
    {
        level += point;
    }

    public void AddDice(Check check)
    {

    }

    public void AddPoint(Check check)
    {
        check.deltaValue += 10 * level;
    }

    public void ReduceTime(Check check)
    {

    }

    public void SaveProsess()
    {

    }

    public void ReduceTargetPoint()
    {

    }

    public void CridicalHit()
    {

    } 


}
