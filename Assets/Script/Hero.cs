using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hero : Character
{
    private static readonly WaitForSeconds _waitForSeconds = new(1);
    public InputActionAsset inputAction;
    public InputActionReference interactAction;
    public InputActionReference useAction;
    public InputActionReference menuNavigateAction;
    public InputActionReference menuSelectAction;
    public InputActionReference menuBackAction;
    private static Hero _instance; //单例
    public static Hero Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Hero>();
            }
            return _instance;
        }
    }
    public TextMeshProUGUI LifeValueText;
    [HideInInspector]
    public bool canActive = true;
    Vector2 currentDirection = -Vector2.up;
    [HideInInspector]
    public Action currentAction;
    private Interactable currentInteractable;
    public int lagerMask;

    private void Move()
    {
        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");
        Vector2 v = new Vector2(Horizontal,Vertical);
        if(v != Vector2.zero)
        {
            currentDirection = v.normalized;
            transform.position += Time.deltaTime*fSpeed*(Vector3)currentDirection;

            animator.SetBool("IsMoving",true);
            animator.SetFloat("X",currentDirection.x);
            animator.SetFloat("Y",currentDirection.y);
        }
        else
            animator.SetBool("IsMoving",false);
        
    }
    
    void CheckForInteractables()
    {
        // 射线检测前方可互动物品
        RaycastHit2D  hit = Physics2D.Raycast(transform.position,(Vector3)currentDirection,1,lagerMask);

        if (hit)
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                // 如果检测到新的可互动物品
                if (interactable != currentInteractable)
                {
                    // 隐藏之前的互动提示
                    if (currentInteractable != null)
                        currentInteractable.HidePrompt();
                    
                    // 设置当前可互动物品并显示提示
                    currentInteractable = interactable;
                    currentInteractable.ShowPrompt();
                }
                return;
            }
        }
        
        // 如果没有检测到可互动物品，隐藏提示
        if (currentInteractable != null)
        {
            currentInteractable.HidePrompt();
            currentInteractable = null;
        }
    }
    
    void HandleInteractionInput()
    {
        if(currentInteractable == null)
            return;
        //如按下交互键，显示选项

        if (interactAction.action.WasPerformedThisFrame())
        {
            StartCoroutine(ConvController.Instance.ShowOptions(currentInteractable));
            Debug.Log("Interact button");
        }
        
        //如果按住使用键且当前可互动物品是设备，使用设备
        if (useAction.action.WasPerformedThisFrame() && currentInteractable is Device device)
        {
            Debug.Log("Use button");
            UseDevice(device);
        }
            
    }

    //使用设备
    public void UseDevice(Device device)
    {
        Action action = actionManager.ChooseActionHero(device);
        if(action == null)
        {
            Debug.LogWarning("No valid action found for device interaction.");
            return; 
        } 
        StartCoroutine(action.ProcessHero());
    }
    //简单交互
    public void SampleInteractWith(Item item)
    {
        StartCoroutine(item.Interact(this));
    }

    // 调查
    public void Investigate(Interactable target, Topic topic)
    {
        
    }
    // 交涉
    public void Negotiate(Character target, Topic topic)
    {
        StartCoroutine(NegotiationCoroutine(target, topic));
    }
    public IEnumerator NegotiationCoroutine(Character target, Topic topic)
    {
        Negotiation Negotiation = new(topic, target, Timer.Time, Timer.Time + 1);

        yield return _waitForSeconds;

    }

    public void EnableInputActionMapPlayer()
    {
        inputAction.FindActionMap("Menu").Disable();
        inputAction.FindActionMap("Player").Enable();
    }

    public void EnableInputActionMapMenu()
    {
        inputAction.FindActionMap("Player").Disable();
        inputAction.FindActionMap("Menu").Enable();
    }

    void Respawn(int invokeDay = -1)
    {
        transform.position = spawnPosition;
    }
    
    public void DisplayStatsValue()
    {
        lifeController.DisplayStatsValue(LifeValueText);
    }

    //等一下加载
    public IEnumerator Delay()
    {
        yield return new WaitForSeconds(1);
        
    }
    new void Awake()
    {
        base.Awake();
        lagerMask = LayerMask.GetMask("Interactable");
        EventManager.Instance.OnDayBegin += Respawn;
    }
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        DisplayStatsValue(); 
        socialManager.SetUpTopics();
    }

    void OnDestroy()
    {
        EventManager.Instance.OnDayBegin -= Respawn;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(!canActive || Timer.hasPaused) //标记不能行动或timer暂停时不响应输入
            return;  
        Move();
        // 检测前方可互动物品
        CheckForInteractables();
        // 处理互动输入
        HandleInteractionInput();
    }
}
