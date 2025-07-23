using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public Canvas canvas;
    public List<Goods> goodsList;
    public List<Goods> skillBooks;
    public List<Goods> FoodList;
    public List<Goods> rentalList;

    public ToggleGroup foodToggleGroup;
    public ToggleGroup rentalToggleGroup;
    public int essentialIndex1;
    public int essentialIndex2;
    public Toggle prfToggle;
    public Transform foodPanel;
    public Transform rentalPanel;
    [Serializable]
    public class Goods
    {
        public string goodsName;
        public string parameterName;
        public int value;
        public int price;
        public Button button;
    }
    
    public void ShopModifyStats (Goods goods)
    {
        Hero.Instance.lifeController.AddModifier(goods.parameterName,goods.value);
        Hero.Instance.lifeController.AddModifier("money",-goods.price);
    }
    public void ShopLearnSkill (Goods goods)
    {
        Hero.Instance.LearnSkill(goods.value); //value是skill索引
        Hero.Instance.lifeController.AddModifier("money",-goods.price);
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
    void InitializeMenu()
    {
        foreach(var food in FoodList)
        {
            Toggle newToggle = Instantiate(prfToggle,foodPanel);
            newToggle.GetComponentInChildren<Text>(true).text = $"{food.parameterName} [price: {food.price}]";
            newToggle.group = foodToggleGroup;
        }
        foreach(var rental in rentalList)
        {
            Toggle newToggle = Instantiate(prfToggle,rentalPanel);
            newToggle.GetComponentInChildren<Text>(true).text = $"{rental.parameterName} [price: {rental.price}]";
            newToggle.group = rentalToggleGroup;
        }
    }

    void AddShoppingCorroutine()
    {
        Timer.dayEndCoroutineQueueManager.AddCoroutine(ShoppingCorroutine());
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
        foreach(Goods goods in goodsList)
        {
            goods?.button.onClick.AddListener(() => ShopModifyStats(goods));
        }

        foreach(Goods book in skillBooks)
        {
            book?.button.onClick.AddListener(() => ShopLearnSkill(book));
        }

        InitializeMenu();

        Timer.onDayEnd2 += AddShoppingCorroutine;
    }

    void OnDisable()
    {
        Timer.onDayEnd2 -= AddShoppingCorroutine;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
