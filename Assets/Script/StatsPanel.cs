using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{
    public AttributeID[] statIDs;
    public GameObject preferbTestMesh;
    private TextMeshProUGUI[] textMeshes; 

    // Start is called before the first frame update
    void Start()
    {
        textMeshes = new TextMeshProUGUI[statIDs.Length];
        for(int i = 0; i < statIDs.Length; i++)
        {
            GameObject go = Instantiate(preferbTestMesh, GetComponent<RectTransform>());
            textMeshes[i] = go.GetComponent<TextMeshProUGUI>();
            textMeshes[i].text = statIDs[i].ToString();
            textMeshes[i].text += " : " + (int)Hero.Instance.lifeController.statListManager.GetAttributeByEnum(statIDs[i]).value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < statIDs.Length; i++)
        {
            textMeshes[i].text = statIDs[i].ToString();
            textMeshes[i].text += " : " + (int)Hero.Instance.lifeController.statListManager.GetAttributeByEnum(statIDs[i]).value;
        }
    }
}
