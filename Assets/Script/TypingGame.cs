using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using ColorUtility = UnityEngine.ColorUtility;
using System.Linq;
using System.Collections.Generic;

public class TypingGame : MonoBehaviour
{
    private string[] arrows = {"↑","↓","←","→"};
    private List<string> currentArrows;
    //public string[] sentences;        // 游戏句子数组
    public Color correctColor = Color.green;  // 正确字符的颜色
    public Color normalColor = Color.black;   // 普通字符的颜色
    private string currentSentence;   // 当前句子
    private int currentIndex = 0;     // 当前字符索引
    public TextMeshProUGUI textMesh; // 用于颜色修改的TextMeshPro组件
    public TextMeshProUGUI textMeshForQuit;
    public Slider timeLimitSlider;
    public int sentenceCount;  //当天任务句子总数
    private string displaySentence;
    private bool isTyping = false;
    private bool doHaveTime = true;
    public int kpi;
    public float preesure;
    private float timeLimit = 3;
    private float timeLimitTimer;
    private float fishingTime;
    public float fishingTimeThreshold;
    public int fish;
    public int fishThreshold;
    public Hero hero;
    bool hasDisplayQuit;

    
    public int[] Shuffle(string sentence)
    {
        int n = sentence.Length;
        int[] indexArray = Enumerable.Range(0, n - 1).ToArray();
        // 从数组中随机抽取不重复的元素，返回随机排列后的数组
        // 处理空数组或单元素数组
        if (indexArray == null || indexArray.Length <= 1)
            return indexArray;        
        // Fisher-Yates洗牌算法核心逻辑
        for (int i = indexArray.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i); // 生成[0, i]的随机索引
            int temp = indexArray[i];
            indexArray[i] = indexArray[j];
            indexArray[j] = temp;
        }
        
