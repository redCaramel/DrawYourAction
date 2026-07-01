using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptSelector : MonoBehaviour
{
    [SerializeField] private int selectedScriptIndex;
    [SerializeField] private List<GameObject> Scripts;
    [SerializeField] private int MaxScript;

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
    public static ScriptSelector instance {get; private set;}

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
        for(int i = 0;i < MaxScript;i++)
        {
            int index = i;
            Scripts[i].GetComponent<Button>().onClick.AddListener(() => OnScriptClicked(index));
            SetScriptColor(i, 1);
        }
        selectedScriptIndex = 0;
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------
    
    public void OnScriptClicked(int index)
    {
        selectedScriptIndex = index;
    }

    public int GetScriptIndex()
    {
        return selectedScriptIndex;
    }
    public void SetScriptColor(int index, int val)
    {

        Scripts[index].GetComponent<Image>().color = ScriptColor[val];

    }
}
