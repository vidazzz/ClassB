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

    public event Action onTheVaryBegining;
    public event Action onNextFrame;
    public event Action onDayBegin;
    public event Action onDayBegin2;
    public event Action<int> onDayN_Begin;
    public event Action onHourEnd;
    public event Action onOffWork;
    public event Action onOffWork2;
    public event Action onDayEnd;
    public event Action onDayEnd2;
    public event Action onFirstQuitTypingGame;

    public void TheVaryBegining()
    {
        onTheVaryBegining?.Invoke();
        // Additional logic for the vary beginning can be added here
    }
    public void DayBegin()
    {
        onDayBegin?.Invoke();
        // Additional logic for day begin can be added here
    }
    public void DayBegin2()
    {
        onDayBegin2?.Invoke();
        // Additional logic for day begin 2 can be added here
    }

    public void DayN_Begin(int dayNumber)
    {
        onDayN_Begin?.Invoke(dayNumber);
        // Additional logic for day N begin can be added here
    }
    public void HourEnd()
    {
        onHourEnd?.Invoke();
        // Additional logic for hour end can be added here
    }
    public void OffWork()
    {
        onOffWork?.Invoke();
        // Additional logic for off work can be added here
    }
    public void OffWork2()
    {
        onOffWork2?.Invoke();
        // Additional logic for off work 2 can be added here
    }
    public void DayEnd()
    {
        onDayEnd?.Invoke();
        // Additional logic for day end can be added here
    }
    public void DayEnd2()
    {
        onDayEnd2?.Invoke();
        // Additional logic for day end 2 can be added here
    }
    public void NextFrame()
    {
        onNextFrame?.Invoke();
        // Additional logic for next frame can be added here
    }
    public void FirstQuitTypingGame()
    {
        onFirstQuitTypingGame?.Invoke();
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
