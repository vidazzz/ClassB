using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ConvPanel : MonoBehaviour
{
    public Character speeker;
    public string line;
    public List<ConvOptionData> dialogueOptionDataList;

    [System.Serializable]
    public class ConvOptionData
    {
        public string optionText; // 选项文案
        public System.Action onSelectCallback; // 选中回调
    }
    
    [Header("核心配置")]
    [Tooltip("对话气泡的偏移量")]
    public Vector2 offset = new(0,1);
    [Tooltip("line文本text")]
    public TextMeshProUGUI lineText;
    [Tooltip("所有选项按钮的父容器")]
    public Transform optionsGroup;
    [Tooltip("动态按钮的尺寸")]
    public Vector2 btnSize = new(300, 60);
    [Tooltip("选项选中时的高亮颜色")]
    public Color selectColor = Color.yellow;
    [Tooltip("选项未选中时的默认颜色")]
    public Color normalColor = Color.white;
    public bool isFinished = false;


    // 选项按钮列表
    private List<Button> optionBtnList = new();
    // 当前选中的选项索引（核心变量）
    private int currentSelectIndex = 0;
    // 按钮默认的文字组件（兼容UGUI原生Text）
    private List<TextMeshProUGUI> btnTextList = new();

    private void Awake()
    {
    }

    private void Start()
    {
        // 默认选中第一个选项，显示高亮
        UpdateOptionSelectState();
    }

    private void Update()
    {
        UpdateTransform();
        // 若无选项，直接退出
        if (optionBtnList.Count == 0) return;

        // 监听【上键】- 切换上一个选项
        if (Hero.Instance.menuNavigateAction.action.ReadValue<Vector2>().y > 0.1f && Hero.Instance.menuNavigateAction.action.WasPressedThisFrame())
        {
            currentSelectIndex--;
            // 边界限制：索引不能小于0（第一个选项）
            currentSelectIndex = Mathf.Max(currentSelectIndex, 0);
            UpdateOptionSelectState();
        }

        // 监听【下键】- 切换下一个选项
        if (Hero.Instance.menuNavigateAction.action.ReadValue<Vector2>().y < -0.1f && Hero.Instance.menuNavigateAction.action.WasPressedThisFrame())
        {
            currentSelectIndex++;
            // 边界限制：索引不能大于选项总数-1（最后一个选项）
            currentSelectIndex = Mathf.Min(currentSelectIndex, optionBtnList.Count - 1);
            UpdateOptionSelectState();
        }

        // 监听【确认键】- 触发选中选项的点击事件
        if (Hero.Instance.menuSelectAction.action.WasPerformedThisFrame())
        {
            OnOptionConfirm(currentSelectIndex);
        }

        // 监听【退出键】- 关闭菜单
        if (Hero.Instance.menuBackAction.action.WasPerformedThisFrame())
        {
            isFinished = true;
            return;
        }
    }

    #region 动态生成按钮（无预制体）
    /// <summary>
    /// 根据选项数据，批量生成按钮并绑定逻辑
    /// </summary>
    /// <param name="optionDatas">选项数据列表</param>
    public void GenerateOptionButtons(List<ConvOptionData> optionDatas)
    {
        // 清空旧按钮+旧数据（避免重复生成）
        ClearAllOptionButtons();
        optionBtnList.Clear();
        btnTextList.Clear();
        dialogueOptionDataList.Clear();

        foreach (var data in optionDatas)
        {
            dialogueOptionDataList.Add(data); // 保存数据，供回车确认使用
            int index = optionBtnList.Count; // 记录当前按钮索引
            // 1. 创建按钮GameObject
            GameObject btnObj = new($"OptionBtn_{optionBtnList.Count + 1}");
            btnObj.transform.SetParent(optionsGroup, false); // 设为子物体，false保持本地缩放

            // 2. 添加Button组件（核心交互）
            Button btn = btnObj.AddComponent<Button>();
            
            // 3. 添加Image组件（按钮背景，纯色）
            Image bgImg = btnObj.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f); // 按钮背景色（深灰）
            btn.targetGraphic = bgImg; // 绑定Button的交互图形

            // 4. 设置按钮尺寸（RectTransform）
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.sizeDelta = btnSize;

            // 5. 创建文字子物体，添加Text组件
            GameObject textObj = new ("OptionText");
            textObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = data.optionText; // 赋值选项文案
            text.color = normalColor;    // 默认文字颜色
            text.fontSize = 24;          // 字号
            text.alignment = TextAlignmentOptions.Center; // 文字居中

            // 6. 绑定按钮点击事件+导航逻辑
            btn.onClick.AddListener(() =>
            {
                Debug.Log("onClick currentSelectIndex: "+ index );
                currentSelectIndex = index; // 同步索引
                UpdateOptionSelectState();  // 更新高亮
                data.onSelectCallback?.Invoke(); // 执行业务逻辑
            });

            // 7. 存入列表，纳入导航体系
            optionBtnList.Add(btn);
            btnTextList.Add(text);

            // 8. 绑定选项的业务回调
            data.onSelectCallback += () => { Debug.Log($"选中选项：{data.optionText}"); isFinished = true; };
        }

        // 初始化默认选中第一个选项
        if (optionBtnList.Count > 0)
        {
            currentSelectIndex = 0;
            UpdateOptionSelectState();
        }
    }

    /// <summary>
    /// 清空所有动态生成的按钮（关键：避免重复）
    /// </summary>
    public void ClearAllOptionButtons()
    {
        foreach (Transform child in optionsGroup)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    private void UpdateTransform()
    {
        if (speeker != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(speeker.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent as RectTransform, screenPos, Camera.main, out Vector2 localPoint);
            (transform as RectTransform).localPosition = localPoint + offset;
        }
    }

    /// <summary>
    /// 更新选项选中状态：高亮选中项，恢复未选中项
    /// </summary>
    private void UpdateOptionSelectState()
    {
        for (int i = 0; i < optionBtnList.Count; i++)
        {
            if (btnTextList[i] == null) continue;
            
            // 选中项：设置高亮颜色
            if (i == currentSelectIndex)
            {
                btnTextList[i].color = selectColor;
            }
            // 未选中项：恢复默认颜色
            else
            {
                btnTextList[i].color = normalColor;
            }
        }
    }

    /// <summary>
    /// 选项确认逻辑（核心回调，在这里写选中后的业务）
    /// </summary>
    /// <param name="selectIndex">选中的选项索引</param>
    private void OnOptionConfirm(int selectIndex)
    {
        Debug.Log("OnOptionConfirm");
        dialogueOptionDataList[selectIndex].onSelectCallback?.Invoke();
    }
}
