using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckOnDutyPoint : Interactable
{
    public Character character;
    public float addToKpiMultiplier = 1;
    public Interactable beeningChecked;

    public override IEnumerator Interact(Character interactor)
    {
        if(interactor is NPC)
        {
            yield return StartCoroutine(CheckOnDuty(interactor as NPC));
        }
    }

    IEnumerator CheckOnDuty(NPC overseer)
    {
        StartCoroutine(overseer.ShowPopUp("Result",interactTime));
        
        character.lifeController.AddModifier("kpiMultiplier",addToKpiMultiplier);
        yield return new WaitForSeconds(interactTime);
        character.lifeController.AddModifier("kpiMultiplier",-addToKpiMultiplier); //结束查岗 恢复原先的kpi倍率
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
