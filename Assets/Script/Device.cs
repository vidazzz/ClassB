using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Device : Interactable
{
    public Hobby hobby;
    public bool isJob;
    public Character owner;


    public override IEnumerator Interact(Character interactor)
    {
        users.Add(interactor);
        Need gainNeed = null;
        Debug.Log($"{interactor.name} handling {name} gain.needName = {gain.needName}");
        if (gain.needName != "")
        {
            gainNeed = interactor.lifeController.GetNeed(gain.needName);
            gainNeed.isChanging = true; //将需求标记为正在操作
            gainNeed.TryMotifyValue(gain.value);
        }  
        if (cost.needName != "")
            interactor.lifeController.GetNeed(cost.needName).TryMotifyValue(-cost.value);
        yield return StartCoroutine(PlayAnimation());
        if(gainNeed != null)
            gainNeed.isChanging = false;
        users.Remove(interactor);
    }

    protected IEnumerator PlayAnimation()
    {
        if (animator != null)
            animator.SetBool("isOccupied", true);
        Timer.Date beginTime = Timer.Time;
        while (Timer.GetPassedMinutes(beginTime) < duration) //使用时间
        {
            yield return null;
        }
        if (animator != null)
            animator.SetBool("isOccupied", false);
    }
}
