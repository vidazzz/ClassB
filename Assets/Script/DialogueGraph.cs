using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu]
public class DialogueGraph : NodeGraph
{
    public AttributeID checkingID;//talent name or stats name
    public int ckeckingValue;//talent level or stats value
    public DialogueNode trueNode;
    public DialogueNode falseNode;
    public bool hasChecked;
    private DialogueNode startNode;
    public DialogueNode StartNode{
        get{
            if(checkingID == 0) //不填则采用成功节点
                return trueNode;
            else if(!hasChecked) //只做一次检定
            {
                startNode = DialogueDiceCheck();
                hasChecked = true;
                return startNode;
            }
            else
                return startNode;         
        }
    }

    private DialogueNode DialogueDiceCheck()
    { 
        if(DiceCheck.Instance.CheckAttribute(checkingID, ckeckingValue,Hero.Instance))
            return trueNode;
        else
            return falseNode;
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