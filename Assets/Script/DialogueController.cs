using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public DialogueGraph dialogueGraph; //对话图
    public List<DialogueGraph> multiPersonDialogueGraphs; //多人对话图
    [HideInInspector]
    public DialogueNode currentDialogueNode; //当前对话节点
    int currentOptionIndex;
    [HideInInspector]
    public Character character;
    bool isWaitingForInput;
    TextMeshProUGUI dialogueTextMesh; //本人的textmesh
    List<TextMeshProUGUI> meetingTextMeshList; //多人对话参与者的textmesh，[0]是本人

    public IEnumerator GraphDisplayDialogue(DialogueGraph dialogueGraph, List<Interactable> characters = null)
    {
        Timer.Pause();
        //处理多人对话情况
        meetingTextMeshList = new() { dialogueTextMesh };
        if (characters != null)
            foreach (var character in characters)
            {
                meetingTextMeshList.Add(character.GetComponent<DialogueController>().dialogueTextMesh);
                Debug.Assert(character != null, "character不能为null");
                Debug.Assert(meetingTextMeshList[^1] != null, "TextMesh不能为null");
            }

        currentDialogueNode = dialogueGraph.StartNode;

        TextMeshProUGUI textMesh = dialogueTextMesh;//默认发言者是本人
        //对话循环
        while (currentDialogueNode != null)
        {
            isWaitingForInput = true;

            /*
            //跳过已经完成检定的选项
            while (currentDialogueNode.optionNodes.Length > 0) //检查该句选项是否已完成，是则更新当前索引并继续检查下一句
            {
                bool didAllOptionsUnChecked = true;
                for (int i = 0; i < currentDialogueNode.optionNodes.Length; i++)
                {
                    DialogueOptionNode option = currentDialogueNode.GetOutputPort($"optionNodes {i}").Connection.node as DialogueOptionNode;
                    if (option.hasChecked)
                    {
                        currentDialogueNode = option.nextNode;
                        if (currentDialogueNode == null) //容错，检定过的选项一路通向结束，对话直接结束了，按规则填写对话分支不应该进入这个判断
                        {
                            EndDialogue(textMesh);
                            Debug.LogWarning("检定过的选项一路通向结束，当前对话节点为null");
                            yield break;
                        }
                        didAllOptionsUnChecked = false;
                        break;
                    }
                }
                if (didAllOptionsUnChecked)
                    break;
            }
            */
            //Debug.Log("currentDialogueNode.speekerIndex : "+currentDialogueNode.speekerIndex);
            textMesh = meetingTextMeshList[currentDialogueNode.speekerIndex]; //如果存在多人对话，对话框根据speekerIndex选取
            textMesh.transform.parent.gameObject.SetActive(true);
            textMesh.text = currentDialogueNode.Line;
            if (currentDialogueNode.optionNodes.Length > 0) //有选项
            {
                List<DialogueOptionNode> options = new();
                textMesh.text += "\n";
                //组装选项
                for (int i = 0; i < currentDialogueNode.optionNodes.Length; i++)
                {
                    DialogueOptionNode option = currentDialogueNode.GetOutputPort($"optionNodes {i}").Connection.node as DialogueOptionNode;
                    options.Add(option);//存储选项节点
                    //option.hasChecked = false;
                    textMesh.text += "\n" + (i + 1) + ". " + option.line;
                    if (option.checkingTalentID != 0)
                        if (!option.hasChecked)
                            textMesh.text += "\t" + DiceCheck.Instance.PredictionString(option.checkingTalentID, option.checkingTalentLevel,Hero.Instance);
                        else
                            textMesh.text += "\t" + (option.checkResult ? "<color=green>已成功</color>" : "<color=red>已失败</color>");
                }
                isWaitingForInput = true;
                while (isWaitingForInput) //等待输入
                {
                    for (int j = 0; j < currentDialogueNode.optionNodes.Length; j++)
                    {
                        if (Input.GetKeyDown(KeyCode.Alpha1 + j)) //如果按下数字键，转换为对话选项索引并记录
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
                yield return StartCoroutine(options[currentOptionIndex].OptionEffect(this)); //执行选项效果
            }
            else if (currentDialogueNode.optionNodes.Length == 0) //无选项
            {
                while (true) //等待输入
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        yield return null;
                        break;
                    }
                    yield return null;
                }
                textMesh.transform.parent.gameObject.SetActive(false); //输入后关闭对话框
                currentDialogueNode = currentDialogueNode != null ? currentDialogueNode.NextNode : null; //获取下一句
            }
            if (currentDialogueNode != null ? currentDialogueNode.nextGraph : null != null) //如果有对话图,优先进入对话图
            {
                yield return StartCoroutine(GraphDisplayDialogue(currentDialogueNode.nextGraph));
            }
            yield return null; //等待一帧，避免过快跳过对话
        }
        //Debug.Log(currentDialogueNode);
        //跳出循环，下一句是空，结束对话
        EndDialogue(textMesh);
    }

    void EndDialogue(TextMeshProUGUI textMesh)
    {
        Hero.Instance.canActive = true;
        Timer.Resume();
        textMesh.transform.parent.gameObject.SetActive(false);
        textMesh.text = "";
    }

    /*
    IEnumerator SelectOption(int currentOptionIndex)
    {
        LineNode lineNode = currentDialogue[currentLineIndex];
        DialogueOption option = lineNode.options[currentOptionIndex];
        yield return StartCoroutine(option.OptionEffect(this));
    }
    */
    
    IEnumerator GraghSelectOption(int currentOptionIndex)
    {
        DialogueOptionNode option = currentDialogueNode.GetOutputPort($"optionNodes {currentOptionIndex}").Connection.node as DialogueOptionNode;
        yield return StartCoroutine(option.OptionEffect(this));
    }

    public void TestClearAllCheck()
    {
        dialogueGraph.Reset();
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
