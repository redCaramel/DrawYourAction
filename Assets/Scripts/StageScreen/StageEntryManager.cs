using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class StageEntryManager : MonoBehaviour
{
    [SerializeField] List<GameObject> stageButtons;
    [SerializeField] private Button entryButton;
    private int currentStage = 0;
    void Awake()
    {
        for(int i = 0;i < stageButtons.Count;i++)
        {
            int stageIndex = i+1;
            stageButtons[i].GetComponent<Button>().onClick.AddListener(() => OnStageButtonClicked(stageIndex));
        }
        entryButton.onClick.AddListener(OnEntryButtonClicked);
    }
    private void OnEntryButtonClicked()
    {
        Debug.Log(currentStage);
        if(currentStage == 0) return;
        if(!StageImporter.ImportStage(currentStage)) return;
        SceneManager.LoadScene("RecordDevelopingScene");
    }
    private void OnStageButtonClicked(int num)
    {
        currentStage = num;
        if(!StageImporter.ImportStage(currentStage)) return;
        PopupAnim.ShowPopup(currentStage);
        Debug.Log(currentStage);
    }

}
