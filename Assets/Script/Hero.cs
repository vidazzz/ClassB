using System.Collections;
using TMPro;
using UnityEngine;

public class Hero : Character
{
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
    
    private Interactable currentInteractable;
    
    void CheckForInteractables()
    {
        // 射线检测前方可互动物品
        Collider2D collider = Physics2D.OverlapPoint(transform.position+(Vector3)currentDirection);

         Debug.DrawLine(transform.position,transform.position + (Vector3)currentDirection, Color.yellow, 0.1f);
        if (collider != null)
        {
            Interactable interactable = collider.GetComponent<Interactable>();
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
        // 当按下互动键且有可互动物品时，执行互动
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            StartCoroutine(currentInteractable.Interact(this));
        }
    }

    void Respawn()
    {
        transform.position = spawnPosition;
    }

    public Talent GetTalent(string skillName)
    {
        Debug.Assert(skillName != null,"dialogueTextMesh不能为null");
        return talents[skillName];
    }


    public void DisplayStatsValue()
    {
        LifeValueText.text = "";
        foreach(var statsPair in lifeController.statsPairs)
        {
            LifeValueText.text += $"{statsPair.Key} : {statsPair.Value}\n";
        }
    }

    //等一下加载
    public IEnumerator Delay()
    {
        yield return new WaitForSeconds(1);
        
    }
    new void Awake()
    {
        base.Awake();

    }
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        DisplayStatsValue(); 
        Timer.onDayBegin += Respawn;
    }

    void OnDestroy()
    {
        Timer.onDayBegin -= Respawn;
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
        //显示日常属性
    }
}
