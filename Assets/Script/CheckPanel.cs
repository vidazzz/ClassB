using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckPanel : MonoBehaviour
{
    public static CheckPanel instance;
    public Check check;
    public GameObject prefCheckItemUI;
    public Transform checkItemPanel;
    public Slider slider;
    public Button startButton;
    public TextMeshProUGUI buttonText;
    public GameObject prefSkillCard;
    public Transform skillPanel;
    public Transform dropZone;

    private List<TextMeshProUGUI> texts = new();
    public static void ToggleUI()
    {
        instance.gameObject.SetActive(!instance.gameObject.activeSelf);
    }
    public void Initialize(Check check)
    {
        //清理
        int i = checkItemPanel.childCount - 1;
        if (i >= 0)
        {
            for (; i >= 0; i--)
            {
                Destroy(checkItemPanel.GetChild(i).gameObject);
            }
        }

        foreach (TalentSkill talentSkill in check.challenger.talentSkills)
        {
            GameObject skillCard = Instantiate(prefSkillCard, skillPanel);
            DragHandler dragHandler = skillCard.GetComponent<DragHandler>();
            dragHandler.check = check;
            dragHandler.talentSkill = talentSkill;
            dragHandler.targetParent = dropZone;
            dragHandler.Display();
        }
        this.check = check;
        texts = new();
        foreach (Check.CheckItem checkItem in check.checkItems)
        {
            TextMeshProUGUI text = Instantiate(prefCheckItemUI, checkItemPanel).GetComponentInChildren<TextMeshProUGUI>();
            texts.Add(text);
        }
        Display();
    }

    public void Display()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            texts[i].text = $"check {check.checkItems[i].name}:+{check.checkItems[i].checkResult}";
        }

        slider.value = (float)check.currentValue / check.maxValue;
        buttonText.text = $"{check.currentValue} / {check.maxValue}";
    }
    public void StartCheck()
    {
        StartCoroutine(CheckCoroutine());
        Display();//update display
    }
    private IEnumerator CheckCoroutine()
    {
        startButton.interactable = false;
        yield return check.Process();
        startButton.interactable = true;
        if (check.state != Check.State.checking)
            gameObject.SetActive(false);
    }
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        gameObject.SetActive(false);
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
