using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DataSetting : MonoBehaviour
{
    private static DataSetting _instance; //单例
    public static DataSetting Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DataSetting>();
            }
            return _instance;
        }
    }
    public Talent[] talents;
    public List<Buff> buffs;
    public List<Skill> skills;
    public List<Character> characters;
    public GameObject chatTagPrefab; // 群组聊天标签预制件
    public YouChat youChat; // 你聊的实例
    public List<Device> devices;
    public List<Action> actionList;
    public List<Group> groups = new();
    public static List<Character> Characters
    {
        get
        {
            return Instance.characters;
        }
    }
    public List<HobbySetting> hobbySettings;
    [Serializable]
    public struct HobbySetting
    {
        public AttributeID id;
        public List<Device> devices;
    }

    public void AddGroup(Group group)
    {
        if (!groups.Contains(group))
        {
            groups.Add(group);
        }
    }


    void Awake()
    {
        //临时的buff加载方案
        buffs = new(){
            new StatModifierBuff("burstKpiBouesPossibility",AttributeID.kpiBouesPossibility,0.3f,0.1f),

            new StatModifierBuff("burstKpiBouesMultiplier",AttributeID.kpiBouesMultiplier,2,1,StatModifierBuff.ModifierType.Multiply),

            new StatModifierBuff("burstPreesureResistance",AttributeID.preesureResistance,0.2f,0.3f),
        };
        //临时的skill加载方案
        skills = new(){
            new Skill("FastWork",new int[]{0,1}),
            new Skill("InnerPeace",new int[]{2}),
        };

        devices = FindObjectsOfType<Device>().ToList();   
    }
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
