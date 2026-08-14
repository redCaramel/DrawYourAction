using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageClearManager : MonoBehaviour
{
    private static int _currentStage = 1;
    public static int currentStage
    {
        get => _currentStage;
        set
        {
            if (_currentStage == value) return;
            _currentStage = value;
            if (SaveManager.Instance != null) SaveManager.Instance.SaveCurrentState();
        }
    }
    [SerializeField] List<GameObject> stageButtons;
    [SerializeField] List<Sprite> btnSprites;

    void Awake()
    {
        if(currentStage < 0) currentStage = 1;
        //currentStage = 1;
    }

    void Update()
    {
        Debug.Log(currentStage);
        for(int i = 0;i < stageButtons.Count;i++)
        {
            if(currentStage > i)
            {
                if(i!=4 &&(i+1)%5==0) stageButtons[i].GetComponent<Image>().sprite = btnSprites[2];
                else stageButtons[i].GetComponent<Image>().sprite = btnSprites[0];
                stageButtons[i].transform.Find("text").GetComponent<TextMeshProUGUI>().text = $"#{i+1}";
                stageButtons[i].transform.Find("text").gameObject.SetActive(true);
                stageButtons[i].transform.Find("lock").gameObject.SetActive(false);
            }
            else
            {
                if(i!=4 &&(i+1)%5==0) stageButtons[i].GetComponent<Image>().sprite = btnSprites[3];
                else stageButtons[i].GetComponent<Image>().sprite = btnSprites[1];
                stageButtons[i].transform.Find("text").gameObject.SetActive(false);
                stageButtons[i].transform.Find("lock").gameObject.SetActive(true);
            }

        }
    }

    public static void activateStage()
    {
        currentStage++;
    }
    public static void activateStage(int index)
    {
        if(currentStage < index) currentStage = index;
    }
}
