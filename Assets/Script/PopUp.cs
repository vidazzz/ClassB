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
    private TextMeshProUGUI popUpText;
    public void ShowPopUp(string str)
    {
        StartCoroutine(PopUpCoroutine(str));
    }
    private IEnumerator PopUpCoroutine(string str)
    {
        popUpText.text = str;
        popUpText.transform.parent.gameObject.SetActive(true);
        yield return new WaitForSeconds(5); //弹窗时间
        popUpText.transform.parent.gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        popUpText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
