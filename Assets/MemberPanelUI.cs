using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemberPanelUI : MonoBehaviour
{
    public GameObject taksPanelPrefab;
    public TextMeshProUGUI memberNameText;
    public void DisplayTasks(List<Company.Project.Task> tasks)
    {
        foreach(Company.Project.Task task in tasks)
        {
            GameObject taskPanel = Instantiate(taksPanelPrefab, transform);
            taskPanel.GetComponentInChildren<TextMeshProUGUI>().text = task.taskName;
            taskPanel.GetComponentInChildren<Slider>().value = task.progress;
        }
    }

    void Awake()
    {
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
