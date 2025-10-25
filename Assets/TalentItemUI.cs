using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentItemUI : MonoBehaviour
{
    public Character.TalentData talentData;
    private TextMeshProUGUI textMesh;
    private Button button;
    public void UsePoint()
    {
        Hero.Instance.lifeController.UsePoint(talentData);
        DisplayTalent();
        CheckRemainintPoint();
    }
    public void DisplayTalent()
    {
        textMesh.text = $"{talentData.talent.TalentName} {talentData.value}";
    }
    private void CheckRemainintPoint()
    {
        if (Hero.Instance.lifeController.GetStat("point").value <= 0)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }
    void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponentInChildren<Button>();
        CheckRemainintPoint();
    }
}
