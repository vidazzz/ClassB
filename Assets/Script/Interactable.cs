using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected Animator animator;
    public List<Character> users;
    public int userLimit;
    public bool IsInUse { get { return userLimit > 0 && users.Count >= userLimit; } }
    public NeedChange cost;
    public NeedChange gain;
    public float duration;
    public int interactDisdence = 1; //交互距离,单位是一个Astar.node的半径
    [Serializable]
    public class NeedChange
    {
        public string needName;
        public float value;
    }
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
