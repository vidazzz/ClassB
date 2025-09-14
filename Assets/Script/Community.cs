using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Community : MonoBehaviour
{
    private static Community _instance; //单例
    public static Community Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Community>();
            }
            return _instance;
        }
    }
    public List<HobbyGroup> hobbyGroups = new();
    public List<Group> groups = new();
    public List<float[]> hobbyTopicScoreValueDisplay;
    private List<Character> characters;
    static public Affinity affinity;
    static public HobbyTopicScore hobbyTopicScore;
    public GridLayoutGroup gridLayoutGroup;
    public TextMeshProUGUI affinityTextMeshPref;
    static List<List<TextMeshProUGUI>> textMeshMatrex;
    public class Affinity{
        private float [,] affinityValue;
        public float[,] AffinityValue{get {return affinityValue;}}
        public List<Character> cherecters = new();
        public Affinity(List<Character> npcs)
        {
            cherecters = npcs;
            affinityValue = new float[npcs.Count,npcs.Count];
            for(int i=0;i<npcs.Count;i++)
            {
                for(int j=0;j<npcs.Count;j++)
                {
                    affinityValue[i,j] = 0;
                }
            }
        }

        public void ModifyAffinity(Character a, Character b, float value)
        {
            int rows = cherecters.IndexOf(a);
            int columns = cherecters.IndexOf(b);
            affinityValue[rows, columns] += value;
            Symmetry(rows, columns);
            //ab各自更新好感度效果状态
            a.TryUpdateAffinityEffect(b);
            b.TryUpdateAffinityEffect(a);
            PrintAffinity();
        }

        public float GetAffinity(Character a,Character b)
        {
            int A = cherecters.IndexOf(a);
            int B = cherecters.IndexOf(b);
            return affinityValue[A,B];
        }

        void Symmetry(int i,int j)
        {
            affinityValue[j,i] = affinityValue[i,j];
        }

    }
    public static void PrintAffinity()
    {
        int columns = affinity.cherecters.Count + 1;
        for(int i = 1; i < columns ; i ++)
        {
            for(int j = 1; j < columns ; j ++)
            {
                textMeshMatrex[i][j].text = affinity.AffinityValue[i-1,j-1].ToString();
            }
        }
    }
    public class HobbyTopicScore
    {
        static private float[,,] hobbyTopicScoreValue;
        public float[,,] HobbyTopicScoreValue { get { return hobbyTopicScoreValue; } }
        static public List<Character> characters = new();
        public HobbyTopicScore(List<Character> npcs)
        {
            characters = npcs;
            hobbyTopicScoreValue = new float[npcs.Count, npcs.Count,DataSetting.Hobbies.Count];
            for (int i = 0; i < npcs.Count; i++)
            {
                for (int j = 0; j < npcs.Count; j++)
                {
                    for(int k = 0; k < DataSetting.Hobbies.Count; k++)
                    {
                        hobbyTopicScoreValue[i, j, k] = 0;
                    }
                }
            }
        }

        public void ModifyHobbyTopicScore(Character a, Character b, Hobby hobby, float value)
        {
            int rows = characters.IndexOf(a);
            int columns = characters.IndexOf(b);
            int hobbyIndex = DataSetting.Hobbies.IndexOf(hobby);
            hobbyTopicScoreValue[rows, columns, hobbyIndex] += value;
            Symmetry(rows, columns);
        }

        public float GetHobbyTopicScore(Character a, Character b, int hobbyIndex)
        {
            int A = characters.IndexOf(a);
            int B = characters.IndexOf(b);
            return hobbyTopicScoreValue[A, B, hobbyIndex];
        }

        void Symmetry(int i, int j)
        {
            for (int k = 0; k < hobbyTopicScoreValue.GetLength(2); k++)
            {
                hobbyTopicScoreValue[j, i, k] = hobbyTopicScoreValue[i, j, k];
            }
            
        }

        static public List<HobbyGroup> ChackHobbyToppicScore()
        {
            
            List<HobbyGroup> hobbyGroups = new();
            List<int> characterIndexList = new();
            //剔除已经有group的角色
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i].groups.Count == 0)
                    characterIndexList.Add(i);
            }
            if (characterIndexList.Count == 0)
                return hobbyGroups;
            
            Debug.Log("ChackHobbyToppicScore");
            for (int hobbyIndex = 0; hobbyIndex < DataSetting.Hobbies.Count; hobbyIndex++) //遍历hobby划分圈子
            {
                int[] arr = characterIndexList.ToArray();
                for (int lastIdxIdx = 0; lastIdxIdx < arr.Length; lastIdxIdx++) //根据对应hobby将characterIndexArr划分成圈子
                {
                    HobbyGroup hobbyGroup = new() { menberIndexList = new() { arr[lastIdxIdx] }, hobbyIndex = hobbyIndex };
                    for (; lastIdxIdx < arr.Length;)//将有关联的元素全部换到前面
                    {
                        bool isIndexMoved = false;
                        for (int j = lastIdxIdx + 1; j < arr.Length; j++)
                        {
                            Debug.Log($"hobbyTopicScoreValue[{arr[lastIdxIdx]}, {arr[j]}, {hobbyIndex}] = {hobbyTopicScoreValue[arr[lastIdxIdx], arr[j], hobbyIndex]}");
                            if (hobbyTopicScoreValue[arr[lastIdxIdx], arr[j], hobbyIndex] >= 1f)
                            {
                                Debug.Log($"characterIndexArr[{lastIdxIdx}] = {arr[lastIdxIdx]}");

                                int temp = arr[lastIdxIdx + 1];
                                arr[lastIdxIdx + 1] = arr[j];
                                arr[j] = temp;

                                lastIdxIdx++;
                                hobbyGroup.menberIndexList.Add(arr[lastIdxIdx]);
                                isIndexMoved = true;
                            }
                        }
                        if (!isIndexMoved)
                            break;
                    }

                    if (hobbyGroup.menberIndexList.Count >= 3) //不少于3人才是圈子
                        hobbyGroups.Add(hobbyGroup);
                }
            }
            Debug.Log("ChackHobbyToppicScore " + hobbyGroups.Count);
            if (hobbyGroups.Count > 0)
                return hobbyGroups;
            else
                return null;
        }
    }

    [Serializable]
    public struct HobbyGroup
    {
        public List<int> menberIndexList;
        public int hobbyIndex;
    }

    public static Coroutine StartConversation(Conversation conversation)
    {
        Debug.Log("StartConversation " + conversation.participants);
        return Instance.StartCoroutine(conversation.Process());
    }

    public static void StopConversation(Conversation conversation)
    {
        Instance.StopCoroutine(conversation.process);
    }

    public Group CreatGroup(HobbyGroup hobbyGroup)
    {
        Hobby hobby = DataSetting.Hobbies[hobbyGroup.hobbyIndex];
        List<Character> menbers = new();
        foreach (int index in hobbyGroup.menberIndexList)
        {
            menbers.Add(characters[index]);
        }

        Group group = YouChat.Instance.SetupChatTagAndBandGroup(hobby, menbers);

        foreach (Character menber in menbers)
        {
            menber.groups.Add(group);
        }

        return group;
    }

    public void CreatAllGroup()
    {
        if (hobbyGroups == null)
            return;
        if (hobbyGroups.Count == 0)
            return;
        foreach (HobbyGroup hobbyGroup in hobbyGroups)
        {
            Group group = CreatGroup(hobbyGroup);
            if (group != null)
                groups.Add(group);
        }
        hobbyGroups.Clear();
    }



    void InitializeCommunityPannel()
    {
        int columns = affinity.cherecters.Count + 1;
        textMeshMatrex = new();
        gridLayoutGroup.constraintCount = columns;
        for(int i = 0 ; i < columns ; i ++)
        {
            textMeshMatrex.Add(new());
            for(int j = 0 ; j < columns ; j ++)
            {
                TextMeshProUGUI newText = Instantiate(affinityTextMeshPref,gridLayoutGroup.gameObject.GetComponent<RectTransform>());
                if(i == 0 && j == 0)
                    newText.text = "CherecterName";
                else if(i == 0)
                    newText.text = affinity.cherecters[j-1].name;
                else if(j == 0)
                    newText.text = affinity.cherecters[i-1].name;
                textMeshMatrex[i].Add(newText);
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        characters = DataSetting.Characters;
        affinity = new Affinity(characters);
        hobbyTopicScore = new HobbyTopicScore(characters);
        //加载社交关系效果
        foreach (Character character in characters)
        {
            foreach (Character targetCharacter in characters)
            {
                if (character.Equals(targetCharacter))
                    continue;
                foreach (AffinityEffectArgs affinityArgs in character.affinityEffectArgsList)
                {
                    AffinityEffect newEffect = new(affinityArgs, character, targetCharacter);
                    targetCharacter.affinityEffects.Add(newEffect);
                }
            }
        }
    }
    void Start()
    {
        EventManager.Instance.OnHourEnd += () => hobbyGroups = HobbyTopicScore.ChackHobbyToppicScore();
        EventManager.Instance.OnDayBegin += invokeDay => CreatAllGroup();
        InitializeCommunityPannel();
        PrintAffinity();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
