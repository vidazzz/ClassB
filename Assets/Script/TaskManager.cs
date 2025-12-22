using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class TaskManager : MonoBehaviour
{
    Character character;
    public List<Task> tasks = new();
    [Serializable]
    public class TaskData
    {
        //public Character owner;
        public string taskName;
        public Interactable interactable;
        public int duration;
    }

    [Serializable]
    public class Task
    {
        public string taskName;
        private Character owner;
        private TaskManager taskManager;
        private Interactable interactable;
        private Action.InteractableData devicePriority;
        private Timer.Date deadLine;
        private float priorityValue = 20;

        public Task(Character character,TaskData taskData)
        {
            owner = character;
            taskName = taskData.taskName;
            SetupDevicePriority();
            taskManager = owner.GetComponent<TaskManager>();
            interactable = taskData.interactable;
            deadLine = Timer.Time + taskData.duration;
            CheckAndAddPriorityTask();
            EventManager.Instance.OnNextFrame += CheckIfDeadLine;
        }

        private void CheckIfDeadLine()
        {
            if (Timer.Time >= deadLine)
                CloseTask();
        }

        private void CloseTask()
        {
            RemovePriorityTask();
            EventManager.Instance.OnNextFrame -= CheckIfDeadLine;
            taskManager.tasks.Remove(this);
            Debug.Log($"{owner}'s 任务 {taskName} 完成");
        }

        private void SetupDevicePriority()
        {
            Action action = owner.actionManager.actions.Find(a => a.interactables.Contains(interactable as Device));
            if (action != null)
            {
                Action.InteractableData devicePriority = action.InteractableDataList.Find(dp => dp.interactable == (interactable as Device));
                if (devicePriority != null)
                {
                    this.devicePriority = devicePriority;
                }
            }
        }
        private void CheckAndAddPriorityTask()
        { 
            devicePriority.priorityTask += priorityValue;
        }
        private void RemovePriorityTask()
        {
            devicePriority.priorityTask -= priorityValue;
        }
    }

    public void AcceptTask(TaskData taskData)
    {
        Task newTask = new(character, taskData);
        tasks.Add(newTask);
    }

    private void Awake()
    {
        character = GetComponent<Character>();
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
