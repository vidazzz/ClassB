using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public List<Action> actions = new();
    public Action currentAction;
    public Action CurrentAction{get { currentAction = ChooseAction(); return currentAction; } }
    public Character character;

    public Canvas screenSpaceCanvas;
    public GameObject progressBarPrefab;
    [HideInInspector]
    public GameObject progressBarObj;
    public ProgressBar progressBar;

    public void SetUpActions()
    {
        foreach(Action action in DataSetting.Instance.actionList)
        {
            Action newAction = new(action,character);
            actions.Add(newAction);
        }
    }

    public Action ChooseAction()
    {     
        Action maxPriorityAction = actions 
                                    .OrderByDescending(a => a.Priority)
                                    .FirstOrDefault();
        Debug.Log($"{character.name}'s action:{maxPriorityAction.target.name} Priority: {maxPriorityAction.Priority}");
        return maxPriorityAction;
    }

    void Awake()
    {
        character = GetComponent<Character>();
        screenSpaceCanvas = GameObject.Find("ScreenSpaceCanvas").GetComponent<Canvas>();
        Debug.Assert(screenSpaceCanvas != null, "Cannot find ScreenSpaceCanvas in scene");
        actions = new List<Action>();
        SetUpActions();

        progressBarObj = Instantiate(progressBarPrefab, screenSpaceCanvas.transform);
        progressBarObj.SetActive(false);
        progressBar = progressBarObj.GetComponent<ProgressBar>();
        progressBar.owner = gameObject;
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
