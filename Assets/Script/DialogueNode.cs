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
			if (formatIds.Length > 0)
			{
				string[] formatArgs = new string[formatIds.Length];
				for (int i = 0; i < formatIds.Length; i++)
				{
					formatArgs[i] = formatIds[i].ToString();
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

    public AttributeID checkingStatID;
    public int checkingValue;
	public DialogueGraph nextGraph; //对话图
	private DialogueNode nextNode; 
	public DialogueNode NextNode //下一段节点
	{
		get
		{
			if (nextNode != null)
				return nextNode;
			if (checkingStatID == 0)
				return GetOutputPort("succeededNode")?.Connection?.node as DialogueNode;
			if (DiceCheck.Instance.CheckAttribute(checkingStatID, checkingValue, Hero.Instance))
				return GetOutputPort("succeededNode")?.Connection?.node as DialogueNode;
			else
				return GetOutputPort("failedNode")?.Connection?.node as DialogueNode;
		}
		set
		{
			nextNode = value;
		}
	}


    public AttributeID[] formatIds;
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