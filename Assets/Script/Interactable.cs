using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public float interactTime;
    protected Animator animator;
    [NonSerialized] public NPC occupiedBy;
    public Vector3[] interactPoint;

    [SerializeField] protected string interactionPrompt = "互动";
    [SerializeField] protected KeyCode interactionKey = KeyCode.E;
    
    // 显示互动提示
    public virtual void ShowPrompt()
    {
        // 这里可以实现UI提示的显示逻辑
        Debug.Log($"按 {interactionKey} 与 {gameObject.name} 互动");
    }
    
    // 隐藏互动提示
    public virtual void HidePrompt()
    {
        // 这里可以实现UI提示的隐藏逻辑
    }
    
    // 互动的核心方法，由子类实现具体功能
    public abstract IEnumerator Interact(Character interactor);
    
    protected void Awake()
    {
        animator = GetComponent<Animator>();
    }
}
