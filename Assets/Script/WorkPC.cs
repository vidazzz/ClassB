using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

[RequireComponent(typeof(TypingGame))]
public class WorkPC : Device
{
    public override IEnumerator Interact(Character interactor)
    {
        if (interactor is Hero)
        {
            Hero.Instance.canActive = false;
            //yield return StartCoroutine();
            Hero.Instance.canActive = true;
        }
        else if(interactor is NPC)
        {
            NPC npc = interactor as NPC;
            yield return StartCoroutine(npc.workManager.Process(npc.workEfficiency, (int)duration));
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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
