using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public class Device : Interactable
{
    public string hobbyName;
    public Character owner;
    public float efficiencyMultiplier = 1f;
    public float outputMultiplier = 1f;
    
    public override IEnumerator Interact(Character starter)
    {
        ActionManager actionManager = starter.actionManager;
        if (starter is Hero)
        {
            Debug.Log("Hero using Device");
            yield return actionManager.StartCoroutine(actionManager.CurrentAction.ProcessHero());
            Debug.Log("Used by Hero");
        }
        else if(starter is NPC)
        {
            Debug.Log("actionManager.CurrentAction.target: " + actionManager.CurrentAction.target);
            yield return actionManager.StartCoroutine(actionManager.CurrentAction.ProcessNPC());
        }
        DeclaimedBy(starter);
    }

    protected void TogglePlayAnimation()
    {
        if (animator != null)
        {
            bool start = animator.GetBool("isOccupied");
            animator.SetBool("isOccupied", !start);
        }
    }

    new void Awake()
    {
        base.Awake();

    }

    private void Start()
    {
    }

    private void Update()
    {
    }
}
