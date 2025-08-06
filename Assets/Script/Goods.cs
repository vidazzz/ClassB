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
    public string parameterName;
    public int value;
    public int price;
    public int skillIndex; // 如果是技能书，则对应技能的索引
    
    public enum GoodsType
    {
        None = 0,
        Common,
        SkillBook
    }

    void ShopModifyStats ()
    {
        Hero.Instance.lifeController.AddModifier(parameterName,value);
        Hero.Instance.lifeController.AddModifier("money",-price);
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
                button.onClick.AddListener(() => Hero.Instance.LearnSkill(skillIndex));
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
