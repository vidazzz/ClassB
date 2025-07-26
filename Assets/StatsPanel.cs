using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{
    public string[] statsNames;
    public GameObject preferbTestMesh;

    // Start is called before the first frame update
    void Start()
    {
        foreach(string statsName in statsNames)
        {
            GameObject go = Instantiate(preferbTestMesh, GetComponent<RectTransform>());
            TextMeshProUGUI textMesh = go.GetComponent<TextMeshProUGUI>();
            textMesh.text = statsName;
            textMesh.text += " "+Hero.Instance.lifeController.statsPairs[statsName].ToString("F2");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
