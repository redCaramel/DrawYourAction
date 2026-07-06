using System.Collections.Generic;
using UnityEngine;

public class ScriptDataManager : MonoBehaviour
{
    private List<ScriptData> Scripts = new List<ScriptData>();

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ScriptDataManager instance {get; private set;}

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

        Init();
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    private void Init()
    {
        Scripts.Add(new ScriptData("a", 1, 1));
        Scripts.Add(new ScriptData("b", 3, 1));
        Scripts.Add(new ScriptData("c", 5, 1));

        for (int i = 0; i < Scripts.Count; i++)
            ScriptObjectManager.instance.UpdateScriptView(i, Scripts[i]);
    }

    public int ScriptCount => Scripts.Count;

    public ScriptData getScript(int index)
    {
        return Scripts[index];
    }
    public void SetScript(int index, ScriptData data)
    {
        if (index < 0 || index >= Scripts.Count) return;
        Scripts[index] = data;
        ScriptObjectManager.instance.UpdateScriptView(index, data);
    }
    public void EnsureScript(int index)
    {
        while (Scripts.Count <= index)
        {
            Scripts.Add(new ScriptData(""));
            ScriptObjectManager.instance.UpdateScriptView(Scripts.Count - 1, Scripts[Scripts.Count - 1]);
        }
    }
    public List<Action> GetAction(int index)
    {
        if (index < 0 || index >= Scripts.Count) return new List<Action>();
        return new List<Action>(Scripts[index].actions);
    }
    public void AddAction(int index, Action act)
    {
        Scripts[index].actions.Add(act);
    }
}
