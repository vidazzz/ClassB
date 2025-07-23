using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Test : MonoBehaviour
{
 private List<IEnumerator> coroutineList = new List<IEnumerator>();

       // 可在Inspector中配置的事件
    public UnityEvent<int> onMessageReceived;

    public void SendMessage(int message)
    {
        // 触发事件
        onMessageReceived.Invoke(message);
    }
}
