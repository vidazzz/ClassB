using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    protected List<Character> users = new();
    public int userLimit;
    public bool IsFull { get { return userLimit > 0 && users.Count >= userLimit; } }

    public int interactDisdence = 1; //交互距离,单位是一个Astar.node的半径
    public Vector3[] interactPoints;

    [SerializeField] protected string interactionPrompt = "互动";
    [SerializeField] protected KeyCode interactionKey = KeyCode.E;
    
    public bool TryClaimedBy(Character character)
    {
        if(!IsFull && !CheckIfClaimedBy(character))
        {
            users.Add(character);
            return true;
        }
        else
            return false;
    }

    public bool CheckIfClaimedBy(Character character)
    {
        return users.Contains(character);
    }
    public void DeclaimedBy(Character character)
    {
        users.Remove(character);
    }
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
        //将可交互物的Z轴位置初始化为0
        transform.position = new(transform.position.x,transform.position.y,0);
    }
}
