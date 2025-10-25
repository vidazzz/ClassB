using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WorkManager : MonoBehaviour
{
    public List<Company.Project.Task> tasks = new();
    public float workEfficiency = 1f;
    public float workTime;//总工时
    public Company.Project.Task currentTask;
    public void ChooseTask()
    {
        Timer.Date minStartTime = tasks.Min(t => t.startTime);
        currentTask = tasks.Find(t => t.startTime <= minStartTime && !t.isCompleted && t.isAssigned);
    }
    public IEnumerator Process(float workEfficiency, int workTime)
    {
        if (currentTask == null)
        {
            Debug.LogError("No task assigned!");
            yield return null;
        }
        Timer.Date beginTime = Timer.Time;
        while(Timer.Time - beginTime < workTime)
        {
            if(currentTask.isCompleted)
            {
                ChooseTask();
                if (currentTask == null)
                {
                    Debug.Log("All tasks completed!");
                    yield break;
                }
                beginTime = Timer.Time;
            }
            currentTask.Process(workEfficiency * Timer.DeltaTime / 60);
            yield return null;
        }
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
