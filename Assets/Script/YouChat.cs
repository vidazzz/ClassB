using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class YouChat : MonoBehaviour
{
    private static YouChat _instance; //单例
    public static YouChat Instance{ get{ if (_instance == null) {_instance = FindObjectOfType<YouChat>();} return _instance; }}
    public GameObject heroMessagePrefab;
    public GameObject messagePrefab;
    public GameObject chatPrefab;
    public TextMeshProUGUI typpeinTextMesh;
    [HideInInspector]
    public ScrollRect chatScrollRect;
    public RectTransform chatContent;
    public RectTransform chatPanel;
    public RectTransform MessagePanel;
    private DialogueNode currentDialogueNode;
    private int currentOptionIndex = -1;
    bool isWaitingForInput = false;
    public List<Chat> chats = new();
    public GameObject typeinOptionPrefab;
    public List<TypeinOption> typeinOptions = new();
    public RectTransform typeinContent;
    public RectTransform TaskPanel;
    public GameObject TaskBottonPrefab;

    [Serializable]
    public class TypeinOption
    {
        public string optionName;
        public TypeinOptionType optionType;
    }
    
    public enum TypeinOptionType
    {
        Null = 0,
        PostTask,
        Chat,
    }

    [Serializable]
    public class Chat
    {
        public string chatName;
        public Sprite profilePic;
        public List<Character> groupMembers;
        public List<Message> chatHistory;
        public List<GameObject> messageItems;
        public Group group;
    }

    public class Message
    {
        public string message;
        public Character character;
        public Timer.Date time;
        public Message(string message, Character character)
        {
            this.message = message;
            this.character = character;
            time = Timer.Time;
        }
    }

    public void InitializeChat()
    {
        foreach (Chat chat in chats)
        {
            GameObject chatTag = Instantiate(chatPrefab, chatPanel);
            chatTag.GetComponentInChildren<TextMeshProUGUI>().text = chat.chatName;
            chatTag.GetComponentsInChildren<Image>()[1].sprite = chat.profilePic;
            chatTag.GetComponent<Button>().onClick.AddListener(() =>
            {
                chatPanel.gameObject.SetActive(false);
                MessagePanel.gameObject.SetActive(true);
                //StartCoroutine(GraphDisplayDialogue(chat));
                //SetupTypein(chat);
            });
        }
    }

    public Group SetupChatTagAndBandGroup(AttributeID hobbyID, List<Character> groupMembers, Sprite profilePic = null)
    {
        GameObject chatTag = Instantiate(chatPrefab, chatPanel);
        Group group = chatTag.GetComponent<Group>();
        group.InitializeGroup(hobbyID, groupMembers, profilePic);
        chatTag.GetComponentInChildren<TextMeshProUGUI>().text = group.groupName;
        chatTag.GetComponentsInChildren<Image>()[1].sprite = profilePic;
        chatTag.GetComponent<Button>().onClick.AddListener(() =>
        {
            //chatPanel.gameObject.SetActive(false);
            ActiveMessagePanel(group.chat);
            
        });
        SetupTypein();
        SetupTaskPanel(group);
        return group;
    }

    public void ActiveMessagePanel(Chat chat)
    {
        MessagePanel.gameObject.SetActive(true);
        foreach (GameObject messageItem in chat.messageItems)
        {
            messageItem.SetActive(true);
        }
    }

    public void ClearMessagePanel(Chat chat)
    {
        foreach (GameObject messageItem in chat.messageItems)
        {
            messageItem.SetActive(false);
        }
    }
    public void SetupTypein()
    {
        foreach (TypeinOption option in typeinOptions)
        {
            GameObject optionItem = Instantiate(typeinOptionPrefab, typeinContent);
            optionItem.GetComponentInChildren<TextMeshProUGUI>().text = option.optionName;
            optionItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                switch (option.optionType)
                {
                    case TypeinOptionType.PostTask:
                        // 处理活动发起逻辑
                        
                        TaskPanel.gameObject.SetActive(true);
                        break;
                    case TypeinOptionType.Chat:
                        // 处理聊天逻辑
                        Debug.Log("Chat clicked.");
                        break;
                    default:
                        Debug.LogWarning("Unknown type of option clicked.");
                        break;
                }
            });
        }
    }

    //活动选择面板
    public void SetupTaskPanel(Group group)
    {
        foreach (TaskManager.TaskData taskData in group.activeTasks)
        {
            GameObject TaskBotton = Instantiate(TaskBottonPrefab, TaskPanel);
            TaskBotton.GetComponentInChildren<TextMeshProUGUI>().text = taskData.taskName;
            TaskBotton.GetComponent<Button>().onClick.AddListener(() =>
            {
                PostTask(taskData, group, Hero.Instance);
                TaskPanel.gameObject.SetActive(false);
            });
        }
    }

    public void PostTask(TaskManager.TaskData taskData, Group group, Character character)
    {
        // 这里可以添加设置活动投票的逻辑
        if (group.TryPostTask(taskData))
        {
            Message message = new("发起活动：" + taskData.taskName, character);
            SendMassage(message, group.chat);
        }
        else
        {
            Debug.Log($"{character.name}发起活动{taskData.taskName}失败，{group.groupName}群已有活动{group.activeTask.taskName}，无法发起新活动");
        }
    }

    public void SendMassage(Message message, Chat chat)
    {
        GameObject messageItem;
        if (message.character is Hero)
            messageItem = Instantiate(heroMessagePrefab, chatContent);
        else
            messageItem = Instantiate(messagePrefab, chatContent);
        messageItem.GetComponentInChildren<TextMeshProUGUI>().text = message.message;
        messageItem.GetComponentsInChildren<Image>()[1].sprite = message.character.profilePic;
        messageItem.GetComponentInChildren<TextMeshProUGUI>().text += "\n<size=60%>" + message.time.ToString() + "</size>";
        chat.messageItems.Add(messageItem);
        chat.chatHistory.Add(message);
        chatScrollRect.verticalNormalizedPosition = 0f;

        Debug.Log("chatScrollRect.verticalNormalizedPosition: " + chatScrollRect.verticalNormalizedPosition);
    }

    public IEnumerator DialogueGraphToChat(Chat chat,DialogueGraph dialogueGraph)
    {
        List<Character> groupMenbers = chat.groupMembers;
        Timer.Pause();
        //处理多人对话情况

        currentDialogueNode = dialogueGraph.StartNode;

        //对话循环
        while (currentDialogueNode != null)
        {
            isWaitingForInput = true;
            if (currentDialogueNode.nextGraph != null) //如果有对话图,优先进入对话图
            {
                currentDialogueNode = currentDialogueNode.nextGraph.StartNode;
            }
            Message message = new(currentDialogueNode.Line, groupMenbers[currentDialogueNode.speekerIndex]);
            SendMassage(message, chat);
            if (currentDialogueNode.optionNodes.Length > 0) //有选项
            {
                List<DialogueOptionNode> options = new();
                typpeinTextMesh.text += "\n";
                //组装选项
                for (int i = 0; i < currentDialogueNode.optionNodes.Length; i++)
                {
                    DialogueOptionNode option = currentDialogueNode.GetOutputPort($"optionNodes {i}").Connection.node as DialogueOptionNode;
                    options.Add(option);//存储选项节点
                    //option.hasChecked = false;
                    typpeinTextMesh.text += "\n" + (i + 1) + ". " + option.line;
                    if (option.checkingTalentID != 0)
                        if (!option.hasChecked)
                            typpeinTextMesh.text += "\t" + DiceCheck.Instance.PredictionString(option.checkingTalentID, option.checkingTalentLevel, Hero.Instance);
                        else
                            typpeinTextMesh.text += "\t" + (option.checkResult ? "<color=green>已成功</color>" : "<color=red>已失败</color>");
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
                yield return StartCoroutine(options[currentOptionIndex].OptionEffect(groupMenbers[0].dialogueController)); //执行选项效果
            }
            else if (currentDialogueNode.optionNodes.Length == 0) //无选项
            {
                yield return new WaitForSeconds(1f);               
                currentDialogueNode = currentDialogueNode.NextNode; //获取下一句
            }
            yield return null; //等待一帧，避免过快跳过对话
        }
        Debug.Log(currentDialogueNode);
    }
    void Awake()
    {
        chatScrollRect = GetComponentInChildren<ScrollRect>(true);
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
