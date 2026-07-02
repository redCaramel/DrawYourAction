using System.Collections.Generic;
using UnityEngine;

public class ActionRecorder : MonoBehaviour
{
    private List<ScriptData> recordActions = new List<ScriptData>();
    private int currentRecordingIndex = 0;
    private List<Action> Actions => recordActions.Count > currentRecordingIndex ? recordActions[currentRecordingIndex].actions : null;
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
                recording = false;
                
            }
        }
    }
    public void ApplyAction(Action act)
    {
        act.timestamp = recordDuration - currentTime;
        Actions.Add(act);
    }
    public List<Action> GetAction()
    {
        return Actions != null ? new List<Action>(Actions) : new List<Action>();
    }
    public List<Action> GetAction(int index)
    {
        if (index < 0 || index >= recordActions.Count) return new List<Action>();
        return new List<Action>(recordActions[index].actions);
    }
    public int GetRecordCount() => recordCount;
    public void StartRecording(int time)
    {
        currentRecordingIndex = ScriptSelector.instance.GetScriptIndex();
        while (recordActions.Count <= currentRecordingIndex)
            recordActions.Add(new ScriptData(""));
        recordActions[currentRecordingIndex] = new ScriptData("");
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
        if (index < 0 || index >= recordActions.Count) return;
        ScriptData data = recordActions[index];
        data.name = title;
        recordActions[index] = data;
    }
}
