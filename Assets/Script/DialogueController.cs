using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public List<DialogueData> dialogues;
    public List<DialogueData> multiPersonDialogues;
    List<LineNode> currentDialogue;
    public int currentLineIndex;
    int currentOptionIndex;
    [HideInInspector]
    public Character character;
    bool isWaitingForInput;
    TextMeshProUGUI dialogueTextMesh; //本人的textmesh
    List<TextMeshProUGUI> meetingTextMeshList; //多人对话参与者的textmesh，[0]是本人

    public void Initialize(DialogueData dialogue ,List<GameObject> objs = null)
    {
        meetingTextMeshList = new(){dialogueTextMesh};
        if(objs != null)
            foreach(var obj in objs)
            {
                meetingTextMeshList.Add(obj.GetComponent<DialogueController>().dialogueTextMesh);
                Debug.Assert(obj != null, "obj不能为null");
                Debug.Assert(meetingTextMeshList[^1] != null,"TextMesh不能为null");
            }
        currentLineIndex = dialogue.FirstLineIndex <= 0 ? 0 : dialogue.FirstLineIndex - 1;
        currentDialogue = dialogue.dialogue;
    }
    public IEnumerator DisplayDialogue()
    {
        Timer.Pause();
        TextMeshProUGUI textMesh = dialogueTextMesh;//默认发言者是本人 
        if(currentLineIndex < currentDialogue.Count)
        {
            isWaitingForInput = true;
            LineNode lineNode;
            //Debug.Log(currentLineIndex);
            //跳过已经完成检定的选项
            while(currentDialogue[currentLineIndex].options.Count > 0) //检查该句选项是否已完成，是则更新当前索引并继续检查下一句
            {
                //Debug.Log(currentLineIndex);
                bool didAllOptionsUnChecked = true;
                foreach (var option in currentDialogue[currentLineIndex].options)
                {
                    //Debug.Log(currentLineIndex);
                    if(option.hasChecked)
                    {
                        if(option.jumpToLine == 0) //如果填0是下一句
                            currentLineIndex ++;
                        else
                            currentLineIndex = option.jumpToLine -1;
                        if(currentLineIndex >= currentDialogue.Count) //容错，检定过的选项一路通向结束，对话直接结束了，按规则填写对话分支不应该进入这个判断
                            {
                                EndDialogue(textMesh);
                                yield break;
                            }
                        didAllOptionsUnChecked = false;
                        break;
                    }
                }
                if(didAllOptionsUnChecked)
                    break;  
            }
            lineNode = currentDialogue[currentLineIndex];

            if(lineNode.speekerIndex != 0)            
                textMesh = meetingTextMeshList[lineNode.speekerIndex]; //如果存在多人对话，对话框根据speekerIndex选取
            textMesh.transform.parent.gameObject.SetActive(true);
            textMesh.text = lineNode.Line;
            if (lineNode.options.Count > 0) //有选项
            {
                textMesh.text += "\n";
                int i = 1;
                //组装选项
                foreach (var option in lineNode.options)
                {
                    textMesh.text += "\n" + i + ". " + option.line;
                    if(option.checkingSkillName != "")
                        textMesh.text +="\t" + DiceCheck.Instance.PredictionString(option.checkingSkillName,option.checkingSkillLevel);
                    i++;
                }
                isWaitingForInput = true;
                while(isWaitingForInput) //等待输入
                {
                    for (int j = 0; j < lineNode.options.Count; j++)
                    {
                        if(Input.GetKeyDown(KeyCode.Alpha1 + j)) //如果按下数字键，转换为对话选项索引并记录
                        {
                            yield return null;  
                            currentOptionIndex = j;
                            isWaitingForInput = false;
                            break;
                        }
                    }
                    yield return null;
                }
                textMesh.transform.parent.gameObject.SetActive(false); //输入后关闭对话框
                yield return StartCoroutine(SelectOption(currentOptionIndex));
            }
            else if (lineNode.options.Count == 0) //无选项
            {
                while(true) //等待输入
                {
                    if(Input.GetKeyDown(KeyCode.Space))
                    {
                        yield return null;
                        break;
                    }
                    yield return null;
                }
                textMesh.transform.parent.gameObject.SetActive(false); //输入后关闭对话框
                yield return StartCoroutine(DisplayNextLine());
            }
        }
        else
        {
            EndDialogue(textMesh);
        }
    }
    
    public IEnumerator DisplayNextLine()
    {
        if(currentDialogue[currentLineIndex].nextLine == 0)
            currentLineIndex ++; //如果是0就按照索引顺序找下一句
        else
            currentLineIndex = currentDialogue[currentLineIndex].nextLine - 1;
        yield return StartCoroutine(DisplayDialogue());
    }

    void EndDialogue(TextMeshProUGUI textMesh)
    {
        Hero.Instance.canActive = true;
        Timer.Resume();
        textMesh.transform.parent.gameObject.SetActive(false);
        textMesh.text = "";
        currentLineIndex = 0;
    }

    IEnumerator SelectOption(int currentOptionIndex)
    {
        LineNode lineNode = currentDialogue[currentLineIndex];
        DialogueOption option = lineNode.options[currentOptionIndex];
        yield return StartCoroutine(option.OptionEffect(this));
    }

    public void TestClearAllCheck()
    {
        foreach(var dialogue in dialogues)
        {
            dialogue.Reset();
        }          
    }
    
    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<Character>();
        dialogueTextMesh = GetComponentInChildren<TextMeshProUGUI>(true);
        Debug.Assert(dialogueTextMesh != null,$"{gameObject}dialogueTextMesh不能为null");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
