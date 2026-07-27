using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageData
{
    public int sceneNum;
    public string sceneName;
    public string title;
    public int maxScript;
    public int handSize;
    public List<MissionData> mission;
}

[System.Serializable]
public class StageDataList
{
    public List<StageData> stageData;
}
