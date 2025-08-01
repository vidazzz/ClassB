using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DialogueNode : Node {
	[SerializeField]
    private string line;
    public string Line
	{
		get
		{
			if (formatArgNames.Length > 0)
			{
				string[] formatArgs = new string[formatArgNames.Length];
				for (int i = 0; i < formatArgNames.Length; i++)
				{
					formatArgs[i] = Hero.Instance.lifeController.statsPairs[formatArgNames[i]].ToString();
				}
				return string.Format(line, formatArgs);
			}
			else
				return line;
		}
	}
	[Input] public int nodeIndex; //输入端口，连接上一段的输出端口
	[Output] public int succeededNode;
	[Output] public int failedNode; 
	[Output(dynamicPortList = true)] public string[] optionNodes; //选项端口，连接选项节点的输出端口

    public string checkingStatsName;
    public int checkingValue;
	public DialogueGraph nextGraph; //对话图
	private DialogueNode nextNode; 
	public DialogueNode NextNode //下一段节点
	{
		get
		{
			if (nextNode != null)
				return nextNode;
			if (checkingStatsName == "")
				return GetOutputPort("succeededNode")?.Connection?.node as DialogueNode;
			if (Hero.Instance.lifeController.Check(checkingStatsName, checkingValue))
				return GetOutputPort("succeededNode")?.Connection?.node as DialogueNode;
			else
				return GetOutputPort("failedNode")?.Connection?.node as DialogueNode;
		}
		set
		{
			nextNode = value;
		}
	}


    public string[] formatArgNames;
    public int speekerIndex = 0; //0是本人

	// Use this for initialization
	protected override void Init()
	{
		base.Init();
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port) {
		return port.Connection?.node;
	}
}