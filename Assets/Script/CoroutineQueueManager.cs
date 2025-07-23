using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineQueueManager
{
    private Queue<IEnumerator> coroutineQueue = new();
    private bool isProcessing = false;
    public bool isProcessingValue {
        get {return isProcessing;}
    }

    // 添加协程到队列
    public void AddCoroutine(IEnumerator coroutine)
    {
        coroutineQueue.Enqueue(coroutine);
    }

    // 处理队列中的协程
    public IEnumerator ProcessQueue()
    {
        Timer.Pause();
        isProcessing = true;
        while (coroutineQueue.Count > 0)
        {
            // 取出并执行队列中的下一个协程
            yield return coroutineQueue.Dequeue();
        }
        isProcessing = false;
        Timer.Resume();
    }
}