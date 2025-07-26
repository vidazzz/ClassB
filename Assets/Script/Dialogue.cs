using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;


[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject {
    public LineCheckingType lineCheckingType;
    public string checkingName;//talent name or stats name
    public int ckeckingValue;//talent level or stats value
    public int seccuedIndex;
    public int failedIndex;
    public bool hasChecked;
    private int _firstLineIndex = 1;
    public int FirstLineIndex{
        get{
            if(checkingName == "") //不填检定天赋名则从第一句开始
                return 1;
            else if(!hasChecked) //只做一次检定
            {
                switch(lineCheckingType)
                {
                    case LineCheckingType.Stats:
                        _firstLineIndex = DialogueStatsCheck(); //保存检定结果
                        break;
                    case LineCheckingType.Skill:
                        _firstLineIndex = DialogueDiceCheck();
                        break;
                    default:
                        _firstLineIndex = DialogueDiceCheck();
                        break;
                }
                hasChecked = true;
                return _firstLineIndex;
            }
            else
                return _firstLineIndex;         
        }
    }
    public List<LineNode> dialogue;

    private int DialogueDiceCheck()
    { 
        if(DiceCheck.Instance.Check(Hero.Instance.GetTalent(checkingName), ckeckingValue))
            return seccuedIndex;
        else
            return failedIndex;
    }

    private int DialogueStatsCheck()
    {
        if(Hero.Instance.lifeController.Check(checkingName,ckeckingValue))
            return seccuedIndex;
        else
            return failedIndex;
    }

    public void Reset()
    {
        hasChecked = false;
        foreach (var line in dialogue)
        {
            foreach(var option in line.options)
            {
                option.hasChecked = false;
            }
        }
    }
}


[Serializable]
public class LineNode
{
    [SerializeField]
    private string line;
    public string Line{
        get {
                if(formatArgNames.Length > 0)
                {
                    string[] formatArgs = new string[formatArgNames.Length];
                    for ( int i = 0 ; i < formatArgNames.Length ; i ++)
                    {
                        formatArgs[i] = Hero.Instance.lifeController.statsPairs[formatArgNames[i]].ToString();
                        Debug.Log(formatArgs[i]);
                    }
                    return string.Format(line,formatArgs);
                }
                else
                    return line;
            }
        }
    public string[] formatArgNames;
    public int speekerIndex = 0; //0是本人
    public int nextLine{
        get {
            if(checkingStatsName == "")
                return seccuedIndex;
            if(Hero.Instance.lifeController.Check(checkingStatsName,checkingValue))
                return seccuedIndex;
            else
                return failedIndex;
        }
    }//非选项跳转时跳转至哪句话，1base，0是按照索引找下一句
    public string checkingStatsName;
    public int checkingValue;
    public int seccuedIndex;
    public int failedIndex;
    public List<DialogueOption> options;

}

public enum LineCheckingType
{
    none = 0,
    Stats,
    Skill,
}

[Serializable]
public enum EffectType
{
    None = 0,
    ModifyStats,
    ModifyAfinity,
    CheckItem,
    OffWork,
    RollBack,
}

[Serializable]
public class DialogueOption //: MonoBehaviour
{
    public string line;
    public string checkingTalentName;
    public int checkingTalentLevel;
    public EffectType effectType;
    public EffectType failedEffectType;
    public int succedJumpToLine; //1base，0表示跳转索引顺序的下一句，如果要检定注意不要填0
    public int failedJumpToLine; //1base，0表示跳转索引顺序的下一句，如果要检定注意不要填0
    [HideInInspector]
    public int jumpToLine; //1base，最终跳转索引
    //[HideInInspector]
    public bool hasChecked = false;
    public List<string> strings;
    public List<string> failedStrings;
    private bool checkResult = true; //如果不检定，默认使用成功结果

    public IEnumerator OptionEffect(DialogueController dialogueController)
    {
        EffectType type;     
        checkResult = true;//如果不检定，默认使用成功结果
        if(checkingTalentName != "") //填了checkingSkillName就进行检定
        {
            checkResult = DiceCheck.Instance.Check(Hero.Instance.GetTalent(checkingTalentName),checkingTalentLevel);
            hasChecked = true;
        }
        if(checkResult) //根据检定结果决定采用的效果和跳转索引
        {
            type = effectType;
            jumpToLine = succedJumpToLine;
        }
        else
        {
            type = failedEffectType;
            jumpToLine = failedJumpToLine;
        }
            
        // 根据配置名称选择要执行的协程
        switch (type)
        {
            case EffectType.ModifyStats:
                yield return EffectModifyParameter(strings);
                break;
            case EffectType.ModifyAfinity:
                yield return EffectModifyAffinity(strings,dialogueController.character);
                break;
            case EffectType.CheckItem:
                yield return EffectCheckItem(strings);
                break;
            case EffectType.RollBack:
                RollbackRequiredKPI();
                break;
            case EffectType.OffWork:
                Timer.shouldOffWorkNow = true;
                break;
            // 可以添加更多协程类型...
            default:
                break;
        }
        Debug.Log(jumpToLine);
        if(jumpToLine > 0) //1base，选项跳转至索引jump-1 句
            yield return EffectJumpToLine(dialogueController,jumpToLine - 1);
        else //如果填0就跳转下一句
            yield return dialogueController.DisplayNextLine();
    }
    public IEnumerator EffectJumpToLine(DialogueController dialogueController,int lineIndex)
    {
        dialogueController.currentLineIndex = lineIndex;
        yield return dialogueController.DisplayDialogue();
    }

    //调整属性
    public IEnumerator EffectModifyParameter(List<string> strings)
    {
        Hero.Instance.lifeController.AddModifier(strings[0],Convert.ToInt32(strings[1]));
        yield return null;
    }

    //调整好感度
    public IEnumerator EffectModifyAffinity(List<string> strings,Character target)
    {
        Community.affinity.ModifyAffinity(Hero.Instance,target,Convert.ToInt32(strings[0]));
        Community.PrintAffinity();
        yield return null;
    }
    public void RollbackRequiredKPI()
    {
        Hero.Instance.lifeController.statsPairs["requiredKPI"] = Hero.Instance.lifeController.statsPairs["lastRequiredKPI"];
        Hero.Instance.DisplayStatsValue();
    }
    public IEnumerator EffectCheckItem(List<string> strings)
    {
        yield return null;
    }
    public IEnumerator EffectOffWork()
    {
        yield return null;
    }
}

