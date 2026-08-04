using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScriptWriteManager : MonoBehaviour
{
    [SerializeField] private Button titleApplyButton;
    [SerializeField] private TMP_InputField titleInputField;
    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ScriptWriteManager instance {get; private set;}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }
    private void Awake()
    {
        
        if(instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
        titleApplyButton.onClick.AddListener(OnTitleApplyButtonClicked);
        
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------
    public void OnTitleApplyButtonClicked()
    {
        ActionRecorder.instance.setScriptTitle(ScriptObjectManager.instance.GetScriptIndex(), titleInputField.text);
    }
    void OnEnable()
    {
        UpdateWriteScreen();
    }
    public void UpdateWriteScreen()
    {
        int index = ScriptObjectManager.instance.GetScriptIndex();
        UpdateWriteScreen(index);
        
    }
    public void UpdateWriteScreen(int index)
    {
        titleInputField.text = ScriptDataManager.instance.getScript(index).name;
        //TODO - more datas
    }
}
