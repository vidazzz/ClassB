using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EssentialToggle : MonoBehaviour
{
    [HideInInspector]
    public float price;
    [HideInInspector]
    public ToggleGroup toggleGroup;
    private Toggle toggle;
    [HideInInspector]
    public Shop shop;
    [HideInInspector]
    public List<Toggle> toggles;
    private bool lastCheckedState = false;// 用于记录上次的选中状态
    public void CheckMoney()
    {
        bool isAffordable;
        if (toggle.isOn)
            isAffordable = Hero.Instance.lifeController.GetStatValue("money") >= shop.advancePayment;
        else
            isAffordable = Hero.Instance.lifeController.GetStatValue("money") >= shop.advancePayment + price;
        toggle.interactable = isAffordable;
        if (!isAffordable && toggle.isOn)
        {
            shop.ResetToggleGroup(toggles);
        }
    }
    public void ChangeAdvancePayment(bool newCheckedState)
    {
        if (toggle.isOn)
        {
            if (!lastCheckedState)
            {
                shop.advancePayment += price;
                lastCheckedState = newCheckedState; // 更新上次的选中状态
            }
        }
        else
        {
            shop.advancePayment -= price;
            lastCheckedState = newCheckedState; // 更新上次的选中状态
        }
        Debug.Log($"AdvancePayment changed to {shop.advancePayment}");
    }
    // Start is called before the first frame update
    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(ChangeAdvancePayment);
    }
    void Start()
    {
        lastCheckedState = toggle.isOn; // 初始化上次的选中状态
    }

    // Update is called once per frame
    void Update()
    {
        CheckMoney();
    }
}
