using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public class Device : Interactable
{
    public Hobby hobby;
    public Character owner;
    public List<string> checkItems;
    public List<Character.Value> outputs;
    public float maxProgress = 100;
    public Canvas screenSpaceCanvas;
    public GameObject progressBarPrefab;
    private GameObject progressBarObj;
    public ProgressBar progressBar;
    public Operation operation;
    


    public override IEnumerator Interact(Character interactor)
    {
        users.Add(interactor);
        if (interactor is Hero)
        {
            Debug.Log("Hero using Device");
            progressBar.gameObject.SetActive(true);
            operation = new(interactor, checkItems, outputs, maxProgress);
            yield return interactor.StartCoroutine(operation.ProcessHero());
            progressBar.gameObject.SetActive(false);
            Debug.Log("Used by Hero");
        }
        Need gainNeed = null;
        //处理收益和消耗
        if (gain.needName != "")
        {
            gainNeed = interactor.lifeController.GetNeed(gain.needName);
            gainNeed.isChanging = true; //将需求标记为正在操作
            gainNeed.MotifyValue(gain.value);
        }
        if (cost.needName != "")
            interactor.lifeController.GetNeed(cost.needName).MotifyValue(-cost.value);
        yield return StartCoroutine(PlayAnimation());
        if (gainNeed != null)
            gainNeed.isChanging = false;
        users.Remove(interactor);
    }

    protected IEnumerator PlayAnimation()
    {
        if (animator != null)
            animator.SetBool("isOccupied", true);
        Timer.Date beginTime = Timer.Time;
        while (Timer.Time - beginTime < duration) //使用时间
        {
            yield return null;
        }
        if (animator != null)
            animator.SetBool("isOccupied", false);
    }

    new void Awake()
    {
        base.Awake();
        screenSpaceCanvas = GameObject.Find("ScreenSpaceCanvas")?.GetComponent<Canvas>();
        Debug.Assert(screenSpaceCanvas != null, "Cannot find ScreenSpaceCanvas in scene");
    }

    private void Start()
    {
        progressBarObj = Instantiate(progressBarPrefab, screenSpaceCanvas.transform);
        progressBarObj.SetActive(false);
        progressBar = progressBarObj.GetComponent<ProgressBar>();
        Debug.Log(progressBar);
        progressBar.owner = gameObject;
    }

    private void Update()
    {
        if(progressBarObj.activeSelf)
        {
            progressBar.progress = operation.Progress;
        }
    }
}
