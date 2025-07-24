using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
public delegate void timerDelegate();
public class Timer : MonoBehaviour
{
    private int dd;
    private int hh;
    private float mm;
    private static int deltaTime; //游戏中计时器timer的每帧时间，单位时游戏中的分
    public static int DeltaTime{
        get {return deltaTime;}
    }
    public bool isOffWork = false;
    private bool hasDayEnded = false;
    [SerializeField]
    private static int oneSecondInGame = 4;
    public TextMeshProUGUI displayText;
    public static bool hasPaused;
    public static CoroutineQueueManager dayBeginCoroutineQueueManager;
    public static CoroutineQueueManager theVaryBeginingCoroutineQueueManager;
    public static CoroutineQueueManager dayEndCoroutineQueueManager;
    public static CoroutineQueueManager nextFrameCoroutineQueueManager;
    public static CoroutineQueueManager offWorkCoroutineQueueManager;
    public static timerDelegate onTheVaryBegining;
    public static timerDelegate onDayBegin;
    public static timerDelegate onDayEnd;
    public static timerDelegate onDayEnd2;

    public static timerDelegate onHourEnd;
    public static timerDelegate onStartWork;
    public static timerDelegate onOffWork;
    public static timerDelegate onOffWork2;
    public static timerDelegate onNextFrame;
    [HideInInspector]
    public static bool shouldOffWorkNow;
    [HideInInspector]
    public static bool hadQuitTypingGame; //用于标记是否退出过打字界面
    private static GameObject pauseIcon;

    public Queue<IEnumerator> dayBeginCoroutineQueue = new();
    
    IEnumerator ClockCoroutine()
    {
        //计时使用工作时钟 9-33
        //显示使用正常时钟 0-24
        yield return new WaitForSeconds(0.5f);
        onTheVaryBegining?.Invoke(); //一切的开始                      
        yield return StartCoroutine(theVaryBeginingCoroutineQueueManager.ProcessQueue()); //一切的开始
        while(true)
        {
            deltaTime = 0;
            while (hasPaused)
                yield return null;
            mm += Time.deltaTime * oneSecondInGame * Hero.Instance.lifeController.TimeMultiplier;
            onNextFrame?.Invoke(); //下一帧
            if(!nextFrameCoroutineQueueManager.IsQueueEmpty)
                yield return StartCoroutine(nextFrameCoroutineQueueManager.ProcessQueue()); //下一帧
            
            
            if(hh >= 9) //上班时间
            {
                if(hasDayEnded) //新的一天开始
                {
                    onDayBegin?.Invoke();
                    yield return StartCoroutine(dayBeginCoroutineQueueManager.ProcessQueue());
                    hasDayEnded = false;
                    isOffWork = false;
                }
            }
            if(mm >=60)
            {
                hh ++;
                mm = 0;
                onHourEnd?.Invoke();
            }
            if(hh == 18 && !isOffWork) //下班时间
            {
                isOffWork = true;
                onOffWork?.Invoke();
                yield return StartCoroutine(offWorkCoroutineQueueManager.ProcessQueue());
                onOffWork2?.Invoke();
            }
            if(hh >= 26) //凌晨2点
            {  
                if(!hasDayEnded) //如果还没有结束一天的日程，就在这里强制结束
                {
                    yield return StartCoroutine(EndTheDay());
                }
            }
            if(shouldOffWorkNow) //那现在就下班！
            {
                shouldOffWorkNow = false;
                yield return StartCoroutine(EndTheDay());
            }
            //if(hh >= 24)
            yield return null;
        }
    }
    public IEnumerator EndTheDay()
    {
        hasDayEnded = true;
        onDayEnd?.Invoke();
        onDayEnd2?.Invoke();
        hh = 9;
        dd ++;  
        yield return StartCoroutine(dayEndCoroutineQueueManager.ProcessQueue());
    }
    void DisplayClock()
    {
        int hhr = hh >= 24 ? hh-24 : hh;//上班时钟转换成正常时钟
        displayText.text = "DAY"+dd+"\t" + hhr + ":" + (int)mm;
    }

    public static void Pause()
    {
        if(hasPaused)
            return;
        deltaTime = 0;
        pauseIcon.SetActive(true);
        hasPaused = true;
    }
    public static void Resume()
    {
        if(!hasPaused)
            return;
        pauseIcon.SetActive(false);
        hasPaused = false;
    }
    
    void OnTheVaryBegining()
    {   
        StartCoroutine(CutSceneController.Instance.ExecuteCoroutines(0)); //由于需要这个打字自行运转，不能在timer协程中yield return，所以新开一个协程并行处理
    }
    void AddNextFrameCoroutines() //装填协程进入队列
    {
        
    }
    void AddVaryBeginingCoroutine()
    {
    }
    void AddDayBeginCoroutine()
    {
        dayBeginCoroutineQueueManager.AddCoroutine(Blackout.Instance.FadeInOrOutCoroutine());
        if(hadQuitTypingGame)//退出过打字界面才有后面的交互
            dayBeginCoroutineQueueManager.AddCoroutine(CutSceneController.Instance.ExecuteCoroutines(2));
    }
    void AddOffWorkCoroutine()
    {
        if(hadQuitTypingGame)//退出过打字界面才有后面的交互
        {
            offWorkCoroutineQueueManager.AddCoroutine(CutSceneController.Instance.ExecuteCoroutines(3));
        }
        else
            offWorkCoroutineQueueManager.AddCoroutine(EndTheDay());
            
    }
    void AddDayEndCoroution()
    {
        dayEndCoroutineQueueManager.AddCoroutine(Blackout.Instance.FadeInOrOutCoroutine());
        dayEndCoroutineQueueManager.AddCoroutine(Blackout.Instance.DisplayText()); //一天结束后的旁白
    }

    public static void SetOneSecondInGame(int value)
    {
        oneSecondInGame = value;
    }


    void Awake()
    {
        dayBeginCoroutineQueueManager = new();
        theVaryBeginingCoroutineQueueManager = new();
        offWorkCoroutineQueueManager = new();
        dayEndCoroutineQueueManager = new();
        nextFrameCoroutineQueueManager = new();
        onTheVaryBegining += OnTheVaryBegining;
        onTheVaryBegining += AddVaryBeginingCoroutine;
        onDayBegin += AddDayBeginCoroutine;
        onOffWork += AddOffWorkCoroutine;
        onDayEnd += AddDayEndCoroution;

        pauseIcon = GameObject.Find("PauseIcon");
        pauseIcon.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        mm = DateTime.Now.Minute;
        hh = DateTime.Now.Hour < 9 ? DateTime.Now.Hour + 24 : DateTime.Now.Hour;
        StartCoroutine(ClockCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        DisplayClock();
    }

    void OnDisable()
    {
        onTheVaryBegining -= OnTheVaryBegining;
        onTheVaryBegining -= AddVaryBeginingCoroutine;
        onDayBegin -= AddDayBeginCoroutine;
        onOffWork -= AddOffWorkCoroutine;
        onDayEnd -= AddDayEndCoroution;
    }
}
