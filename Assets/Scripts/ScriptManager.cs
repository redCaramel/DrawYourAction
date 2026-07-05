using System.Collections.Generic;
using UnityEngine;

public class ScriptManager : MonoBehaviour
{
    private List<ScriptData> Scripts = new List<ScriptData>();

    [SerializeField] private GameObject scriptPrefab;
    [SerializeField] private RectTransform scriptListContent;
    [SerializeField] private float scriptSpacing = 20f;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ScriptManager instance {get; private set;}

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
        ApplyInstance();
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
    }

    private void ApplyInstance()
    {
        List<GameObject> ScriptObjects = new List<GameObject>();
        float itemHeight = scriptPrefab.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < Scripts.Count; i++)
        {
            GameObject instance = Instantiate(scriptPrefab, scriptListContent);
            RectTransform rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -i * (itemHeight + scriptSpacing));
            ScriptObjects.Add(instance);
        }
        ScriptSelector.instance.init(ScriptObjects);
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
    }
    public void EnsureScript(int index)
    {
        while (Scripts.Count <= index)
            Scripts.Add(new ScriptData(""));
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
