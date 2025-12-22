using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(LifeController))]
public class WorkManager : MonoBehaviour
{
    public float kpi;
    public float requiredKPI;
    public float lastRequiredKPI;//上一次的kpi指标
    public float kpiMultiplier;//加buff时生效的kpi倍率
    public float salary;
    public bool haveFinishWork;//结算时是否完成了当天工作
    public List<Company.Project.Task> tasks = new();
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
    void Awake()
    {
        tasks = new();
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
