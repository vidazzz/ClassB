using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    
    public string PredictionString(string skillName,int checkingSkillLevel)
    {
        Talent skill = Hero.Instance.GetTalent(skillName);
        float possibility;
        Color color;
        possibility = (float)skill.level/(skill.level + checkingSkillLevel);
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
        return "[" + skill.name + "]<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + possibility.ToString() +"%</color>";
    }
    public bool Check(Talent talent,int checkingSkillLevel)
    {
        bool result;
        
        int dice = UnityEngine.Random.Range(0,talent.level + checkingSkillLevel);
        result = dice < talent.level;   
        
       
        StartCoroutine(DisplayResult(talent.name,result));
        return result;
            
    }

    public IEnumerator DisplayResult(string talentName,bool checkResult)
    {
        string result = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.magenta)}>{talentName}</color>: ";
        if(checkResult)
            result += $"<color={"green"}>success";
        else
            result += $"<color={"red"}>failure";
        checkResultText.text = result;
        checkResultText.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(5); //弹窗时间
        checkResultText.transform.parent.gameObject.SetActive(false);
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
