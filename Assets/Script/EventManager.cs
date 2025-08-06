using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private static EventManager instance;
    
    public static EventManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EventManager>();
            }
            return instance;
        }
    }

    public event Action OnTheVaryBegining;
    public event Action OnNextFrame;
    public event Action<int> OnDayBegin;
    public event Action<int> OnDayBegin2;
    public event Action OnHourEnd;
    public event Action OnOffWork;
    public event Action OnOffWork2;
    public event Action OnDayEnd;
    public event Action OnDayEnd2;
    public event Action OnFirstQuitTypingGame;

    public void TheVaryBegining()
    {
        OnTheVaryBegining?.Invoke();
        // Additional logic for the vary beginning can be added here
    }
    public void DayBegin(int invokeDay)
    {
        OnDayBegin?.Invoke(invokeDay);
        // Additional logic for day begin can be added here
    }
    public void DayBegin2(int invokeDay)
    {
        OnDayBegin2?.Invoke(invokeDay);
        // Additional logic for day begin 2 can be added here
    }

    public void HourEnd()
    {
        OnHourEnd?.Invoke();
        // Additional logic for hour end can be added here
    }
    public void OffWork()
    {
        OnOffWork?.Invoke();
        // Additional logic for off work can be added here
    }
    public void OffWork2()
    {
        OnOffWork2?.Invoke();
        // Additional logic for off work 2 can be added here
    }
    public void DayEnd()
    {
        OnDayEnd?.Invoke();
        // Additional logic for day end can be added here
    }
    public void DayEnd2()
    {
        OnDayEnd2?.Invoke();
        // Additional logic for day end 2 can be added here
    }
    public void NextFrame()
    {
        OnNextFrame?.Invoke();
        // Additional logic for next frame can be added here
    }
    public void FirstQuitTypingGame()
    {
        OnFirstQuitTypingGame?.Invoke();
        // Additional logic for first quit typing game can be added here
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
