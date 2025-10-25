using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WhitBoard : Interactable
{
    public GameObject PMUI;
    public override IEnumerator Interact(Character interactor)
    {
        if (interactor is Hero)
        {
            //UIManager.Instance.OpenUI("ProjectUI");
            PMUI.SetActive(true);
            DynamicGridLayout PM_Layout = PMUI.GetComponentInChildren<DynamicGridLayout>();
            PM_Layout.panels.Clear();//清空
            //构建module panel
            PM_Layout.AddPanel(Company.Instance.project.modules.Count);
            for (int i = 0; i < PM_Layout.panels.Count; i++)
            {
                DynamicGridLayout GroupLayout = PM_Layout.panels[i].GetComponent<DynamicGridLayout>();
                //构建member panel
                GroupLayout.AddPanel(Company.Instance.project.modules[i].group.members.Count);
                for (int j = 0; j < GroupLayout.panels.Count; j++)
                {
                    MemberPanelUI memberPanelUI = GroupLayout.panels[j].GetComponent<MemberPanelUI>();
                    memberPanelUI.memberNameText.text = Company.Instance.project.modules[i].group.members[j].name;
                    memberPanelUI.DisplayTasks(Company.Instance.project.modules[i].group.members[j].workManager.tasks);
                }
                yield return null;
                GroupLayout.ArrangePanelsInGrid(); 
                
            }
            yield return null;
            PM_Layout.ArrangePanelsInGrid();    
        }   
        yield return null;
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
