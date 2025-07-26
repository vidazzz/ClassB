using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{
    public string[] statsNames;
    public GameObject preferbTestMesh;
    private TextMeshProUGUI[] textMeshes; 

    // Start is called before the first frame update
    void Start()
    {
        textMeshes = new TextMeshProUGUI[statsNames.Length];
        for(int i = 0; i < statsNames.Length; i++)
        {
            GameObject go = Instantiate(preferbTestMesh, GetComponent<RectTransform>());
            textMeshes[i] = go.GetComponent<TextMeshProUGUI>();
            textMeshes[i].text = statsNames[i];
            textMeshes[i].text += " : " + (int)Hero.Instance.lifeController.statsPairs[statsNames[i]];
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < statsNames.Length; i++)
        {
            textMeshes[i].text = statsNames[i];
            textMeshes[i].text += " : " + (int)Hero.Instance.lifeController.statsPairs[statsNames[i]];
        }
    }
}
