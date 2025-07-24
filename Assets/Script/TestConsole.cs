using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class TestConsole : MonoBehaviour
{
    public GameObject testPanel;
    private DialogueController[] dialogueControllers;
    bool isTestPausing = false;
    public void TestResetDialogue()
    {
        foreach(var dialogueController in dialogueControllers)
        {
            dialogueController.TestClearAllCheck();
        }
    }
    public void TestSetTimepass1()
    {
        Timer.SetOneSecondInGame(4);
    }
        public void TestSetTimepass2()
    {
        Timer.SetOneSecondInGame(60);
    }
        public void TestSetTimepass3()
    {
        Timer.SetOneSecondInGame(120);
    }

    // Start is called before the first frame update
    void Start()
    {
        dialogueControllers = FindObjectsOfType<DialogueController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            testPanel.SetActive(!testPanel.activeSelf);
            if(testPanel.activeSelf)
            {
                if(!Timer.hasPaused)
                {
                    Timer.Pause();
                    isTestPausing = true;
                }      
            }
            else if(isTestPausing == true)
                Timer.Resume();
        }
    }
}
