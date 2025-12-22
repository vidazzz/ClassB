using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;

[RequireComponent(typeof(TypingGame))]
public class WorkPC : Device
{
    /*
    public override IEnumerator Interact(Character interactor)
    {
        operation = interactor.workManager.CurrentTask;
        if(operation == null)
        {
            Debug.LogError("No task assigned!");
            yield break;
        }
        users.Add(interactor);
        StartCoroutine(PlayAnimation());
        if (interactor is Hero)
        {
            Hero.Instance.canActive = false;
            progressBar.gameObject.SetActive(true);
            yield return interactor.StartCoroutine(operation.ProcessHero());
            progressBar.gameObject.SetActive(false);
            Hero.Instance.canActive = true;
        }
        else if (interactor is NPC)
        {
            yield return StartCoroutine(operation.ProcessNPC());
            owner.action.UpdatePriorityWork();
        }
        users.Remove(interactor);
    }
    */
}
