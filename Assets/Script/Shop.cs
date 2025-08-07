using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public Canvas canvas;
    public List<GoodsData> goodsList;
    public List<GoodsData> skillBooks;
    public List<GoodsData> FoodList;
    public List<GoodsData> rentalList;

    public ToggleGroup foodToggleGroup;
    private List<Toggle> foodToggles;
    public ToggleGroup rentalToggleGroup;
    private List<Toggle> rentalToggles;
    private int essentialIndex1;
    private int essentialIndex2;
    public Toggle prfToggle;
    public Image prfBook;
    public Transform foodPanel;
    public Transform rentalPanel;
    public Transform goodsScrollViewContent;
    [Serializable]
    public class GoodsData
    {
        public Goods.GoodsType goodsType;
        public string goodsName;
        public Sprite icon; // 添加图标属性
        public string parameterName;
        public int value;
        public int price;
        public int skillIndex; // 如果是技能书，则对应技能的索引
    }
    [HideInInspector]
    public float advancePayment;
    public void ShopModifyStats (GoodsData goods)
    {
        if(Hero.Instance.lifeController.AddModifier("money",-goods.price))
            Hero.Instance.lifeController.AddModifier(goods.parameterName,goods.value);    
    }
    public void StartShopping()
    {
        canvas.gameObject.SetActive(true);
    }

    public void EndShopping()
    {
        SetEssentialIndex();
        EssentialCheckOut();
        canvas.gameObject.SetActive(false);
    }

    void EssentialCheckOut()
    {
        ShopModifyStats (FoodList[essentialIndex1]);
        ShopModifyStats (rentalList[essentialIndex2]);
    }
    public void SetEssentialIndex()
    {
        essentialIndex1 = foodToggleGroup.ActiveToggles().First().transform.GetSiblingIndex() - 1;
        essentialIndex2 = rentalToggleGroup.ActiveToggles().First().transform.GetSiblingIndex() - 1;
    }
    public void ResetToggleGroup(List<Toggle> toggles)
    {
        toggles[0].isOn = true; // 默认选中第一个
    }
    void InitializeMenu()
    {
        foodToggles = new();
        rentalToggles = new();
        foreach (var food in FoodList)
        {
            Toggle newToggle = Instantiate(prfToggle, foodPanel);
            EssentialToggle essentialToggle = newToggle.GetComponent<EssentialToggle>();
            essentialToggle.shop = this; // 设置商店引用
            essentialToggle.toggles = foodToggles; // 设置toggles引用
            essentialToggle.price = food.price;
            newToggle.GetComponentInChildren<Text>(true).text = $"{food.goodsName} [price: {food.price}]";
            newToggle.group = foodToggleGroup;
            foodToggles.Add(newToggle);
        }
        foreach (var rental in rentalList)
        {
            Toggle newToggle = Instantiate(prfToggle, rentalPanel);
            EssentialToggle essentialToggle = newToggle.GetComponent<EssentialToggle>();
            essentialToggle.shop = this; // 设置商店引用
            essentialToggle.toggles = rentalToggles; // 设置toggles引用
            essentialToggle.price = rental.price;
            newToggle.GetComponentInChildren<Text>(true).text = $"{rental.goodsName} [price: {rental.price}]";
            newToggle.group = rentalToggleGroup;
            rentalToggles.Add(newToggle);
        }
        foreach (var book in skillBooks)
        {
            Image newBook = Instantiate(prfBook, goodsScrollViewContent);
            newBook.GetComponent<Goods>().goodsType = book.goodsType;
            newBook.GetComponent<Goods>().goodsName = book.goodsName;
            newBook.GetComponent<Goods>().Icon = book.icon; // 设置图标
            newBook.GetComponent<Goods>().parameterName = book.parameterName;
            newBook.GetComponent<Goods>().value = book.value;
            newBook.GetComponent<Goods>().price = book.price;
            newBook.GetComponent<Goods>().skillIndex = book.skillIndex;
        }
    }

    void AddShoppingCorroutine()
    {
        CoroutineQueueManager.dayEndCoroutineQueue2.AddCoroutine(ShoppingCorroutine());
    }

    IEnumerator ShoppingCorroutine()
    {
        StartShopping();
        do yield return null;
        while(!Input.GetKeyDown(KeyCode.Escape));
        EndShopping();
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (GoodsData goods in goodsList)
        {

        }

        foreach(GoodsData book in skillBooks)
        {
            
        }

        InitializeMenu();

        EventManager.Instance.OnDayEnd2 += AddShoppingCorroutine;
    }

    void OnDisable()
    {
        EventManager.Instance.OnDayEnd2 -= AddShoppingCorroutine;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
