using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScriptExporter : MonoBehaviour
{
    [SerializeField] private Button exportButton;
    [SerializeField] private Sprite nonReadySprite;
    [SerializeField] private Sprite ReadySprite;

    private Image startImage;
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
        NextSceneName = StageImporter.sceneName;
        startImage = exportButton.GetComponent<Image>();
    }

    private void OnExportButtonClicked()
    {
        if(ScriptObjectManager.instance.FindNextUnplacedScript() != -1) return;
        ExportedScripts = BuildOrderedScripts();
        Debug.Log(ExportedScripts.Count);
        StageModeSetting.setMode(false);
        SceneManager.LoadScene(NextSceneName);
    }
    void Update()
    {
        if(ScriptObjectManager.instance.FindNextUnplacedScript() != -1) startImage.sprite = nonReadySprite;
        else startImage.sprite = ReadySprite;
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
