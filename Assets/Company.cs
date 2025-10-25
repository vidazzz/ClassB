using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

public class Company : MonoBehaviour
{
    private static Company _instance; //单例
    public static Company Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Company>();
            }
            return _instance;
        }
    }
    public List<Character> menbers;
    public Project project;
    public List<Department> departments;
    [System.Serializable]
    public class Project
    {
        public string projectName;
        public Timer.Date startTime;
        public Timer.Date duration;
        public float workload;
        public float progress;
        public List<Module> modules;
        public Project(string projectName, float workload,int startDay, int durationInDays, List<Group> groups)
        {
            this.projectName = projectName;
            this.workload = workload;
            startTime = new Timer.Date(startDay);
            duration = new Timer.Date(durationInDays);
            progress = 0;
            BreakDown(groups);
            PM(); 
        }
        [System.Serializable]
        public class Module
        {
            public string moduleName;
            public float workload;
            public Group group;
            public List<Task> tasks;
        }
        [System.Serializable]
        public class Task
        {
            public string taskName;
            public Module module;
            public bool isAssigned;
            public bool isCompleted;
            public float workload;
            public float completedWorkload;
            public float progress { get { return completedWorkload / workload; } }
            public Timer.Date startTime;
            public Timer.Date endTime;
            public Timer.Date actualStartTime;
            public Timer.Date actualEndTime;
            public void Process(float workDone)
            {
                if(isCompleted)
                {
                    Debug.LogWarning($"{taskName} is already completed!");
                    return;
                }
                completedWorkload += workDone;
                if (completedWorkload >= workload)
                {
                    Complete();
                }
            }
            public void Complete()
            {
                isCompleted = true;
                actualEndTime = Timer.Time;
                Debug.Log($"{taskName} completed");
            }

        }
        public void BreakDown(List<Group> groups)
        {
            modules = new List<Module>();
            int moduleCount = groups.Count;
            float remainingWorkload = workload;
            for (int i = 0; i < moduleCount; i++)
            {
                Module module = new()
                {
                    moduleName = $"Module {i + 1}",
                    group = groups[i]
                };
                if (i == moduleCount - 1)
                {
                    module.workload = remainingWorkload;
                }
                else
                {
                    module.workload = Random.Range(remainingWorkload * 0.2f, remainingWorkload * 0.5f);
                    remainingWorkload -= module.workload;
                }
                modules.Add(module);
            }
            foreach (Module module in modules)
            {
                module.tasks = new List<Task>();
                int minTackCount = module.group.members.Count;
                int taskCount = Random.Range(minTackCount, minTackCount + 4);
                float remainingModuleWorkload = module.workload;
                for (int j = 0; j < taskCount; j++)
                {
                    Task task = new()
                    {
                        taskName = $"{module.moduleName}-Task {j + 1}",
                        module = module
                    };
                    if (j == taskCount - 1)
                    {
                        task.workload = remainingModuleWorkload;
                    }
                    else
                    {
                        task.workload = Random.Range(remainingModuleWorkload * 0.2f, remainingModuleWorkload * 0.5f);
                        remainingModuleWorkload -= task.workload;
                    }
                    module.tasks.Add(task);
                }
            }
        }
        public void PM()
        {
            foreach(Module module in modules)
            {
                //重制成员任务单
                foreach(Character member in module.group.members)
                {
                    member.workManager.tasks.Clear();
                    member.workManager.workTime = 0;
                }
                //任务工作量降序排列
                module.tasks.Sort((a, b) => b.workload.CompareTo(a.workload));
                foreach(Task task in module.tasks)
                {
                    //成员工时升序排列
                    module.group.members.Sort((a, b) => a.workManager.workTime.CompareTo(b.workManager.workTime));
                    //将工作量最高的任务分配给当前工时最短的成员
                    Character member = module.group.members[0];
                    member.workManager.tasks.Add(task);
                    task.startTime = startTime + (int)member.workManager.workTime;
                    member.workManager.workTime += task.workload / member.workManager.workEfficiency;
                    task.isAssigned = true;
                }
            }
        }
    }
    [System.Serializable]
    public class Department
    {
        public string departmentName;
        public List<Group> groups;
    }
    [System.Serializable]
    public class Group
    {
        public string groupName;
        public List<Character> members;
        public Project.Module module;
    }

    void Awake()
    {
        project = new Project("Project A", 1000, 30, 1, departments[0].groups);
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
