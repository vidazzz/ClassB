using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Community : MonoBehaviour
{
    public List<Character> characters = new();
    static public Affinity affinity;
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

        public void ModifyAffinity(Character a,Character b,float value)
        {
            int rows = cherecters.IndexOf(a);
            int columns = cherecters.IndexOf(b);
            affinityValue[rows,columns] += value;
            Symmetry(rows,columns);
            //ab各自更新好感度效果状态
            a.TryUpdateAffinityEffect(b);
            b.TryUpdateAffinityEffect(a);
        }

        public float GetAfinity(Character a,Character b)
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
    void initializeCommunityPannel()
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
        affinity = new Affinity(characters);
        //加载社交关系效果
        foreach(Character character in characters)
        {
            foreach(Character targetCharacter in characters)
            {
                if(character.Equals(targetCharacter))
                    continue;
                foreach(AffinityEffectArgs affinityArgs in character.affinityEffectArgsList)
                {
                    AffinityEffect newEffect = new(affinityArgs,character,targetCharacter);
                    targetCharacter.affinityEffects.Add(newEffect);
                }
            }
        }
    }
    void Start()
    {
        initializeCommunityPannel();
        PrintAffinity();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
