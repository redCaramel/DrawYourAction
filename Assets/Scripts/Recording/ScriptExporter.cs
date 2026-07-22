using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScriptExporter : MonoBehaviour
{
    [SerializeField] private Button exportButton;

    private string NextSceneName = "ActionScene";

    public static List<ScriptData> ExportedScripts { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        ExportedScripts = null;
    }

    private void Awake()
    {
        exportButton.onClick.AddListener(OnExportButtonClicked);
        NextSceneName += CreateRecordInstance.stageCount;
    }

    private void OnExportButtonClicked()
    {
        ExportedScripts = BuildOrderedScripts();
        Debug.Log(ExportedScripts.Count);
        SceneManager.LoadScene(NextSceneName);
    }

    private List<ScriptData> BuildOrderedScripts()
    {
        List<ScriptData> ordered = new List<ScriptData>();
        int slotCount = ScriptArrManager.instance.SlotCount;
        for (int slot = 0; slot < slotCount; slot++)
        {
            int scriptIndex = ScriptArrManager.instance.GetScriptAtSlot(slot);
            if (scriptIndex < 0) continue;
            ordered.Add(ScriptDataManager.instance.getScript(scriptIndex));
        }
        return ordered;
    }
}
