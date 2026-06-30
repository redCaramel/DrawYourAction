using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptSelector : MonoBehaviour
{
    [SerializeField] private int selectedScriptIndex;
    [SerializeField] private List<Button> Scripts;
    [SerializeField] private int MaxScript;

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
            Scripts[i].onClick.AddListener(() => OnScriptClicked(index));
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
}
