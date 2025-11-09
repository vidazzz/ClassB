using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
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
    private static int oneSecondInGame = 5;
    static public float originTimeScale;
    static private int dd;
    static private int hh;
    static private float mm;
    static private float ss;
    public struct Date : IComparable<Date>
    {
        public int dd;
        public int hh;
        public int mm;
        public Date(int dd, int hh = 0, int mm = 0)
        {
            this.dd = dd;
            this.hh = hh;
            this.mm = mm;
        }
        // 重载+运算符
        public static Date operator +(Date a, Date b)
        {
            int minute = a.mm + b.mm;
            int hour = a.hh + b.hh + minute / 60;
            int day = a.dd + b.dd + hour / 24;
            minute %= 60;
            hour %= 24;
            return new Date(day, hour, minute);
        }
        public static int operator -(Date a, Date b)
        {
            int aMinute = a.dd * 24 * 60 + a.hh * 60 + a.mm;
            int bMinute = b.dd * 24 * 60 + b.hh * 60 + b.mm;

            return aMinute - bMinute;
        }
        public static Date operator +(Date a, int b)
        {
            int minute = a.mm + b;
            int hour = a.hh + minute / 60;
            int day = a.dd + hour / 24;
            minute %= 60;
            hour %= 24;
            return new Date(day, hour, minute);
        }
        public static bool operator >(Date a, Date b)
        {
            int aMinute = a.dd * 24 * 60 + a.hh * 60 + a.mm;
            int bMinute = b.dd * 24 * 60 + b.hh * 60 + b.mm;
            return (aMinute > bMinute);
        }

        public static bool operator <(Date a, Date b)
        {
            int aMinute = a.dd * 24 * 60 + a.hh * 60 + a.mm;
            int bMinute = b.dd * 24 * 60 + b.hh * 60 + b.mm;
            return (aMinute < bMinute);
        }
        public static bool operator <=(Date a, Date b)
        {
            int aMinute = a.dd * 24 * 60 + a.hh * 60 + a.mm;
            int bMinute = b.dd * 24 * 60 + b.hh * 60 + b.mm;
            return (aMinute <= bMinute);
        }

        public static bool operator >=(Date a, Date b)
        {
            int aMinute = a.dd * 24 * 60 + a.hh * 60 + a.mm;
            int bMinute = b.dd * 24 * 60 + b.hh * 60 + b.mm;
            return (aMinute >= bMinute);
        }

        public static bool operator ==(Date a, Date b)
        {
            int aMinute = a.dd * 24 * 60 + a.hh * 60 + a.mm;
            int bMinute = b.dd * 24 * 60 + b.hh * 60 + b.mm;
            return (aMinute == bMinute);
        }

        public static bool operator !=(Date a, Date b)
        {
            return !(a == b);
        }

        public override readonly bool Equals(object obj)
        {
            if (!(obj is Date))
                return false;
            Date other = (Date)obj;
            return this == other;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(dd, hh, mm);
        }

        public readonly int CompareTo(Date other)
        {
            int total = dd * 24 * 60 + hh * 60 + mm;
            int otherTotal = other.dd * 24 * 60 + other.hh * 60 + other.mm;
            if (total < otherTotal) return -1;
            if (total > otherTotal) return 1;
            return 0;
        }

        public readonly int ToMinutes()
        {
            //Debug.Log($"{dd} days, {hh} hours, {mm} minutes to total minutes");
            return (int)Mathf.Ceil(dd * 24 * 60 + hh * 60 + mm);     
        }
    }
    public static Date Time
    {
        get { return new Date(dd, hh, (int)mm); }
    }
    private static float deltaTime; //游戏中计时器timer的每帧时间，单位是游戏中的秒
    public static float DeltaTime{
        get {return deltaTime;}
    }
    public bool isOffWork = false;
    private bool hasDayEnded = false;

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
            deltaTime = UnityEngine.Time.deltaTime * oneSecondInGame;
            ss += deltaTime;
            
            EventManager.Instance.NextFrame(); //下一帧
            if (!CoroutineQueueManager.nextFrameCoroutineQueue.IsQueueEmpty)
                yield return StartCoroutine(CoroutineQueueManager.nextFrameCoroutineQueue.ProcessQueue()); //下一帧
            if (!CoroutineQueueManager.firstQuitTypingGameCoroutineQueue.IsQueueEmpty)
                yield return StartCoroutine(CoroutineQueueManager.firstQuitTypingGameCoroutineQueue.ProcessQueue()); //处理第一次退出打字游戏的协程队列
            if (ss >= 60)
            {
                mm++;
                ss = 0;
            }
                
            if (mm >=60)
            {
                hh ++;
                mm = 0;
                EventManager.Instance.HourEnd();
            }
            if (hh >= 9) //上班时间
            {
                if (hasDayEnded) //新的一天开始
                {
                    EventManager.Instance.DayBegin(dd);
                    yield return StartCoroutine(CoroutineQueueManager.dayBeginCoroutineQueue.ProcessQueue());
                    if (hadQuitTypingGame) //如果退出过打字界面
                    {
                        EventManager.Instance.DayBegin2(dd);
                        yield return StartCoroutine(CoroutineQueueManager.dayBeginCoroutineQueue2.ProcessQueue());
                    }
                    hasDayEnded = false;
                    isOffWork = false;
                }
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
        displayText.text = $"DAY{dd}\t{string.Format("{0:00}:{1:00}:{2:00}", hhr, (int)mm, (int)ss)}";
    }

    public static void Pause(float newTimeScale = 0)
    {
        if (hasPaused)
            return;
        Debug.Log("Pause");
        //deltaTime = 0;
        if (newTimeScale > 0)
            originTimeScale = newTimeScale;
        else
            originTimeScale = UnityEngine.Time.timeScale;
        UnityEngine.Time.timeScale = 0;
        pauseIcon.SetActive(true);
        hasPaused = true;
    }
    public static void Resume(float newTimeScale = 0)
    {
        if(!hasPaused)
            return;
        Debug.Log("Resume");
        if (newTimeScale > 0)
            originTimeScale = newTimeScale;
        UnityEngine.Time.timeScale = originTimeScale;
        pauseIcon.SetActive(false);
        hasPaused = false;
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
