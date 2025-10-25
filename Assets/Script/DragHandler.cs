using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 卡牌原始父物体（用于释放时归位）
    private Transform originalParent;
    // 拖拽时的临时父物体（通常是Canvas，避免被其他UI遮挡）
    private Transform canvasTransform;
    // 卡牌初始位置（用于取消拖拽时复位）
    private Vector3 originalPosition;
    // Canvas Group组件（用于拖拽时调整透明度）
    private CanvasGroup canvasGroup;
    public Transform targetParent;
    public TalentSkill talentSkill;
    public Check check;
    public TextMeshProUGUI displayText;

    private void Awake()
    {
        // 获取Canvas作为拖拽时的临时父物体（确保卡牌在最上层）
        canvasTransform = GameObject.Find("Canvas").transform;
        // 获取自身CanvasGroup组件（若没有则添加）
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // 开始拖拽时调用
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 记录原始父物体和位置
        originalParent = transform.parent;
        originalPosition = transform.position;

        // 拖拽时将卡牌临时放到Canvas下（避免被其他UI遮挡）
        transform.SetParent(canvasTransform);
        // 关闭射线检测（防止拖拽时卡牌自身拦截事件）
        canvasGroup.blocksRaycasts = false;
        // 拖拽时半透明效果
        canvasGroup.alpha = 0.7f;
    }

    // 拖拽过程中持续调用
    public void OnDrag(PointerEventData eventData)
    {
        // 将鼠标位置转换为UI世界坐标，并更新卡牌位置
        Vector3 newPosition;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasTransform.GetComponent<RectTransform>(),  // 参考Canvas的RectTransform
            eventData.position,                            // 鼠标屏幕位置
            eventData.pressEventCamera,                    // 事件相机（Overlay模式可传null）
            out newPosition))                              // 输出转换后的世界坐标
        {
            transform.position = newPosition;
        }
    }

    // 结束拖拽时调用
    public void OnEndDrag(PointerEventData eventData)
    {
        // 恢复射线检测和透明度
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // 检测释放位置是否在目标区域
        if (IsDroppedOnTarget(eventData))
        {
            // 执行释放逻辑
            Debug.Log("卡牌释放到目标区域！");
            transform.SetParent(targetParent);

            talentSkill.Execute(check);
        }
        else
        {
            // 未释放到目标区，回到原始位置
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }

    // 检测是否释放到目标区域（需手动实现）
    private bool IsDroppedOnTarget(PointerEventData eventData)
    {
        // 方法1：通过标签检测（假设目标区域有"DropZone"标签）
        if (eventData.pointerCurrentRaycast.gameObject != null
            && eventData.pointerCurrentRaycast.gameObject.CompareTag("DropZone"))
        {
            return true;
        }

        // 方法2：通过指定目标区域的RectTransform检测（更精确）
        // RectTransform dropZone = GameObject.Find("DropZone").GetComponent<RectTransform>();
        // return RectTransformUtility.RectangleContainsScreenPoint(
        //     dropZone, eventData.position, eventData.pressEventCamera);

        return false;
    }

    public void Display()
    {
        displayText.text = talentSkill.name;
    }
}
