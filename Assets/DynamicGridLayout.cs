using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DynamicGridLayout : MonoBehaviour
{
    public bool isVertical; // 是否垂直排列
    private RectTransform container;
    public GameObject panelPrefab;
    public float spacing; // Panel之间的间距
    public List<RectTransform> panels = new();

    private void Awake()
    {
        container = GetComponent<RectTransform>();

    }
    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        
    }
    private void Start()
    {

    }

    public void AddPanel(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newPanel = Instantiate(panelPrefab, container);
            RectTransform panelRect = newPanel.GetComponent<RectTransform>();
            panels.Add(panelRect);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void ArrangePanelsInGrid()
    {
        float containerHeight = container.rect.height; // 容器高度
        float currentX = 0; // 当前行的X坐标
        float currentY = 0; // 当前行的Y坐标


        foreach (var panel in panels)
        {
            float panelWidth = panel.rect.width;
            float panelHeight = panel.rect.height;

            // 检查是否需要换列
            if (!isVertical)
                if (currentY + panelHeight > containerHeight && currentY != 0)
                {
                    currentY = 0; // 重置Y坐标到行首
                    currentX += panelWidth + spacing; // 换列：X坐标右移
                }

            // 设置Panel位置
            panel.anchoredPosition = new Vector2(currentX, -currentY); // Y为负（UGUI特性）
            // 更新Y坐标为下一Panel位置
            currentY += panelHeight + spacing;
            Debug.Log($"currentY: {currentY} panelHeight: {panelHeight}");
        }

        // 调整容器高度以包裹所有内容
        if(isVertical)
            container.sizeDelta = new Vector2(container.sizeDelta.x, currentY);
    }
}
