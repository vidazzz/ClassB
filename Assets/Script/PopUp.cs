using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    private static PopUp _instance; //单例
    public static PopUp Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PopUp>();
            }
            return _instance;
        }
    }
    public TextMeshProUGUI popUpText;

    public void ShowPopUp(string str)
    {
        StopAllCoroutines(); //停止之前的弹窗
        StartCoroutine(PopUpCoroutine(str));
    }
    private IEnumerator PopUpCoroutine(string str)
    {
        popUpText.text = str;
        popUpText.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(5); //弹窗时间
        popUpText.transform.parent.gameObject.SetActive(false);
    }

    void Awake()
    {
        gameObject.SetActive(false);
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
