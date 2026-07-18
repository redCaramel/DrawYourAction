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
        sampleData.missonList = new List<string> { "- 모든 적 처치", "- 영지 탈환" };
        sampleData.missonType = new List<int> {1, 2}; 
        ui.Init(sampleData) ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
