using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScriptExporter : MonoBehaviour
{
    [SerializeField] private Button exportButton;

    private const string NextSceneName = "ResultScene";

    public static List<ScriptData> ExportedScripts { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        ExportedScripts = null;
    }

    private void Awake()
    {
        exportButton.onClick.AddListener(OnExportButtonClicked);
    }

    private void OnExportButtonClicked()
    {
        ExportedScripts = ScriptDataManager.instance.GetScripts();
        SceneManager.LoadScene(NextSceneName);
    }
}
