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

        foreach (MissionData mission in sc.missonList)
        {
            if(mission.maxValue!=0)
                result += $"* {mission.mainText} ({mission.currentValue}/{mission.maxValue})\n";
            else 
                result += $"* {mission.mainText}\n";

        }
        SceneText.text = result;
    }
}
