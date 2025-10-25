using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalentManagerUI : MonoBehaviour
{
    private Character character;
    private List<Character.TalentData> talentDatas;
    public GameObject prefTalentItemUI;

    private void InitializeTalentItemUI()
    {
        foreach (Character.TalentData talentData in talentDatas)
        {
            TalentItemUI newTalentItemUI = Instantiate(prefTalentItemUI, transform).GetComponent<TalentItemUI>();
            newTalentItemUI.talentData = talentData;
            newTalentItemUI.DisplayTalent();
        }
    }
    void Awake()
    {
        character = Hero.Instance;
        talentDatas = character.talentDataList;
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
