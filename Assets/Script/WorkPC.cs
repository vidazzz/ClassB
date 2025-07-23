using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkPC : Interactable
{
    
    TypingGame typingGame;
    public override IEnumerator Interact(Character interactor)
    {
        Hero.Instance.canActive = false;
        yield return StartCoroutine(typingGame.StartTyping());
        Hero.Instance.canActive = true;       
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
