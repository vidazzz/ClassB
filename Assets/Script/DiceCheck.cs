using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;


public class DiceCheck : MonoBehaviour
{
    private static DiceCheck _instance; //单例
    public static DiceCheck Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DiceCheck>();
            }
            return _instance;
        }
    }
    private TextMeshProUGUI checkResultText;
    
    public string PredictionString(AttributeID checkingID ,int checkingValue ,Character character)
    {
        Talent talent = (Talent)character.lifeController.talentListManager.GetAttributeByEnum(checkingID);
        float possibility;
        Color color;
        possibility = (float)talent.value/(talent.value + checkingValue);
        possibility = (float)Math.Round(possibility, 2)*100;
        if(possibility < 10)
            color = Color.gray;
        else if(possibility <30)
            color = Color.red;
        else if(possibility <60)
            color = Color.yellow;
        else if(possibility <80)
            color = Color.cyan;
        else
            color = Color.green;
        return "[" + talent.Name + "]<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + possibility.ToString() +"%</color>";
    }
    public bool CheckTalent(AttributeID checkingTalentID,int checkingSkillLevel,Character character)
    {
        Talent heroTalent = (Talent)character.lifeController.talentListManager.GetAttributeByEnum(checkingTalentID);
        bool result;
        int dice = (int)UnityEngine.Random.Range(0,heroTalent.value + checkingSkillLevel);
        result = dice < heroTalent.value;   
        DisplayResult(heroTalent.Name,result);
        return result;
    }
    
    public bool CheckStats(AttributeID statsID,float value,Character character)
    {
        bool result = character.GetComponent<LifeController>().statListManager.GetAttributeByEnum(statsID).value >= value;
        return result;
    }

    public void DisplayResult(string talentName, bool checkResult)
    {
        string result = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.magenta)}>{talentName}</color>: ";
        if (checkResult)
            result += $"<color={"green"}>success";
        else
            result += $"<color={"red"}>failure";
        PopUp.Instance.ShowPopUp(result);
    }

    public void Check()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        checkResultText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
