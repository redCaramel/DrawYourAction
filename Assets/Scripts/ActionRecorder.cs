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
                ScriptData data = ScriptDataManager.instance.getScript(currentRecordingIndex);
                data.status = 3;
                ScriptDataManager.instance.SetScript(currentRecordingIndex, data);
                recording = false;

            }
        }
    }
    public void ApplyAction(Action act)
    {
        act.timestamp = recordDuration - currentTime;
        ScriptDataManager.instance.AddAction(currentRecordingIndex, act);
    }
    public int CurrentRecordingIndex => currentRecordingIndex;
    public int GetRecordCount() => recordCount;
    public void StartRecording(int time)
    {
        currentRecordingIndex = ScriptObjectManager.instance.GetScriptIndex();
        ScriptDataManager.instance.EnsureScript(currentRecordingIndex);
        ScriptData data = ScriptDataManager.instance.getScript(currentRecordingIndex);
        ScriptDataManager.instance.SetScript(currentRecordingIndex, new ScriptData(data.name, data.maxDuration, 2));
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
        if (index < 0 || index >= ScriptDataManager.instance.ScriptCount) return;
        ScriptData data = ScriptDataManager.instance.getScript(index);
        data.name = title;
        ScriptDataManager.instance.SetScript(index, data);
    }
}