        return indexArray;
    }

    void Start()
    {  
         
    }
    void OnDisable()
    {
        Timer.onOffWork -= EndGame;//订阅广播,以后结算时自动关闭打字界面
        Timer.onDayEnd -= EndGame; //订阅广播,以后结算时自动关闭打字界面
        Timer.onOffWork2 -= Hero.Instance.lifeController.TrySalarySettleAccounts;
        Timer.onDayEnd -= Hero.Instance.lifeController.TrySalarySettleAccountsAffterWork;
    }

    void Update()
    {
        fishingTimer();       
    }
    
    void CheckInput()
    {
        bool isMarch = false;
        // 获取当前应该输入的字符
        switch(currentArrows[currentIndex])
        {
            case "↑":
                if(Input.GetKeyDown(KeyCode.W))
                    isMarch = true;
                break;
            case "↓":
                if(Input.GetKeyDown(KeyCode.S))
                    isMarch = true;
                break;
            case "←":
                if(Input.GetKeyDown(KeyCode.A))
                    isMarch = true;
                break;
            case "→":
                if(Input.GetKeyDown(KeyCode.D))
                    isMarch = true;
                break;
            default:
                break;
        }

        // 比较输入字符与目标字符
        if (isMarch)
        {
            // 输入正确，移动到下一个字符
            currentIndex++;
            // 更改字符颜色
            HighlightCorrectCharacter();  
        }
        else
        {
            Debug.Log(Input.inputString);
            currentIndex = 0;
            displaySentence = $"<mspace=60px><color=#{ColorUtility.ToHtmlStringRGB(normalColor)}>{currentSentence}</color>";
            textMesh.text = displaySentence;
            // 输入错误重新开始本句子
        }
    }
    
    void HighlightCorrectCharacter()
    {
        // 修改文本格式以突出显示正确输入的字符
        displaySentence = "";
        // 已输入正确的部分（绿色）
        displaySentence += $"<mspace=60px><color=#{ColorUtility.ToHtmlStringRGB(correctColor)}>";
        displaySentence += currentSentence[..currentIndex];
        displaySentence += "</color>";
        
        // 未输入的部分（黑色）
        if (currentIndex < currentSentence.Length)
        {
            displaySentence += $"<mspace=60px><color=#{ColorUtility.ToHtmlStringRGB(normalColor)}>";
            displaySentence += currentSentence[currentIndex..];
            displaySentence += "</color>";
        }
        
        // 更新显示文本
        textMesh.text = displaySentence;
    }

    void HighlightCorrectCharacter(string sentence)
    {
        // 修改文本格式以突出显示正确输入的字符
        displaySentence = "";
        // 已输入正确的部分（绿色）
        displaySentence += $"<mspace=100px><color=#{ColorUtility.ToHtmlStringRGB(correctColor)}>";
        displaySentence += sentence[..currentIndex];
        displaySentence += "</color>";
        
        // 未输入的部分（黑色）
        if (currentIndex < sentence.Length)
        {
            displaySentence += $"<mspace=100px><color=#{ColorUtility.ToHtmlStringRGB(normalColor)}>";
            displaySentence += sentence[currentIndex..];
            displaySentence += "</color>";
        }
        
        // 更新显示文本
        textMesh.text = displaySentence;
    }
    
    public IEnumerator StartTyping()
    {
        Debug.Log("StartTyping");
        Coroutine timelimitTimer = StartCoroutine(TimeLimitTimer());
        StartNewOne();
        yield return Check(); //持续检查字符直到退出或完成
        if(!isTyping)
            yield break;
        if(doHaveTime) //不是因为倒计时结束而是因为完成句子跳出检查的
        {
            StopCoroutine(timelimitTimer);
            SettleAccount(); //结算
        }     
        yield return StartTyping(); //没有退出则继续下一句
    }

    public IEnumerator Check()
    {
        int[] ShuffleIndexArray = Shuffle(currentSentence);
        while(currentIndex < currentSentence.Length && isTyping && doHaveTime)
        {
            if(!Timer.hasPaused)
            {
                if(fish >= fishThreshold)
                {
                    if(!hasDisplayQuit)
                        DisplayQuitGame();
                    if(Input.GetKeyDown(KeyCode.Escape) && !Timer.hasPaused)
                        EndGame();       
                }
                // 检测键盘输入
                if (Input.anyKeyDown)
                {
                    fishingTime = 0;
                    CheckInput();
                }
                if ((int)Hero.Instance.lifeController.PreesureLevel > 2) //如果压力过大就会导致字符跳动
                    HighlightCorrectCharacter(ReplaceRandomChar(currentSentence,ShuffleIndexArray,(int)Hero.Instance.lifeController.PreesureLevel-2));
                else
                    HighlightCorrectCharacter(currentSentence);
            }
            yield return null;
        }
    }
    public void StartNewOne()
    {
        isTyping = true;
        currentArrows = new();
        int j = Random.Range(4,6);
        for(int i = 0 ; i < j ; i++)
        {
            currentArrows.Add(arrows[Random.Range(0,arrows.Length)]);
        }
        currentSentence = string.Join("", currentArrows);
        // 从句子数组中随机选择一个句子
        //currentSentence = sentences[Random.Range(0, sentences.Length)];   
        
        displaySentence = $"<mspace=100px><color=#{ColorUtility.ToHtmlStringRGB(normalColor)}>{currentSentence}</color>";
        
        // 重置索引
        currentIndex = 0;
        
        // 打开textMesh所在panel
        textMesh.transform.parent.gameObject.SetActive(true);
        
        // 显示初始文本（全黑色）
        textMesh.text = displaySentence;
    }
    private IEnumerator TimeLimitTimer()
    {
        
        doHaveTime = true;
        timeLimitSlider.value = 1;
        timeLimitTimer = 0;
        while(timeLimitTimer < timeLimit)
        {
            if(!Timer.hasPaused)
            {
            timeLimitTimer += Time.deltaTime;
            timeLimitSlider.value = Mathf.Lerp(1,0,timeLimitTimer/timeLimit);
            }
            yield return null;
        }
        doHaveTime = false;
    }

    public void EndGame()
    {
        isTyping = false;
        // 关闭textMesh所在panel
        textMesh.transform.parent.gameObject.SetActive(false);
        if(textMeshForQuit.transform.parent.gameObject.activeSelf)//如果退出提示开着,第一次主动退出
        {
            textMeshForQuit.transform.parent.gameObject.SetActive(false);// 关闭提示退出的panel
            Timer.nextFrameCoroutineQueueManager.AddCoroutine(CutSceneController.Instance.ExecuteCoroutines(1)); //装填cutscene协程，执行一次ExecuteCoroutines进入演出
            Timer.onOffWork += EndGame;//订阅广播,以后结算时自动关闭打字界面
            Timer.onDayEnd += EndGame; //订阅广播,以后结算时自动关闭打字界面

            Timer.onDayEnd -= Hero.Instance.lifeController.SalarySettleAccounts;        //取消之前的结算策略
            Timer.onOffWork += Hero.Instance.lifeController.Try_KPI_SettleAccounts;     //第一次关闭界面后使用新的结算策略
            Timer.onOffWork2 += Hero.Instance.lifeController.TrySalarySettleAccounts;   //第一次关闭界面后使用新的结算策略
            Timer.onDayEnd += Hero.Instance.lifeController.TrySalarySettleAccountsAffterWork;   //第一次关闭界面后使用新的结算策略
            Timer.hadQuitTypingGame = true;
            Debug.Log("如果退出提示开着");
        }
        StopAllCoroutines();
    }
    
    public string ReplaceRandomChar(string input,int[] indexArrey,int indexCount)
    {
        if (string.IsNullOrEmpty(input)) return input;
        // 转换为字符数组
        char[] chars = input.ToCharArray();
        for(int i  = 0 ; i < indexCount && i < indexArrey.Length ; i++)
        {
            int asciiCode = Random.Range(65, 91);
            char randomArrow = (char)asciiCode; //arrows[Random.Range(0,arrows.Length-1)].Single();
            // 修改指定位置的字符
            //Debug.Log($"indexArrey[i]:{indexArrey[i]} ,  chars.length{chars.Length}");
            chars[indexArrey[i]] = randomArrow;
        }
        
        // 返回新字符串
        return new string(chars);
    }

    private void DisplayQuitGame()
    {
        hasDisplayQuit = true;
        // 开启提示退出的panel
        textMeshForQuit.transform.parent.gameObject.SetActive(true);
        textMeshForQuit.text =  $"Do <color={"red"}>Not</color> Press ESC";
    }

    public void SettleAccount()
    {
        Hero.Instance.lifeController.AddModifier("kpi",kpi*Hero.Instance.lifeController.KpiMultiplier);
        Hero.Instance.lifeController.AddModifier("preesure",preesure);
    }

    void fishingTimer()
    {
        if(!isTyping || Timer.hasPaused)
            return;
        fishingTime += Time.deltaTime;
        if(fishingTime>=fishingTimeThreshold)
        {
            fishingTime = 0;
            fish ++;
            Hero.Instance.lifeController.AddModifier("preesure",-1); //摸鱼解压 
            if(Hero.Instance.lifeController.KpiMultiplier>1)
                Hero.Instance.lifeController.AddModifier("kpi", -5); //老板查岗时公然摸鱼扣kpi
        }
    }
}
