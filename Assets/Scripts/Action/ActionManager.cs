using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    [SerializeField] ActionUIManager ui;
    [SerializeField] ActionMissionManager missonManager;
    void Start()
    {

        SceneInfo sceneData;
        sceneData.sceneNum = StageImporter.stageCount;
        sceneData.content = StageImporter.title;
        sceneData.missonList = StageImporter.missionList;

        ui.Init(sceneData);
        missonManager.Init(sceneData.missonList);
        InitialCutSceneManager.instance.StartCutScene();
    }
}
