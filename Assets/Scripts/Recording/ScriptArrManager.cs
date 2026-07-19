using System.Collections.Generic;
using UnityEngine;

public class ScriptArrManager : MonoBehaviour
{
    [SerializeField] private List<int> slotScriptIndices = new List<int>();

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ScriptArrManager instance {get; private set;}

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

    public void SetScriptAtSlot(int slotIndex, int scriptIndex)
    {
        if (slotIndex < 0) return;
        while (slotScriptIndices.Count <= slotIndex)
            slotScriptIndices.Add(-1);
        slotScriptIndices[slotIndex] = scriptIndex;
    }

    public int GetScriptAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotScriptIndices.Count) return -1;
        return slotScriptIndices[slotIndex];
    }
}
