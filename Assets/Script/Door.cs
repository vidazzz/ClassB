using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    [HideInInspector]
    public DialogueController dialogueController;
    public override IEnumerator Interact(Character interactor)
    {
        if(interactor is Hero) //对象是主角
        {
            Timer.Pause();
            Hero.Instance.canActive = false;
            int dialogueIndex = 0; //需要控制的时候找地方再赋值
            if(dialogueController.dialogues.Count > 0)
                dialogueController.Initialize(dialogueController.dialogues[dialogueIndex]);
            yield return StartCoroutine(dialogueController.DisplayDialogue());
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
