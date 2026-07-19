using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RecordingMenuSelector : MonoBehaviour
{
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject recordButton;
    [SerializeField] private GameObject writeButton;
    [SerializeField] private GameObject setButton;
    [SerializeField] private List<Sprite> buttonSprites;
    private Image rec, write, set;
    private int selectedMenu;
    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static RecordingMenuSelector instance {get; private set;}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }
    private void Awake()
    {
        selectedMenu = 1;
        rec = recordButton.GetComponent<Image>();
        write = writeButton.GetComponent<Image>();
        set = setButton.GetComponent<Image>();

        rec.sprite = buttonSprites[1];
        write.sprite = buttonSprites[2];
        set.sprite = buttonSprites[4];

        recordButton.GetComponent<Button>().onClick.AddListener(OnRecordButtonClicked);
        writeButton.GetComponent<Button>().onClick.AddListener(OnWriteButtonClicked);
        setButton.GetComponent<Button>().onClick.AddListener(OnSetButtonClicked);
        if(instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    public void OnRecordButtonClicked()
    {
        selectedMenu = 1;
        rec.sprite = buttonSprites[1];
        write.sprite = buttonSprites[2];
        set.sprite = buttonSprites[4];
        RecordUIManager.instance.ShowContent(0);
    }
    public void OnWriteButtonClicked()
    {
        if(ActionRecorder.instance.isRecording()) return;
        selectedMenu = 2;
        rec.sprite = buttonSprites[0];
        write.sprite = buttonSprites[3];
        set.sprite = buttonSprites[4];
        RecordUIManager.instance.ShowContent(1);
    }
    public void OnSetButtonClicked()
    {
        if(ActionRecorder.instance.isRecording()) return;
        selectedMenu = 3;
        rec.sprite = buttonSprites[0];
        write.sprite = buttonSprites[2];
        set.sprite = buttonSprites[5];
        RecordUIManager.instance.ShowContent(2);
    }
    public int getMenu()
    {
        return selectedMenu;
    }
}
