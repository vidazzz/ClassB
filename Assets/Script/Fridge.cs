using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fridge : Interactable
{
    public override IEnumerator Interact(Character interactor)
    {
        yield return StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        animator.SetBool("isOccupied",true);
        yield return new WaitForSeconds(interactTime);
        animator.SetBool("isOccupied",false);
    }
}
