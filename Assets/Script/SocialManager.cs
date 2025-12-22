using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class SocialManager : MonoBehaviour
{
    public List<Attitude> attitudes = new();
    public Character owner;
    public Conversation conversation;
    public Rader rader;
    public AttributeID topicHobbyId;
    public float conversationAffinityThreshold;
    //话题库
    public List<Topic> topics;

    public void CreatConversation()
    {
        if (topicHobbyId == 0)
        {
            Debug.LogError($"{name} try to creat conversation with no hobby!");
            return;
        }
        conversation = new(topicHobbyId, owner);
    }
    public bool FindConversation()
    {
        bool result = false;
        foreach (Character target in rader.interactables)
        {
            if (TryJoinConversation(target))
            {
                result = true;
                break;
            }
        }
        return result;
    }

    public bool TryJoinConversation(Character target)
    {
        if (Community.affinity.GetAffinity(owner, target) < conversationAffinityThreshold) //检查好感度
            return false;
        if (target.socialManager.conversation != null)
        {
            conversation = target.socialManager.conversation;
            conversation.Add(owner);
            return true;
        }
        else
            return false;
    }

    public void QuitConversation()
    {
        conversation?.Remove(owner);
        conversation = null;
    }

    public void OpenRadder()
    {
        rader.enabled = true;
    }

    public void CloseRadder()
    {
        rader.enabled = false;
    }


    public void SetUpAttitudes()
    {
        foreach(Character other in DataSetting.Instance.characters)
        {
            if(other != owner)
            {
                Attitude newAttitude = new(owner,other);
                attitudes.Add(newAttitude);
            }
        }
    }

    public Attitude GetAttitudeTowards(Character other)
    {
        //Debug.Log($"OWNER:{owner} other:{other}");
        return attitudes.Find(a => a.to == other);
    }

    public void StartConv(Character target)
    {
        
    }
    public void Awake()
    {
        owner = GetComponent<Character>();
        if(owner is NPC npc)
        rader = npc.rader;
        SetUpAttitudes();
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
