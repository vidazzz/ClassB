using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DialogueOptionNode : Node
{
	public string line;
	[Input] public int nodeIndex; //输入端口，连接上一段的输出端口
	[Output] public int succeededNode;
	[Output] public int failedNode;
	public string checkingTalentName;
	public int checkingTalentLevel;
	public EffectType succedEffectType;
	public List<string> succedArgs;
	public EffectType failedEffectType;
	public List<string> failedArgs;


	public DialogueNode BelongedNode
	{
		get
		{
			return GetInputPort("nodeIndex").Connection?.node as DialogueNode;
		}
	}//所属对话节点

	[HideInInspector]
	public DialogueNode nextNode;
	public DialogueGraph nextGraph; //对话图
	//[HideInInspector]
	public bool hasChecked = false;


	public bool checkResult = true; //如果不检定，默认使用成功结果

	public IEnumerator OptionEffect(DialogueController dialogueController)
	{
		EffectType type;
		List<string> args;
		if (checkingTalentName != "" && !hasChecked) //填了checkingTalentName且没有检定过就进行检定
		{
			checkResult = DiceCheck.Instance.CheckTalent(checkingTalentName, checkingTalentLevel ,Hero.Instance);
			hasChecked = true;
		}
		if (checkResult) //根据检定结果决定采用的效果和跳转索引
		{
			type = succedEffectType;
			args = succedArgs;
			nextNode = GetOutputPort("succeededNode").Connection?.node as DialogueNode;
		}
		else
		{
			type = failedEffectType;
			args = failedArgs;
			nextNode = GetOutputPort("failedNode").Connection?.node as DialogueNode;
		}

		// 根据配置名称选择要执行的协程
		if (!hasChecked)
		{
			switch (type)
			{
				case EffectType.ModifyStats:
					yield return EffectModifyParameter(args);
					break;
				case EffectType.ModifyAfinity:
					yield return EffectModifyAffinity(args, dialogueController.character);
					break;
				case EffectType.CheckItem:
					yield return EffectCheckItem(args);
					break;
				case EffectType.Check:
					yield return EffectCheck(args, dialogueController.character);
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
		}
		if(nextGraph != null) //优先进入对话图
			yield return dialogueController.GraphDisplayDialogue(nextGraph);
		else
			dialogueController.currentDialogueNode = nextNode; //下一句
	}

	//调整属性
	public IEnumerator EffectModifyParameter(List<string> strings)
	{
		Hero.Instance.lifeController.AddModifier(strings[0], Convert.ToInt32(strings[1]));
		yield return null;
	}

	//调整好感度
	public IEnumerator EffectModifyAffinity(List<string> strings, Character target)
	{
		Community.affinity.ModifyAffinity(Hero.Instance, target, Convert.ToInt32(strings[0]));
		Community.PrintAffinity();
		yield return null;
	}
	public void RollbackRequiredKPI()
	{
		Hero.Instance.lifeController.SetStatValue("requiredKPI", Hero.Instance.lifeController.GetStatValue("lastRequiredKPI"));
		Hero.Instance.DisplayStatsValue();
	}
	public IEnumerator EffectCheckItem(List<string> strings)
	{
		yield return null;
	}
	public IEnumerator EffectCheck(List<string> args,Character owner)
	{
		Check check = new(args, owner,Hero.Instance);
		CheckPanel.instance.Initialize(check);
		CheckPanel.ToggleUI();
		while (CheckPanel.instance.gameObject.activeSelf)
		{
			yield return null;
		}
	}
	public IEnumerator EffectOffWork()
	{
		yield return null;
	}
	
	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) {
		return port?.Connection?.node; // Replace this
	}
}