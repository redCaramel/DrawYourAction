using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    [SerializeField] ActionUIManager ui;
    [SerializeField] ActionMissionManager missonManager;
    void Start()
    {
        //TODO - Import Scene Info
        SceneInfo sampleData;
        sampleData.sceneNum = 0;
        sampleData.content = "임시 제목";
        sampleData.missonList = new List<MissionData>{};
        // 각 변수명이 미션 이름, 현재 진행도, 목표치, ID 라고 가정했을 때:
        sampleData.missonList.Add(new MissionData 
        { 
            mainText = "지점 도달", 
            currentValue = 0, 
            maxValue = 0, 
            type = 1,
            isCleared = false
        });
        ui.Init(sampleData);
        missonManager.Init(sampleData.missonList);
    }
}
