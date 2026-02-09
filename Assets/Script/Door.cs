using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DialogueController))]
public class Door : Item
{
    [HideInInspector]
    public DialogueController dialogueController;
    public override IEnumerator Interact(Character interactor)
    {
        if(interactor is Hero) //对象是主角
        {
            Timer.Pause();
            if(dialogueController.dialogueGraph != null)
                yield return StartCoroutine(dialogueController.GraphDisplayDialogue(dialogueController.dialogueGraph));
            Timer.Resume();
        }
        else //interactor is NPC
        {
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        dialogueController = GetComponent<DialogueController>();
    }
}
