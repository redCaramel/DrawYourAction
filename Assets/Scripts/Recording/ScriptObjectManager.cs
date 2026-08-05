using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScriptObjectManager : MonoBehaviour
{
    [SerializeField] private int selectedScriptIndex;
    [SerializeField] private List<GameObject> Scripts = new List<GameObject>();

    [SerializeField] private GameObject scriptPrefab;
    [SerializeField] private RectTransform scriptListContent;
    [SerializeField] private float scriptSpacing = 20f;
    [SerializeField] private float initialSpacing = 30f;
    private bool isFirstUpdate = true;

    public static readonly List<Color> StatusColor = new List<Color>
    {
        new Color(1f, 1f, 1f),
        new Color(241/255f, 255/255f, 139/255f),
        new Color(1f, 0f, 0f),
        new Color(81/255f, 1f, 0f)
    };

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
    public static ScriptObjectManager instance {get; private set;}

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

    // Instantiates slots up to and including index, so the GameObject list always
    // has one entry per ScriptData index in ScriptManager.
    private void EnsureSlot(int index)
    {
        bool isFirst = true;
        float itemHeight = scriptPrefab.GetComponent<RectTransform>().rect.height;
        while (Scripts.Count <= index)
        {
            int i = Scripts.Count;
            GameObject slot = Instantiate(scriptPrefab, scriptListContent);
            RectTransform rect = slot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            if(isFirst)
            {
                rect.anchoredPosition = new Vector2(0f, -i * (itemHeight + scriptSpacing) - scriptSpacing - initialSpacing);
                isFirst = false;
            }
            else rect.anchoredPosition = new Vector2(0f, -i * (itemHeight + scriptSpacing) - scriptSpacing);
            slot.GetComponent<ScriptDragger>().ScriptIndex = i;
            slot.GetComponent<Button>().onClick.AddListener(() => OnScriptClicked(i));
            Scripts.Add(slot);
        }
        if(isFirstUpdate) {
            isFirstUpdate = false;
            ChangeSelectedScript(0,0);
        }
        RectTransform rt = scriptListContent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, Scripts.Count * 350);
    }

    // Called by ScriptManager whenever a ScriptData entry is created or changed,
    // so the GameObject view always reflects the latest ScriptData.
    public void UpdateScriptView(int index, ScriptData data)
    {
        EnsureSlot(index);
        Scripts[index].transform.Find("title").GetComponent<TMP_Text>().text =
            string.IsNullOrEmpty(data.name) ? "스크립트 " + (index + 1) : data.name;
        Scripts[index].transform.Find("time").GetComponent<TMP_Text>().text =
            string.IsNullOrEmpty(data.name) ? "- sec" : $"{data.maxDuration} sec";
        Scripts[index].GetComponent<Image>().color = ScriptColor[data.color];
    }

    private void OnScriptClicked(int index)
    {
        ChangeSelectedScript(selectedScriptIndex, index);
        ProgressBarManager.instance.SetMaxDuration(ScriptDataManager.instance.getScript(index).maxDuration);
        ScriptWriteManager.instance.UpdateWriteScreen(index);
        selectedScriptIndex = index;
    }

    public int GetScriptIndex()
    {
        return selectedScriptIndex;
    }
    public void SetScriptColor(int index, int color)
    {
        ScriptData data = ScriptDataManager.instance.getScript(index);
        data.color = color;
        ScriptDataManager.instance.SetScript(index, data);
        Scripts[index].GetComponent<Image>().color = ScriptColor[color];

    }
    private void ChangeSelectedScript(int old, int newer) {
        if (old != -1) ClearScriptOutline(old);
        if (newer != -1) HighlightScriptOutline(newer);
    }

    private void ClearScriptOutline(int index)
    {
        Outline outline = Scripts[index].GetComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0);
        outline.effectDistance = new Vector2(1, -1);
    }

    private void HighlightScriptOutline(int index)
    {
        Outline outline = Scripts[index].GetComponent<Outline>();
        outline.effectColor = StatusColor[ScriptDataManager.instance.getScript(index).status];
        outline.effectDistance = new Vector2(10, -10);
    }

    // Called after ScriptDragger/ScriptDropper places a script into a slot,
    // so the selection moves on to a script that hasn't been placed yet.
    // Sets selectedScriptIndex to -1 when every script has been placed.
    public void SelectNextUnplacedScript()
    {
        int nextIndex = FindNextUnplacedScript();
        ChangeSelectedScript(selectedScriptIndex, nextIndex);
        selectedScriptIndex = nextIndex;
    }

    public int FindNextUnplacedScript()
    {
        int scriptCount = ScriptDataManager.instance.ScriptCount;
        for (int i = 0; i < scriptCount; i++)
        {
            if (!IsScriptPlaced(i)) return i;
        }
        return -1;
    }

    private bool IsScriptPlaced(int scriptIndex)
    {
        int slotCount = ScriptArrManager.instance.SlotCount;
        for (int slot = 0; slot < slotCount; slot++)
        {
            if (ScriptArrManager.instance.GetScriptAtSlot(slot) == scriptIndex) return true;
        }
        return false;
    }
    void Update()
    {
        HighlightScriptOutline(selectedScriptIndex);
    }
    public void SetScriptThumbnail(int index, Sprite thumbnail)
    {
        ScriptData data = ScriptDataManager.instance.getScript(index);
        data.thumbnail = thumbnail;
        ScriptDataManager.instance.SetScript(index, data);
        Scripts[index].transform.Find("thumbnail").GetComponent<Image>().sprite = thumbnail;
    }
}
