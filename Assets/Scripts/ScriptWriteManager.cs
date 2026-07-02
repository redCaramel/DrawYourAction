using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ScriptWriteManager : MonoBehaviour
{
    [SerializeField] private Button titleApplyButton;
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private List<GameObject> Scripts;
    void Awake()
    {
        titleApplyButton.onClick.AddListener(OnTitleApplyButtonClicked);
    }
    public void OnTitleApplyButtonClicked()
    {
        Debug.Log(ScriptSelector.instance.GetScriptIndex());
        Scripts[ScriptSelector.instance.GetScriptIndex()].GetComponentInChildren<TMP_Text>().text = titleInputField.text;
        ActionRecorder.instance.setScriptTitle(ScriptSelector.instance.GetScriptIndex(), titleInputField.text);
    }
}
