using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConvController : MonoBehaviour
{
    private static ConvController _instance; //单例
    public static ConvController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ConvController>();
            }
            return _instance;
        }
    }
    public Transform canvas;
    public GameObject prefConvPanel;

    public IEnumerator ShowOptions(Interactable interactable)
    {
        GameObject objConvPanel = Instantiate(prefConvPanel,canvas,false);
        ConvPanel convPanel = objConvPanel.GetComponent<ConvPanel>();
        
        List<ConvPanel.ConvOptionData> convOptionDataList = SetUpOptions(interactable);
        convPanel.speeker = Hero.Instance;
        convPanel.GenerateOptionButtons(convOptionDataList);

        Hero.Instance.EnableInputActionMapMenu();
        Hero.Instance.canActive = false;
        Timer.Pause();
    
        while(!convPanel.isFinished)
            yield return null;
        Destroy(objConvPanel);

        Hero.Instance.EnableInputActionMapPlayer();
        Hero.Instance.canActive = true;
        Timer.Resume();
        
    }

    //根据可交互物构建选项
    public List<ConvPanel.ConvOptionData> SetUpOptions(Interactable interactable)
    {
        List<ConvPanel.ConvOptionData> convOptionDataList = new();
        //为NPC构建选项
        if(interactable is NPC npc)
        {
            foreach(Topic topic in  Hero.Instance.socialManager.topics)
            {
                //交涉话题
                if(topic.responder == npc)
                {
                    convOptionDataList.Add(new ConvPanel.ConvOptionData()
                    {
                        optionText = "Negotiate: " + topic.description,
                        onSelectCallback = () => { Debug.Log("selected topic:" + topic.description); Hero.Instance.Negotiate(npc,topic); }
                    });
                }
                //调查话题
                if(topic.interactables.Contains(npc))
                {
                    convOptionDataList.Add(new ConvPanel.ConvOptionData()
                    {
                        optionText = "Investigate: " + topic.description,
                        onSelectCallback = () => { Debug.Log("selected topic:" + topic.description); Hero.Instance.Investigate(npc,topic); }
                    });
                }
            }
        }
        //为物品构建选项
        else if(interactable is Item item)
        {
            //简单交互
            convOptionDataList.Add(new ConvPanel.ConvOptionData()
            {
                optionText = "Interact: " + item.name,
                onSelectCallback = () => { Hero.Instance.SampleInteractWith(item); }
            });
            //调查话题
            foreach(Topic topic in Hero.Instance.socialManager.topics)
            {
                if(topic.interactables.Contains(item))
                {
                    convOptionDataList.Add(new ConvPanel.ConvOptionData()
                    {
                        optionText = "Investigate: " + topic.description,
                        onSelectCallback = () => { Debug.Log("selected topic:" + topic.description); Hero.Instance.Investigate(item,topic); }
                    });
                }
            }
        }
        else if(interactable is Device device)
        {
            //调查话题
            foreach(Topic topic in Hero.Instance.socialManager.topics)
            {
                if(topic.interactables.Contains(device))
                {
                    convOptionDataList.Add(new ConvPanel.ConvOptionData()
                    {
                        optionText = "Investigate: " + topic.description,
                        onSelectCallback = () => { Debug.Log("selected topic:" + topic.description); Hero.Instance.Investigate(device,topic); }
                    });
                }
            }
        }
        //添加默认结束选项
        convOptionDataList.Add(new ConvPanel.ConvOptionData()
        {
            optionText = "Nothing..."
        });
        return convOptionDataList;
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
