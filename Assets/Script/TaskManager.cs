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
        public string needName;
        public float targetValue;
        public List<Interactable> interactables;
        public Timer.Date deadLine;
    }

    [Serializable]
    public class Task
    {
        public string taskName;
        Character owner;
        TaskManager taskManager;
        Need targetNeed;
        float originStatValue;
        float targetValue;
        List<Interactable> interactables = new();
        Timer.Date deadLine;//hh,mm
        float priorityValue = 20;

        public Task(Character character,TaskData taskData)
        {
            owner = character;
            taskName = taskData.taskName;
            taskManager = owner.GetComponent<TaskManager>();
            targetNeed = owner.lifeController.GetNeed(taskData.needName);
            originStatValue = targetNeed.value;
            targetValue = taskData.targetValue;
            interactables = taskData.interactables;
            deadLine = taskData.deadLine;
            CheckAndAddPriorityTask();
            EventManager.Instance.OnMutifyNeeds += CheckIfFinished;
            EventManager.Instance.OnNextFrame += CheckIfDeadLine;
        }

        private void CheckIfDeadLine()
        {
            if (Timer.Instance.HH >= deadLine.hh && Timer.Instance.MM >= deadLine.mm)
                CloseTask();
        }
        private void CheckIfFinished()
        {
            if (targetNeed.value >= targetValue + originStatValue)
                CloseTask();
        }

        private void CloseTask()
        {
            RemovePriorityTask();
            EventManager.Instance.OnMutifyNeeds -= CheckIfFinished;
            EventManager.Instance.OnNextFrame -= CheckIfDeadLine;
            taskManager.tasks.Remove(this);
            Debug.Log($"{owner}'s 任务 {taskName} 完成");
        }

        private void CheckAndAddPriorityTask()
        {
            if (owner is NPC npc)
                foreach (Action action in npc.actions)
                {
                    if (interactables.Contains(action.target))
                    {
                        action.priorityTask += priorityValue;
                    }
                }
        }
        private void RemovePriorityTask()
        {
            if (owner is NPC npc)
                foreach (Action action in npc.actions)
                {
                    if (interactables.Contains(action.target))
                    {
                        action.priorityTask -= priorityValue;
                    }
                }
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
