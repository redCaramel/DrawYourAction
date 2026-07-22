using TMPro;
using UnityEngine;

public class ActionUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI SceneText;
    private SceneInfo currentScene;

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
            string st = mission.isCleared ? "<s>" : "";
            string end = mission.isCleared ? "</s>" : "";
            if(mission.maxValue!=0)
                result += $"* {st}{mission.mainText} ({mission.currentValue}/{mission.maxValue}){end}\n";
            else
                result += $"* {st}{mission.mainText}{end}\n";

        }
        SceneText.text = result;
    }
}
