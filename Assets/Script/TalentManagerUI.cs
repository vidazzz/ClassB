using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentManagerUI : MonoBehaviour
{
    private Character character;
    private List<Talent> talentList;
    public GameObject prefTalentItemUI;

    private void InitializeTalentItemUI()
    {
        foreach (Talent talent in talentList)
        {
            GameObject talentItemUIObj = Instantiate(prefTalentItemUI, transform);
            
            TalentItemUI newTalentItemUI  = talentItemUIObj.GetComponent<TalentItemUI>();
            newTalentItemUI.talent = talent;
            newTalentItemUI.DisplayTalent();
        }
    }
    void Awake()
    {
        character = Hero.Instance;
        talentList = character.lifeController.talentList;
    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeTalentItemUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
