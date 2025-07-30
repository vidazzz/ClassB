using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DialogueOptionNode : Node
{
	public string line;
	public string checkingTalentName;
	public int checkingTalentLevel;
	public EffectType succedEffectType;
	public List<string> succedArgs;
	public EffectType failedEffectType;
	public List<string> failedArgs;
	[Input] public int nodeIndex; //输入端口，连接上一段的输出端口
	[Output] public int succeededNode;
	[Output] public int failedNode;

	[HideInInspector]
	public DialogueNode jumpToNode;
	//[HideInInspector]
	public bool hasChecked = false;


	private bool checkResult = true; //如果不检定，默认使用成功结果

	public IEnumerator OptionEffect(DialogueController dialogueController)
	{
		EffectType type;
		List<string> args;
		checkResult = true;//如果不检定，默认使用成功结果
		if (checkingTalentName != "") //填了checkingSkillName就进行检定
		{
			checkResult = DiceCheck.Instance.Check(Hero.Instance.GetTalent(checkingTalentName), checkingTalentLevel);
			hasChecked = true;
		}
		if (checkResult) //根据检定结果决定采用的效果和跳转索引
		{
			type = succedEffectType;
			args = succedArgs;
			jumpToNode = GetOutputPort("succeededNode").Connection.node as DialogueNode;
		}
		else
		{
			type = failedEffectType;
			args = failedArgs;
			jumpToNode = GetOutputPort("failedNode").Connection.node as DialogueNode;
		}

		// 根据配置名称选择要执行的协程
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
		yield return dialogueController.currentDialogueNode = jumpToNode;
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
	
	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) {
		return port?.Connection?.node; // Replace this
	}
}