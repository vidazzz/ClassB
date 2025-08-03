using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
public delegate void timerDelegate();
public delegate void timerDelegateWithInt(int value);
public class Timer : MonoBehaviour
{
    private static Timer instance;
    
    public static Timer Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Timer>();
            }
            return instance;
        }
    }
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
        EventManager.Instance.TheVaryBegining(); //一切的开始
        yield return StartCoroutine(CoroutineQueueManager.theVaryBeginingCoroutineQueue.ProcessQueue()); //一切的开始
        while(true)
        {
            deltaTime = 0;
            while (hasPaused)
                yield return null;
            mm += Time.deltaTime * oneSecondInGame * Hero.Instance.lifeController.TimeMultiplier;
            EventManager.Instance.NextFrame(); //下一帧
            if (!CoroutineQueueManager.nextFrameCoroutineQueue.IsQueueEmpty)
                yield return StartCoroutine(CoroutineQueueManager.nextFrameCoroutineQueue.ProcessQueue()); //下一帧
            if (!CoroutineQueueManager.firstQuitTypingGameCoroutineQueue.IsQueueEmpty)
                yield return StartCoroutine(CoroutineQueueManager.firstQuitTypingGameCoroutineQueue.ProcessQueue()); //处理第一次退出打字游戏的协程队列
            
            if(hh >= 9) //上班时间
            {
                if(hasDayEnded) //新的一天开始
                {
                    EventManager.Instance.DayBegin();
                    yield return StartCoroutine(CoroutineQueueManager.dayBeginCoroutineQueue.ProcessQueue());
                    if(hadQuitTypingGame) //如果退出过打字界面
                    {
                        EventManager.Instance.DayBegin2();
                        yield return StartCoroutine(CoroutineQueueManager.dayBeginCoroutineQueue2.ProcessQueue());
                    }
                    hasDayEnded = false;
                    isOffWork = false;
                }
            }
            if(mm >=60)
            {
                hh ++;
                mm = 0;
                EventManager.Instance.HourEnd();
            }
            if(hh == 18 && !isOffWork) //下班时间
            {
                isOffWork = true;
                EventManager.Instance.OffWork();
                yield return StartCoroutine(CoroutineQueueManager.offWorkCoroutineQueue.ProcessQueue());
                if(hadQuitTypingGame) //如果退出过打字界面
                {
                    EventManager.Instance.OffWork2();
                    yield return StartCoroutine(CoroutineQueueManager.offWorkCoroutineQueue2.ProcessQueue());
                }
                EventManager.Instance.OffWork2();
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

        EventManager.Instance.DayEnd();
        yield return StartCoroutine(CoroutineQueueManager.dayEndCoroutineQueue.ProcessQueue());
        EventManager.Instance.DayEnd2();
        yield return StartCoroutine(CoroutineQueueManager.dayEndCoroutineQueue2.ProcessQueue());

        hh = 9;
        dd ++;
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

    void AddDayN_BeginCoroutine(int invokDay)
    {
        if (invokDay != dd || !hadQuitTypingGame)
            return;
    }
    void AddOffWorkCoroutine()
    {
        CoroutineQueueManager.offWorkCoroutineQueue.AddCoroutine(EndTheDay());        
    }

    public static void SetOneSecondInGame(int value)
    {
        oneSecondInGame = value;
    }


    void Awake()
    {
        CoroutineQueueManager.dayBeginCoroutineQueue = new();
        CoroutineQueueManager.theVaryBeginingCoroutineQueue = new();
        CoroutineQueueManager.offWorkCoroutineQueue = new();
        CoroutineQueueManager.dayEndCoroutineQueue = new();
        CoroutineQueueManager.nextFrameCoroutineQueue = new();
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
    }
}
