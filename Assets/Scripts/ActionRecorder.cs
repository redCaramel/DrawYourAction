using UnityEngine;

public class ActionRecorder : MonoBehaviour
{
    private int currentRecordingIndex = 0;
    private int recordCount = 0;
    private bool recording = false;
    private float recordDuration = 0f;
    private float currentTime = 0f;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ActionRecorder instance {get; private set;}

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
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    public void Update()
    {
        if(recording)
        {
            currentTime -= Time.deltaTime;
            if(currentTime <= 0f)
            {
                ScriptSelector.instance.SetScriptColor(currentRecordingIndex, 3);
                ScriptData data = ScriptManager.instance.getScript(currentRecordingIndex);
                data.status = 2;
                ScriptManager.instance.SetScript(currentRecordingIndex, data);
                recording = false;

            }
        }
    }
    public void ApplyAction(Action act)
    {
        act.timestamp = recordDuration - currentTime;
        ScriptManager.instance.AddAction(currentRecordingIndex, act);
    }
    public int CurrentRecordingIndex => currentRecordingIndex;
    public int GetRecordCount() => recordCount;
    public void StartRecording(int time)
    {
        currentRecordingIndex = ScriptSelector.instance.GetScriptIndex();
        ScriptManager.instance.EnsureScript(currentRecordingIndex);
        ScriptManager.instance.SetScript(currentRecordingIndex, new ScriptData(""));
        ScriptSelector.instance.SetScriptColor(currentRecordingIndex, 2);
        recordCount++;
        recording = true;
        recordDuration = time;
        currentTime = time;
    }
    public bool isRecording()
    {
        return recording;
    }
    public void setScriptTitle(int index, string title)
    {
        if (index < 0 || index >= ScriptManager.instance.ScriptCount) return;
        ScriptData data = ScriptManager.instance.getScript(index);
        data.name = title;
        ScriptManager.instance.SetScript(index, data);
    }
}
