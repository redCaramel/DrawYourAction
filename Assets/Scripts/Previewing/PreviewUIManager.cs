using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreviewUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI SceneText;
    private SceneInfo currentScene;
    void Awake()
    {
    }


    public void Init(SceneInfo sc)
    {
        currentScene = sc;
        Render();
    }

    public void Refresh()
    {
        Render();
    }

    private void Render()
    {
        string result = "";
        result = $"#{currentScene.sceneNum} - {currentScene.content}\n\n";

        foreach (MissionData mission in currentScene.missonList)
        {
            string status = mission.isCleared ? " (클리어)" : "";
            if(mission.maxValue!=0)
                result += $"* {mission.mainText} ({mission.currentValue}/{mission.maxValue}){status}\n";
            else
                result += $"* {mission.mainText}{status}\n";

        }
        SceneText.text = result;
    }
}
