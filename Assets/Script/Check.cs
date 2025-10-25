using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check
{
    public List<CheckItem> checkItems = new();
    public Interactable owner;
    public Character challenger;
    public State state;
    public enum State
    {
        checking,
        completed,
        runOutOfTime,
    }
    public int maxValue;
    public int currentValue;
    public int deltaValue;
    private float originTimeScale;
    public float timeScaleMultiplier = 20f;
    Timer.Date deadLine;
    int maxTime;
    int duration = 10;
    //检查项
    public class CheckItem
    {
        public string name;
        public int checkResult;
        public string description;
        public Interactable owner;
        public Character challenger;
        public object checkObj;

        public CheckItem(string objName,Interactable owner, Character challenger)
        {
            name = objName;
            this.owner = owner;
            this.challenger = challenger;
            checkObj = FindCheckObj(objName);
            Debug.Log($"FindCheckObj {checkObj}");
            Process();
        }

        private object FindCheckObj(string objName)
        {
            object result;
            result = challenger.lifeController.GetStat(objName);
            result ??= challenger.GetTalent(objName);
            result ??= challenger.GetHobbyData(objName);
            if (result == null)
            {
                if (objName == "affinity")
                    result = Community.affinity;
            }
            if (result == null)
                Debug.LogError("can't find checkObj!");
            return result;
        }

        public void Process()
        {
            float result = 0;
            switch (checkObj)
            {
                case Stat stat:
                    result = stat.value;
                    break;
                case Character.TalentData talentData:
                    result = talentData.value;
                    break;
                case Character.HobbyData hobbyData:
                    result = hobbyData.passion;
                    break;
                case Community.Affinity affinity:
                    result = affinity.GetAffinity(owner as Character,challenger);
                    break;
                default:
                    UnityEngine.Debug.LogError("invalid checkObj type!");
                    break;
            }
            checkResult = (int)result;
        }
    }

    public Check(List<string> objNames, Interactable owner, Character challenger, int maxValue = 100, int maxTime = 0)
    {
        this.owner = owner;
        this.challenger = challenger;
        this.maxValue = maxValue;
        originTimeScale = Timer.originTimeScale;
        deadLine = Timer.Time + maxTime;
        checkItems = new();
        foreach (string name in objNames)
        {
            CheckItem newCheckItem = new(name, this.owner, this.challenger);
            checkItems.Add(newCheckItem);
        }
    }

    public void UpdateDeltaValue()
    {
        int sum = 0;
        foreach (CheckItem checkItem in checkItems)
        {
            sum += checkItem.checkResult;
        }
        deltaValue += sum;
    }
    public IEnumerator Process()
    {
        UpdateDeltaValue();
        Timer.Resume(originTimeScale * timeScaleMultiplier);//加速
        
        Timer.Date beginTime = Timer.Time;
        while (Timer.Time < beginTime + duration)
        {
            yield return new WaitForSeconds(0.5f);
            if (maxTime > 0)
            {
                if (Timer.Time >= deadLine)
                {
                    RunOutOfTime();
                    yield break;
                }
            }
        }

        Timer.Pause(originTimeScale);

        currentValue += deltaValue;
        if (currentValue >= maxValue)
        {
            currentValue = maxValue;
            Completed();
        }
    }

    void Completed()
    {
        state = State.completed;
    }

    void RunOutOfTime()
    {
        state = State.runOutOfTime;
    }

}
