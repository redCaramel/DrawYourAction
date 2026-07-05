using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScriptWriteManager : MonoBehaviour
{
    [SerializeField] private Button titleApplyButton;
    [SerializeField] private TMP_InputField titleInputField;
    void Awake()
    {
        titleApplyButton.onClick.AddListener(OnTitleApplyButtonClicked);
    }
    public void OnTitleApplyButtonClicked()
    {
        ActionRecorder.instance.setScriptTitle(ScriptSelector.instance.GetScriptIndex(), titleInputField.text);
    }
}
