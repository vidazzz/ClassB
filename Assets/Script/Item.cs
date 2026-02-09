using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : Interactable
{
    public override IEnumerator Interact(Character interactor)
    {
        Debug.Log("Interacting with item");
        yield return null;
    }
}
