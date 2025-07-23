using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public string[] talentNames;
    public List<Buff> buffs;
    public List<Skill> skills;

    // Start is called before the first frame update
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
        
    }
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
