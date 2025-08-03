using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineQueueManager : MonoBehaviour
{
    private static CoroutineQueueManager instance;
    public static CoroutineQueueManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CoroutineQueueManager>();
            }
            return instance;
        }
    }

    public static CoroutineQueue theVaryBeginingCoroutineQueue = new();
    public static CoroutineQueue dayBeginCoroutineQueue = new();
    public static CoroutineQueue dayBeginCoroutineQueue2 = new();//用于退出打字游戏后
    public static CoroutineQueue onHourEndCoroutineQueue = new();
    public static CoroutineQueue dayEndCoroutineQueue = new();
    public static CoroutineQueue dayEndCoroutineQueue2 = new();
    public static CoroutineQueue nextFrameCoroutineQueue = new();
    public static CoroutineQueue offWorkCoroutineQueue = new();
    public static CoroutineQueue offWorkCoroutineQueue2 = new();//用于退出打字游戏后
    public static CoroutineQueue firstQuitTypingGameCoroutineQueue = new();
}

public class CoroutineQueue
{
    private Queue<IEnumerator> queue = new();
    public bool IsQueueEmpty { get { return queue.Count == 0; } }
    private bool isProcessing = false;
    public bool isProcessingValue
    {
        get { return isProcessing; }
    }

    // 添加协程到队列
    public void AddCoroutine(IEnumerator coroutine)
    {
        queue.Enqueue(coroutine);
    }

    // 处理队列中的协程
    public IEnumerator ProcessQueue()
    {
        Timer.Pause();
        isProcessing = true;
        while (queue.Count > 0)
        {
            // 取出并执行队列中的下一个协程
            yield return queue.Dequeue();
        }
        isProcessing = false;
        Timer.Resume();
    }
}