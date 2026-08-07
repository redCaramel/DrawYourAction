using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScriptWriteManager : MonoBehaviour
{
    [SerializeField] private Button titleApplyButton;
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private List<GameObject> colorGroup;
    [SerializeField] private GameObject previewScript;
    [SerializeField] private Button thumbnailDrawButton;
    [SerializeField] private GameObject DrawingPopup;
    [SerializeField] private IconDrawer iconDrawer;
    private string previewScriptName;
    private string previewScriptTime;
    private int previewScriptColor;
    private Sprite previewScriptThumbnail;

    public static readonly List<Color> ScriptColor = new List<Color>
    {
        new Color(1f, 0f, 0f),
        new Color(255f/255f, 131f/255f, 0f/255f),
        new Color(255f/255f, 255f/255f, 0f/255f),
        new Color(129f/255f, 255f/255f, 0f/255f),
        new Color(0f/255f, 255f/255f, 255f/255f),
        new Color(0f/255f, 24f/255f, 255f/255f),
        new Color(151f/255f, 0f/255f, 255f/255f),
        new Color(255f/255f, 0f/255f, 183f/255f),
        new Color(255f/255f, 255f/255f, 255f/255f),
        new Color(140f/255f, 140f/255f, 140f/255f)
    };
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
        thumbnailDrawButton.onClick.AddListener(OnThumbnailButtonClicked);
        for(int i = 0;i < 10;i++)
        {
            int colorIndex = i;
            colorGroup[i].GetComponent<Button>().onClick.AddListener(() => OnColorButtonClicked(colorIndex));
        }
        
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
        ScriptObjectManager.instance.SetScriptColor(ScriptObjectManager.instance.GetScriptIndex(), previewScriptColor);
        ScriptObjectManager.instance.SetScriptThumbnail(ScriptObjectManager.instance.GetScriptIndex(), previewScriptThumbnail);
    }
    private void OnThumbnailButtonClicked()
    {
        DrawingPopup.SetActive(true);
        iconDrawer.ResetCanvas();
    }
    void OnEnable()
    {
        int index = ScriptObjectManager.instance.GetScriptIndex();
        Debug.Log(previewScriptColor);
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
        for(int i = 0;i < 10;i++)
        {
            if(i==ScriptDataManager.instance.getScript(index).color) colorGroup[i].GetComponent<RawImage>().color = new Color(0,0,0);
            else colorGroup[i].GetComponent<RawImage>().color = new Color(1,1,1);
        }
        previewScriptColor = ScriptDataManager.instance.getScript(index).color;
        previewScriptName = titleInputField.text;
        previewScriptTime = $"{ScriptDataManager.instance.getScript(index).maxDuration} sec";
        previewScriptThumbnail = ScriptDataManager.instance.getScript(index).thumbnail;
    }
    private void OnColorButtonClicked(int num)
    {
        int temp = previewScriptColor;
        previewScriptColor = num;
        colorGroup[temp].GetComponent<RawImage>().color = new Color(1, 1, 1);
        colorGroup[previewScriptColor].GetComponent<RawImage>().color = new Color(0, 0, 0);

    }
    private void UpdatePreviewScript()
    {
        int index = ScriptObjectManager.instance.GetScriptIndex();
        if(index < 0) return;
        previewScriptName = titleInputField.text;
        previewScriptTime = $"{ScriptDataManager.instance.getScript(index).maxDuration} sec";
        previewScript.transform.Find("title").GetComponent<TMP_Text>().text = previewScriptName;
        previewScript.transform.Find("time").GetComponent<TMP_Text>().text = previewScriptTime;
        previewScript.transform.Find("thumbnail").GetComponent<Image>().sprite = previewScriptThumbnail;
        previewScript.GetComponent<Image>().color = ScriptColor[previewScriptColor];
        
    }
    void Update()
    {
        UpdatePreviewScript();
    }
    public void setPreviewThumbnail(Sprite thumbnail)
    {
        previewScriptThumbnail = thumbnail;
        previewScript.transform.Find("thumbnail").GetComponent<Image>().sprite = thumbnail;
    }
}
