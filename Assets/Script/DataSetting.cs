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
    public List<Group> groups = new List<Group>();
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
        public string hobbyName;
        public int id;
        public int[] talentIndexes;
        public List<Device> devices;
    }
    public static List<Hobby> Hobbies;

    public void AddGroup(Group group)
    {
        if (!groups.Contains(group))
        {
            groups.Add(group);
        }
    }

    public void InitializeHobbis()
    {
        Hobbies = new();
        foreach(HobbySetting hobbySetting in hobbySettings)
        {
            List<Talent> hobbyTalents = new();
            foreach (int index in hobbySetting.talentIndexes)
            {
                hobbyTalents.Add(talents[index]);
            }
            Hobbies.Add(new()
            {
                hobbyName = hobbySetting.hobbyName,
                id = hobbySetting.id,
                talents = hobbyTalents,
                devices = hobbySetting.devices
            });
        }
        foreach (Hobby hobby in Hobbies)
        {
            foreach (Device device in hobby.devices)
            {
                device.hobby = hobby;
            }
        }
    }

    void Awake()
    {
        //临时的buff加载方案
        buffs = new(){
            new StatModifierBuff("burstKpiBouesPossibility","kpiBouesPossibility",0.3f,0.1f),

            new StatModifierBuff("burstKpiBouesMultiplier","kpiBouesMultiplier",2,1,StatModifierBuff.ModifierType.Multiply),

            new StatModifierBuff("decreaseTimeMultiplier","timeMultiplier",-0.2f,-0.1f),

            new StatModifierBuff("burstPreesureResistance","preesureResistance",0.2f,0.3f),
        };
        //临时的skill加载方案
        skills = new(){
            new Skill("FastWork",new int[]{0,1}),
            new Skill("InnerPeace",new int[]{2}),
            new Skill("NiceWork",new int[]{3}),
        };

        InitializeHobbis();
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
