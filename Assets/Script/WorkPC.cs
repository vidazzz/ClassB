using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TypingGame))]
public class WorkPC : Device
{
    
    TypingGame typingGame;
    public override IEnumerator Interact(Character interactor)
    {
        if (interactor is Hero)
        {
            Hero.Instance.canActive = false;
            yield return StartCoroutine(typingGame.StartTyping());
            Hero.Instance.canActive = true;
        }
        else
        {
            if (gain.needName != "")
                interactor.lifeController.GetNeed(gain.needName).TryMotifyValue(gain.value);
            if(cost.needName != "")
                interactor.lifeController.GetNeed(cost.needName).TryMotifyValue(-cost.value);
            yield return StartCoroutine(PlayAnimation());
        }
             
    }

    // Start is called before the first frame update
    void Start()
    {
        typingGame = GetComponent<TypingGame>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
