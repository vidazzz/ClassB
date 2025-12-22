using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentItemUI : MonoBehaviour
{
    public Talent talent;
    private TextMeshProUGUI textMesh;
    private Button button;
    public void UsePoint()
    {
        Hero.Instance.lifeController.UsePoint(talent);
        DisplayTalent();
        CheckRemainPoint();
    }
    public void DisplayTalent()
    {
        textMesh.text = $"{talent.Name} {talent.value}";
    }
    private void CheckRemainPoint()
    {
        if (Hero.Instance.lifeController.statListManager.GetAttributeByEnum(AttributeID.point).value <= 0)
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
        CheckRemainPoint();
    }
}
