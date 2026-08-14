using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class StageEntryManager : MonoBehaviour
{
    [SerializeField] List<GameObject> stageButtons;
    [SerializeField] private Button entryButton;
    [SerializeField] private Button preivewButton;
    private int currentStage = 0;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static StageEntryManager instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    // ----------------------------------------------------

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);

        for(int i = 0;i < stageButtons.Count;i++)
        {
            int stageIndex = i+1;
            stageButtons[i].GetComponent<Button>().onClick.AddListener(() => OnStageButtonClicked(stageIndex));
        }
        entryButton.onClick.AddListener(OnEntryButtonClicked);
        preivewButton.onClick.AddListener(OnPreviewButtonClicked);
    }
    public void SetStageButtonsInteractable(bool interactable)
    {
        for(int i = 0;i < stageButtons.Count;i++)
        {
            stageButtons[i].GetComponent<Button>().interactable = interactable;
        }
    }
    private void OnEntryButtonClicked()
    {
        Debug.Log(currentStage);
        if(currentStage == 0) return;
        if(!StageImporter.ImportStage(currentStage)) return;
        AudioManager.instance.PlaySFX(SFXType.button2);
        StageModeSetting.setMode(false);
        SceneManager.LoadScene("RecordDevelopingScene");
        
    }
    private void OnPreviewButtonClicked()
    {
       
        if(currentStage==0) return;
        if(!StageImporter.ImportStage(currentStage)) return;
         AudioManager.instance.PlaySFX(SFXType.button2);
        StageModeSetting.setMode(true);
        SceneManager.LoadScene(StageImporter.sceneName);
        
    }
    private void OnStageButtonClicked(int num)
    {
        
        currentStage = num;
        if(StageClearManager.currentStage < currentStage) return;
        if(!StageImporter.ImportStage(currentStage)) return;
        AudioManager.instance.PlaySFX(SFXType.button1);
        PopupAnim.ShowPopup(currentStage);
        Debug.Log(currentStage);
        
    }

}
