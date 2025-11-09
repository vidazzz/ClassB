using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
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
        //任务单
        public class Task: Operation
        {
            public string taskName;
            public Module module;
            public bool isAssigned;
            public Timer.Date startTime;
            public Timer.Date deadLine;
            public Timer.Date actualStartTime;
            public Timer.Date actualEndTime;
            protected void Complete()
            {
                isCompleted = true;
                actualEndTime = Timer.Time;
                Debug.Log($"{taskName} completed");
            }
            public Task(string taskName, Module module)
            {
                this.taskName = taskName;
                this.module = module;
                checkItems = new List<string>
                {
                    "Act"
                };
                isAssigned = false;
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
                    Task task = new($"{module.moduleName}-Task {j + 1}", module);
                    if (j == taskCount - 1)
                    {
                        task.maxProgress = remainingModuleWorkload;
                    }
                    else
                    {
                        task.maxProgress = Random.Range(remainingModuleWorkload * 0.2f, remainingModuleWorkload * 0.5f);
                        remainingModuleWorkload -= task.maxProgress;
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
                module.tasks.Sort((a, b) => b.maxProgress.CompareTo(a.maxProgress));
                foreach(Task task in module.tasks)
                {
                    //成员工时升序排列
                    module.group.members.Sort((a, b) => a.workManager.workTime.CompareTo(b.workManager.workTime));
                    //将工作量最高的任务分配给当前工时最短的成员
                    Character member = module.group.members[0];
                    member.workManager.tasks.Add(task);
                    task.owner = member;
                    Debug.Log(member.name + " take " + task.taskName);
                    float duration = task.maxProgress / member.workManager.workEfficiency;
                    task.startTime = startTime + (int)Mathf.Ceil(member.workManager.workTime);
                    task.deadLine = task.startTime + (int)Mathf.Ceil(duration);
                    member.workManager.workTime = task.deadLine.ToMinutes();
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
        
    }
    // Start is called before the first frame update
    void Start()
    {
        project = new Project("Project A", 1000, 0, 7, departments[0].groups);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
