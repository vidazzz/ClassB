using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Goods : MonoBehaviour
{
    public GoodsType goodsType;
    Button button;
    public string goodsName;
    public Sprite Icon // 添加图标属性
    { set => GetComponent<Image>().sprite = value; }
    public AttributeID parameterID;
    public int value;
    public int price;
    public int skillIndex; // 如果是技能书，则对应技能的索引
    
    public enum GoodsType
    {
        None = 0,
        Common,
        SkillBook
    }

    void ShopModifyStats()
    {
        Attribute v = Hero.Instance.lifeController.statListManager.GetAttributeByEnum(AttributeID.money);
        if (v.value - price >= 0)
        {
            v.AddModifier(-price);
            Hero.Instance.lifeController.statListManager.AddByEnum(parameterID,value);
        }
    }
    void ShopSkillBook()
    {
        Attribute v = Hero.Instance.lifeController.statListManager.GetAttributeByEnum(AttributeID.money);
        if( v.value - price >= 0)
        {
            v.AddModifier(-price);
            Hero.Instance.LearnSkill(skillIndex);
        }    
    }
    void Awake()
    {
        button = GetComponent<Button>();
    }
    // Start is called before the first frame update
    void Start()
    {
        switch (goodsType)
        {
            case GoodsType.Common:
                button.onClick.AddListener(() => ShopModifyStats());
                break;
            case GoodsType.SkillBook:
                button.onClick.AddListener(() => ShopSkillBook());
                break;
            default:
                button.onClick.AddListener(() => ShopModifyStats());
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
