using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Blackout : MonoBehaviour
{
    private static Blackout _instance;
    public static Blackout Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Blackout>();
            }
            return _instance;
        }
    }
    [SerializeField] private float fadeDuration = 1f; // 淡入持续时间
    TextMeshProUGUI textMesh;
    UnityEngine.UI.Image image;
    public string[] narrationSentances;
    bool isFading;

    public void FadeInOrOut()
    {
        if(!isFading)
            StartCoroutine(FadeInOrOutCoroutine());
    }
    public IEnumerator FadeInOrOutCoroutine()
    {   
        if(isFading)
            yield break;
        Color color = image.color; 
        Debug.Log(color); 
        if(color.a == 0)
            yield return FadeInCoroutine();
        if(color.a == 1)
            yield return FadeOutCoroutine();
    }
       // 淡入协程
    private IEnumerator FadeInCoroutine()
    {
        isFading = true;
        Color color = image.color;
        
        // 淡入过程
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            color.a = alpha;
            image.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终透明度为1
        color.a = 1f;
        image.color = color;
        
        isFading = false;
    }

    // 淡出协程（可选）
    private IEnumerator FadeOutCoroutine()
    {
        isFading = true;
        Color color = image.color;
        
        // 淡出过程
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            color.a = alpha;
            image.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保最终透明度为0
        color.a = 0;
        image.color = color;
        
        isFading = false;
    }

    //文本显示
    public IEnumerator DisplayText()
    {
        textMesh.gameObject.SetActive(true);
        foreach(string s in narrationSentances)
        {
            textMesh.text = s;
            do
                yield return null;
            while(!Input.GetKeyDown(KeyCode.Space)); //等待按下空格键     
        }
        textMesh.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
