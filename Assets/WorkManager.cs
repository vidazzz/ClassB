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
    private Company.Project.Task currentTask;
    public Company.Project.Task CurrentTask
    { 
        get
        {
            if (currentTask == null)
                return ChooseTask();
            else
                return currentTask;
        }
    }
    public Company.Project.Task ChooseTask()
    {
        if(tasks.Count == 0)
        {
            Debug.Log(gameObject + " No tasks!");
            return null;
        }
        var candidates = tasks.Where(t => !t.isCompleted && t.isAssigned).ToList();
        if (candidates.Count == 0)
        {
            Debug.Log("No available tasks!");
            return null;
        }
        return candidates.OrderBy(t => t.startTime).First();
    }
    public IEnumerator Process(int workTime)
    {
        if (CurrentTask == null)
        {
            Debug.LogError("No task assigned!");
            yield return null;
        }
        Timer.Date beginTime = Timer.Time;
        while (Timer.Time - beginTime < workTime)
        {
            if (CurrentTask.isCompleted)
            {
                ChooseTask();
                if (CurrentTask == null)
                {
                    Debug.Log("All tasks completed!");
                    yield break;
                }
                beginTime = Timer.Time;
            }
            CurrentTask.ProcessHero();
            yield return null;
        }
    }
    void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        //工作效率等于act天赋的数值
        Character.TalentData talentData = (Character.TalentData)GetComponent<Character>().GetTalent("Act");
        workEfficiency = talentData.value;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
