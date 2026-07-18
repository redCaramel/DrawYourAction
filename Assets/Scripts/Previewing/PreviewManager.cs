using System.Collections.Generic;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    [SerializeField] PreviewUIManager ui;
    void Start()
    {
        //TODO - Import Scene Info
        SceneInfo sampleData;
        sampleData.sceneNum = 1;
        sampleData.content = "영지 탈환";
        sampleData.missonList = new List<MissionData>{};
        // 각 변수명이 미션 이름, 현재 진행도, 목표치, ID 라고 가정했을 때:
        sampleData.missonList.Add(new MissionData 
        { 
            mainText = "모든 적 처치", 
            currentValue = 0, 
            maxValue = 20, 
            type = 1 
        });
        sampleData.missonList.Add(new MissionData 
        { 
            mainText = "지점 도달", 
            currentValue = 0, 
            maxValue = 0, 
            type = 2
        });
        ui.Init(sampleData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
