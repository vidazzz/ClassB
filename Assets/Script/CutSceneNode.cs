using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class CutSceneNode : Node {
	
	public string objName;
    public string destinationObjName;
    [HideInInspector]
    public GameObject obj;
    public CoroutineType coroutineType;
    public List<string> args;
    public DialogueGraph dialogueGraph;
    [HideInInspector]
    public GameObject destination;

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