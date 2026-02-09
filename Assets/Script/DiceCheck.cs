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
    public bool CheckAttribute(AttributeID checkingID,int checkingValue,Character character)
    {
        bool result;
        int value = character.lifeController.TryGetValueByEnum(checkingID, out Type type);
        if (type == typeof(Talent))
        {
            int dice = UnityEngine.Random.Range(0, value + checkingValue);
            result = dice < value;
        }
        else
        {
            // Unknown attribute type: treat as failure by default
            result = value >= checkingValue;
        }
        DisplayResult(checkingID.ToString(),result);
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
