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

    public static readonly List<Color> ScriptColor = new List<Color>
    {
        new Color(1f, 1f, 1f),
        new Color(241/255f, 255/255f, 139/255f),
        new Color(1f, 0f, 0f),
        new Color(81/255f, 1f, 0f)
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
        float itemHeight = scriptPrefab.GetComponent<RectTransform>().rect.height;
        while (Scripts.Count <= index)
        {
            int i = Scripts.Count;
            GameObject slot = Instantiate(scriptPrefab, scriptListContent);
            RectTransform rect = slot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -i * (itemHeight + scriptSpacing) - scriptSpacing);

            slot.GetComponent<Button>().onClick.AddListener(() => OnScriptClicked(i));
            Scripts.Add(slot);
        }
        RectTransform rt = scriptListContent.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, Scripts.Count * 350);
    }

    // Called by ScriptManager whenever a ScriptData entry is created or changed,
    // so the GameObject view always reflects the latest ScriptData.
    public void UpdateScriptView(int index, ScriptData data)
    {
        EnsureSlot(index);
        Scripts[index].GetComponentInChildren<TMP_Text>().text =
            string.IsNullOrEmpty(data.name) ? "스크립트 " + (index + 1) : data.name;
        SetScriptColor(index, data.status);
    }

    private void OnScriptClicked(int index)
    {
        selectedScriptIndex = index;
    }

    public int GetScriptIndex()
    {
        return selectedScriptIndex;
    }
    private void SetScriptColor(int index, int val)
    {

        Scripts[index].GetComponent<Image>().color = ScriptColor[val];

    }
}
