using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
//交涉相关定义

public class Negotiation
{
    public string description;
    public Topic topic;
    public ActiveWindow activeWindow;
    public struct ActiveWindow
    {
        public Timer.Date startTime;
        public Timer.Date endTime;
    }
    public Character responder;

    public int targetScore;
    public int score;
    public Turn turn;
    public bool result; //交涉结果，成功或失败
    public Negotiation(Topic topic, Character responder, Timer.Date startTime, Timer.Date endTime)
    {
        this.responder = responder;
        this.topic = topic;
        activeWindow = new ActiveWindow
        {
            startTime = startTime,
            endTime = endTime
        };
    }
    public class Turn
    {
        public Intelligence intel1;
        public Intelligence intel2;
        public Character firstToAct;
        public Character lastToAct;
        public Turn(Character first, Character last)
        {
            firstToAct = first;
            lastToAct = last;
        }
        public IEnumerator Process()
        {
            // 处理回合逻辑
            intel1 = UseIntelligence(firstToAct);
            PutIntelOnTable(intel1);
            yield return null;
            intel2 = UseIntelligence(lastToAct);
            PutIntelOnTable(intel2);
            yield return null;
            SettleAccount();

        }

        public Intelligence UseIntelligence(Character character)
        {
            return null;
        }
        
        public void PutIntelOnTable(Intelligence intelligence)
        {
            
        }
        public void SettleAccount()
        {

        }
    }
    //发掘话题
    public Topic DiscoverNewTopic()
    {
        AttributeID theme;
        if(responder is NPC npc)
        {
            if(npc.IsBusy)
            {
                theme = npc.actionManager.CurrentAction.theme;
            }
            else
                return null;
        }
        else if(responder is Hero hero)
        {
            theme = hero.currentAction.theme;
        }
        else
            theme = 0;
        if(theme == 0)
                return null;
        else
            return new Topic(responder); 
    }
    public Topic SelectTopicNPC()
    {
        //临时方案：随机选择一个话题
        return responder.socialManager.topics[Random.Range(0,responder.socialManager.topics.Count)];
    }


    public void CloseNegotiation()
    {
        
    }
}

public class Topic
{
    public string description;
    public Character responder;
    public AttributeID theme = 0;
    public TopicType Type => GetTopicType();
    //相关可交互物
    public List<Interactable> interactables = new();
    public List<Intelligence> unrevealedIntels = new();
    public List<Intelligence> revealedIntels = new();
    public List<Intelligence> oppoIntels = new();
    public float Completeness => revealedIntels.Count / (float)unrevealedIntels.Count;
    public float attitudeValue;
    public float Output => attitudeValue * Completeness;
    public enum TopicType
    {
        none = 0,
        life,
        work,
        political,
    }    

    public Topic(Character responder,string description = "A Topic")
    {
        this.responder = responder;
        this.description = description;
        SetUpUnrevealedIntels();
        //初始化对手情报列表
        int n = Random.Range(3, unrevealedIntels.Count + 1);
        List<int> indexList = Enumerable.Range(0, unrevealedIntels.Count - 1).ToList();
        Debug.Log("indexList count:" + indexList.Count);
        for (int i = 0; i < n && indexList.Count > 0; i++)
        {
            int index = Random.Range(0, indexList.Count);
            Debug.Log("selected index:" + index + " from indexList count:" + indexList.Count);
            Intelligence intel = unrevealedIntels[indexList[index]];
            oppoIntels.Add(intel);
            indexList.RemoveAt(index);
        }
    }

    private void SetUpUnrevealedIntels()
    {
        //根据话题主题设置未揭示情报列表
        int numIntels = Random.Range(5, 11);
        for (int i = 0; i < numIntels; i++)
        {
            Intelligence intel = new(this, Random.Range(1, 6), "Intel Content " + (i + 1));
            unrevealedIntels.Add(intel);
        }
    }

    public TopicType GetTopicType()
    {
        return theme switch
        {
            AttributeID.coffee or AttributeID.sports or AttributeID.art or AttributeID.gaming or AttributeID.music => TopicType.life,
            AttributeID.work => TopicType.work,
            AttributeID.political => TopicType.political,
            _ => TopicType.none,
        };
    }
    public void RevealIntel()
    {
        if (revealedIntels.Count >= unrevealedIntels.Count)
            return;
        //临时方案：随机揭示一条信息
        int index = Random.Range(0, unrevealedIntels.Count);
        Intelligence intel = unrevealedIntels[index];
        revealedIntels.Add(intel);
        unrevealedIntels.RemoveAt(index);
    }
}

public class Intelligence
{
    public Topic topic;
    public int strength;
    public string content;
    public Intelligence(Topic topic, int strength, string content)
    {
        this.topic = topic;
        this.strength = strength;
        this.content = content;
    }
}
