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
    
    public string PredictionString(string checkingName ,int checkingValue ,Character character)
    {
        Character.TalentData talentData = character.GetTalent(checkingName);
        float possibility;
        Color color;
        possibility = (float)talentData.value/(talentData.value + checkingValue);
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
        return "[" + talentData.talent.TalentName + "]<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + possibility.ToString() +"%</color>";
    }
    public bool CheckTalent(string checkingTalentName,int checkingSkillLevel,Character character)
    {
        Character.TalentData heroTalentData = character.GetTalent(checkingTalentName);
        bool result;
        int dice = UnityEngine.Random.Range(0,heroTalentData.value + checkingSkillLevel);
        result = dice < heroTalentData.value;   
        DisplayResult(heroTalentData.talent.TalentName,result);
        return result;
    }
    
    public bool CheckStats(string statsName,float value,Character character)
    {
        bool result = character.GetComponent<LifeController>().GetStatValue(statsName) >= value;
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
