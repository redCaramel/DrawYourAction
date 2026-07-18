using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreviewUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI SceneText;
    void Awake()
    {
    }


    public void Init(SceneInfo sc)
    {
        string result = "";
        result = $"#{sc.sceneNum} - {sc.content}\n\n";

        foreach (string mission in sc.missonList)
        {
            result += $"{mission}\n";
        }
        SceneText.text = result;
    }
}
