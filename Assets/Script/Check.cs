using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class Check
{
    List<CheckItem> checkItems;
    public Interactable Owner;
    public Character challenger;
    int maxValue;
    int currentValue;
    int deltaValue;
    Timer.Date deadLine;
    int maxTime;
    int duration;
    public class CheckItem
    {
        public int result;
        public string description;

        public object checkObj;

        public CheckItem(string itemName)
        {

        }

        public void Process()
        {
            switch (checkObj)
            {
                case Stat stat:
                    CheckStat(stat);
                    break;
                case Hobby hobby:
                    CheckHobby(hobby);
                    break;
                default:
                    break;
            }
        }

        private void CheckStat(Stat stat)
        {
            
        }
        private void CheckHobby(Hobby hobby)
        {

        }
    }

    public void UpdateDeltaValue()
    {
        int sum = 0;
        foreach (CheckItem checkItem in checkItems)
        {
            sum += checkItem.result;
        }
    }
    public IEnumerator Process()
    {
        UpdateDeltaValue();
        currentValue += deltaValue;
        Timer.Date beginTime = Timer.Time;
        while (Timer.Time < beginTime + duration)
        {
            yield return new WaitForSeconds(0.5f);
        }
        if (Timer.Time >= deadLine)
        {
            RunOutOfTime();
            yield break;
        }
        if (currentValue >= maxValue)
        {
            currentValue = maxValue;
            Completed();
        }
    }

    void Completed()
    {

    }

    void RunOutOfTime()
    {

    }

}
