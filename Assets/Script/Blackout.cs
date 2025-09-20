using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    Image image;
    Image CG_Image;
    public string[] narrationSentances;
    bool isFading;

    public void  FadeInOrOut()
    {
        if(!isFading)
            StartCoroutine(FadeInOrOutCoroutine());
    }
    public IEnumerator FadeInOrOutCoroutine()
    {
        if (isFading)
            yield break;
        Color color = image.color;
        if (color.a == 0)
            yield return FadeInCoroutine();
        if (color.a == 1)
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
    public IEnumerator DisplayText(string[] sentances , Sprite[] CGs = null)
    {
        if (CGs != null && CGs.Length > 0)
        {
            int i = 0;
            foreach (Sprite CG in CGs)
            {
                CG_Image.sprite = CG;
                CG_Image.gameObject.SetActive(true);
                for (; i < sentances.Length; i++)
                {
                    if (sentances[i] == "#")
                    {
                        i++;
                        break; // 如果遇到#，则跳到下一张CG的显示
                    }
                    // 显示文本
                    textMesh.text = sentances[i];
                    textMesh.gameObject.SetActive(true);
                    // 等待按下空格键
                    while (!Input.GetKeyDown(KeyCode.Space))
                        yield return null;
                    yield return null; // 等待一帧
                }
                // 如果已经显示完所有文本，只有图片，则等待空格键
                if (i >= sentances.Length)
                {
                    while (!Input.GetKeyDown(KeyCode.Space))
                        yield return null;
                    yield return null; // 等待一帧 
                }
            }
            CG_Image.gameObject.SetActive(false);
            textMesh.gameObject.SetActive(false);
        }
        else
        {
            CG_Image.gameObject.SetActive(false);
            textMesh.gameObject.SetActive(true);
            foreach (string sentance in sentances)
            {
                textMesh.text = sentance;
                // 等待按下空格键
                while (!Input.GetKeyDown(KeyCode.Space))
                    yield return null;
                yield return null; // 等待一帧
            }
            textMesh.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        image = GetComponent<Image>();
        CG_Image = GetComponentsInChildren<Image>(true)[1]; // 获取第二个Image组件作为CG_Image
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        //每日例行黑幕和文本显示
        EventManager.Instance.OnDayEnd += () => CoroutineQueueManager.dayEndCoroutineQueue.AddCoroutine(FadeInOrOutCoroutine());
        EventManager.Instance.OnDayEnd += () => CoroutineQueueManager.dayEndCoroutineQueue.AddCoroutine(DisplayText(narrationSentances));
        EventManager.Instance.OnDayBegin += invokeDay => CoroutineQueueManager.dayBeginCoroutineQueue.AddCoroutine(FadeInOrOutCoroutine());
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}
