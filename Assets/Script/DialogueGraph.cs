using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu]
public class DialogueGraph : NodeGraph
{
    public LineCheckingType lineCheckingType;
    public AttributeID checkingID;//talent name or stats name
    public int ckeckingValue;//talent level or stats value
    public DialogueNode succeededNode;
    public DialogueNode failedNode;
    public bool hasChecked;
    private DialogueNode startNode;
    public DialogueNode StartNode{
        get{
            if(checkingID == 0) //不填则采用成功节点
                return succeededNode;
            else if(!hasChecked) //只做一次检定
            {
                switch(lineCheckingType)
                {
                    case LineCheckingType.Stats:
                        startNode = DialogueStatsCheck(); //保存检定结果
                        break;
                    case LineCheckingType.Skill:
                        startNode = DialogueDiceCheck();
                        break;
                    default:
                        startNode = DialogueDiceCheck();
                        break;
                }
                hasChecked = true;
                return startNode;
            }
            else
                return startNode;         
        }
    }

    private DialogueNode DialogueDiceCheck()
    { 
        if(DiceCheck.Instance.CheckTalent(checkingID, ckeckingValue,Hero.Instance))
            return succeededNode;
        else
            return failedNode;
    }

    private DialogueNode DialogueStatsCheck()
    {
        if(DiceCheck.Instance.CheckStats(checkingID,ckeckingValue,Hero.Instance))
            return succeededNode;
        else
            return failedNode;
    }

    public void Reset()
    {
        hasChecked = false;
        foreach (DialogueNode node in nodes)
        {
            for(int i = 0; i < node.optionNodes.Length; i++)
            {
                DialogueOptionNode option = node.GetOutputPort($"optionNodes {i}").Connection.node as DialogueOptionNode;
                option.hasChecked = false;
            }
        }
    }
}