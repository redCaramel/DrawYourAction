using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class StageImporter : MonoBehaviour
{
    private const string StageDataResourcePath = "stageData";

    public static int scriptCount { get; private set; }
    public static int handCount { get; private set; }
    public static int stageCount { get; private set; }
    public static string sceneName { get; private set; }
    public static string title { get; private set; }
    public static List<int> cardTime {get; private set;}
    public static List<MissionData> missionList { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        scriptCount = 0;
        handCount = 0;
        stageCount = 0;
        sceneName = "";
        title = "";
    }

    public static bool ImportStage(int sceneNum)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(StageDataResourcePath);
        if (jsonFile == null)
        {
            Debug.LogError($"StageImporter: could not find Resources/{StageDataResourcePath}.json");
            return false;
        }

        StageDataList dataList = JsonUtility.FromJson<StageDataList>(jsonFile.text);
        StageData data = dataList.stageData.Find(d => d.sceneNum == sceneNum);
        if (data == null)
        {
            Debug.LogError($"StageImporter: no stageData found with sceneNum {sceneNum}");
            return false;
        }

        scriptCount = data.maxScript;
        handCount = data.handSize;
        stageCount = data.sceneNum;
        sceneName = data.sceneName;
        title = data.title;
        cardTime = data.cardTime;
        missionList = data.mission;
        return true;
    }

}
