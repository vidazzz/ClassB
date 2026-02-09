using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UIElements;

[RequireComponent(typeof(AStar))]
[RequireComponent(typeof(NPC))]
public class BehaviorManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);
    public NPC owner;
    protected ActionManager actionManager;
    protected Animator animator;
    protected Animator popUpAnimator;
    protected AStar astar;
    public SocialManager socialManager;
    private Rader rader;
    public bool isMeeting;
    private bool isTargetInRader = false;
    private bool hasArrived = false;
    private Action lastAction;

    public enum PopUpType
    {
        none = 0,
        talking = 1,
        shock = 2,
        happy = 3,
        progress = 4,
        sleep = 5,
        wantToSocial = 6,
        wantToEat = 7,
        wantToPlay = 8,
        wantToSleep = 9,
        wantToPee = 10,
    }

    void AnimateMove(Vector3 direction)
    {
        if(animator != null)
        {
            animator.SetBool("IsMoving",true);
            animator.SetFloat("X",direction.x);
            animator.SetFloat("Y",direction.y);
        }
    }
    void AnimateStopMove()
    {
        if(animator != null)
        {
            animator.SetBool("IsMoving",false);
            //popUpAnimator.SetInteger("Result",0); //关掉表情气泡
        }
    }

    //头顶气泡
    public void ToggleShowPopUp(Action action)
    {
        string reactionType = "Result";
        var popUpType = action.theme switch
        {
            0 => (PopUpType)0,
            AttributeID.sleep => PopUpType.sleep,
            AttributeID.social => PopUpType.talking,
            _ => PopUpType.progress,
        };
        if (popUpAnimator.GetInteger(reactionType) == 0)
            popUpAnimator.SetInteger(reactionType,(int)popUpType);
        else
            popUpAnimator.SetInteger(reactionType,0);
    }

    public void ToggleShowPopUp(PopUpType popUpType  = 0)
    {
        string reactionType = "Result";
        if (popUpAnimator.GetInteger(reactionType) == 0)
            popUpAnimator.SetInteger(reactionType,(int)popUpType);
        else
            popUpAnimator.SetInteger(reactionType,0);
    }

    public IEnumerator ShowPopUpCoroutin(Action action)
    {
        string reactionType = "Result";
        int originState = popUpAnimator.GetInteger(reactionType);
        var popUpType = action.theme switch
        {
            0 => PopUpType.none,
            AttributeID.sleep => PopUpType.wantToSleep,
            AttributeID.social => PopUpType.wantToSocial,
            AttributeID.sports => PopUpType.wantToPlay,
            AttributeID.fun => PopUpType.wantToPlay,
            AttributeID.toilet => PopUpType.wantToPee,
            AttributeID.eat => PopUpType.wantToEat,
            _ => PopUpType.none,
        };
        if(popUpType == PopUpType.none)
            yield break;
        popUpAnimator.SetInteger(reactionType,(int)popUpType);
        yield return _waitForSeconds1;
        //如果动画中途没有被其他情况改变，恢复到动画播放前的状态
        if(popUpAnimator.GetInteger(reactionType)==(int)popUpType)
            popUpAnimator.SetInteger(reactionType,originState);
    }

    public IEnumerator StartConv(Character target)
    {
        AnimateStopMove();
        FaceTheTarget(target.gameObject);
        yield return _waitForSeconds1;

    }
    public IEnumerator Greeting()
    {
        ToggleShowPopUp(PopUpType.talking);
        yield return new WaitForSeconds(Random.Range(0.5f,1.5f));
        ToggleShowPopUp();
    }

    public IEnumerator AIBehaviorCoroutine()
    {
        while (true)
        {
            yield return StartCoroutine(ProsessSchedule());
        }
    }

    //AI行为协程
    public IEnumerator ProsessSchedule(Action action = null)
    {
        isTargetInRader = false;
        hasArrived = false;
        yield return null;
        if(actionManager.actions.Count == 0)
            yield break;
        action ??= actionManager.CurrentAction; //选择行动
        Debug.Log($"NPC {name} ProsessSchedule {action.Name}");

        //如果想要当前行动与上一个行动不同，播放行动的气泡
        if(action != lastAction)
            StartCoroutine(ShowPopUpCoroutin(action));
        Interactable target = action.target;
        if (target is Device device)
            owner.CheckSocialNeedAndPostTask(device);//检查社交需求
        //设置topicHobby
        socialManager.topicHobbyId = action.theme;
        //向目标位置移动,边走边找，找到就停下
        yield return StartCoroutine(MoveToInteractable(target));
        //是否发现了目标
        if(!isTargetInRader) 
            yield break;
        //尝试宣称,如果失败就记住这个暂时不能用，等一阵子后再看
        if(!target.TryClaimedBy(owner))
        {
            action.WaitForAvailable();
            //重选可交互物目标
            action.ChooseInteractable();
            yield break;
        }
        else
        {
            
        }
        //如果对方是npc，叫住对方
        if(target is NPC npc)
            NotifyTheTarget(npc);
        //移动到交互点
        yield return StartCoroutine(MoveToInteractable(target));
        //是否移动到位了
        if(!hasArrived)
            yield break;
        //面向目标
        FaceTheTarget(target.gameObject);

        yield return null;
        //与目标交互
        if (target is Device d)
        {
            ToggleShowPopUp(action);
            yield return StartCoroutine(action.ProcessNPC());
            ToggleShowPopUp();
        }      
        if (target is Character character)
        {
            ToggleShowPopUp(action);
            yield return StartCoroutine(StartConv(character));
            owner.lifeController.AddAttributeByID(action.theme,action.output);
            ToggleShowPopUp();
        }
        //结束时取消宣称
        target.DeclaimedBy(owner);
        //更新lastAction
        lastAction = action;
    }

    public void FaceTheTarget(GameObject target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        animator.SetFloat("X",Mathf.RoundToInt(direction.x));
        animator.SetFloat("Y",Mathf.RoundToInt(direction.y));
    }

    public IEnumerator MoveTo(GameObject gameObj)
    {
        StopAllCoroutines();
        AnimateStopMove();
        Interactable target = gameObj.GetComponent<Interactable>();
        List<AStar.Node> path = astar.FindPath(transform.position, gameObj.transform.position);
        if (path != null)
        {
            for (int i = 0; i < path.Count - target.interactDisdence; i++)
            {
                AStar.Node node = path[i];
                Debug.DrawLine(node.worldPosition, node.parent.worldPosition, Color.red, 99f);
                Vector3 direction = (node.worldPosition - transform.position).normalized;
                while( transform.position != node.worldPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position,node.worldPosition,Time.deltaTime*owner.fSpeed);
                    AnimateMove(direction);
                    yield return null;
                }    
            }
            AnimateStopMove();
        }
        FaceTheTarget(gameObj);
    }
    void NotifyTheTarget(NPC target)
    {
        target.StopAllCoroutines();
        target.behaviorManager.AnimateStopMove();
    }

    //边移动到目标边判断目标是否进入视野范围，有就停下
    IEnumerator MoveToInteractable(Interactable target)
    {
        Debug.Log("MoveToObj: "+target);
        List<List<AStar.Node>> listPath = new();
        if(target.interactPoints.Length == 0)
            target.interactPoints = new Vector3[]{Vector3.zero};
        foreach (Vector3 point in target.interactPoints)
        {
            listPath.Add(astar.FindPath(transform.position, target.transform.position + point));           
        }
        List<AStar.Node> path = new();
        path = listPath.OrderByDescending(p => p.Count).LastOrDefault();
   
        if (path != null)
        {
            //Debug.Log(path.Count);
            //沿着node向前移动
            for (int i = 0; i < path.Count - target.interactDisdence; i++)
            {
                AStar.Node node = path[i];
                Debug.DrawLine(node.worldPosition, node.parent.worldPosition, Color.red, 99f);
                Vector3 direction = (node.worldPosition - transform.position).normalized;
                while (transform.position != node.worldPosition)
                {
                    transform.position = Vector3.MoveTowards(transform.position, node.worldPosition, Time.deltaTime * owner.fSpeed);
                    AnimateMove(direction);
                    do
                        yield return null;
                    while (Timer.hasPaused); //timer暂停时禁止移动 
                }
                //判断目标是否进入视野范围，有就停下
                if(!isTargetInRader)
                {
                    //Debug.Log("rader.IsInRader(target): " + rader.IsInRader(target));
                    if(rader.IsInRader(target))
                    {
                        isTargetInRader = true;
                        AnimateStopMove();
                        yield break;
                    } 
                }
            }
            //判断目标是否进入视野范围，原地到达情况，没进for循环也要检查一次
            if(!isTargetInRader)
            {
                if(rader.IsInRader(target))
                    isTargetInRader = true;
            }
            hasArrived = true;
            AnimateStopMove();
        }
        else
            Debug.LogWarning("no path!"); 
    }
    private IEnumerator MoveToInteractPoint(Interactable target)
    {
        yield return null;
    }
    public void OnRaderEnter(Character target)
    {
        StartCoroutine(RunInto(target));
    }
    public IEnumerator RunInto(Character target)
    {
        StopAllCoroutines();
        AnimateStopMove();
        //如果对方就是目标，普通交互
        if(target == actionManager.CurrentAction.target)
        {
            StartCoroutine(StartConv(target));
            yield break;
        }
        //如果对方不是目标
        else
        {
            //如果对方忙碌
            if(target.IsBusy)
            {
                //如果与对方价值观契合，是朋友，普通交互
                if(socialManager.GetAttitudeTowards(target).InterestsAlignment >= 30)
                {
                    StartCoroutine(StartConv(target));
                    yield break;
                }
                //否则打招呼
                else
                    StartCoroutine(Greeting());
            }
            //如果对方不忙碌，说明是路上遇见，打招呼
            else
            {
                StartCoroutine(Greeting());
            }
                
        }
        yield return StartCoroutine(target.Interact(owner));
        yield return StartCoroutine(ProsessSchedule());
    }

    void Awake()
    {
        owner = GetComponent<NPC>();
        animator = owner.animator;
        popUpAnimator = owner.popUpAnimator;
        astar = GetComponent<AStar>();
        actionManager = owner.actionManager;
        socialManager = owner.socialManager;
        rader = owner.rader;
    }

    void OnEnable()
    {
        StartCoroutine(AIBehaviorCoroutine());
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
