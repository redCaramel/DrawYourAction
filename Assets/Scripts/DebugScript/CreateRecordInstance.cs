using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CreateRecordInstance : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private TMP_InputField scriptCountInputField;
    [SerializeField] private TMP_InputField handCountInputField;

    private const string NextSceneName = "RecordDevelopingScene";

    public static int scriptCount { get; private set; }
    public static int handCount { get; private set; }
    //public static int InstanceCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        scriptCount = 0;
        handCount = 0;
    }

    void Awake()
    {
        createButton.onClick.AddListener(OnCreateButtonClicked);
    }

    private void OnCreateButtonClicked()
    {
        if (!int.TryParse(scriptCountInputField.text, out int countA)) return;
        if (!int.TryParse(handCountInputField.text, out int countB)) return;
        scriptCount = countA;
        handCount = countB;
        SceneManager.LoadScene(NextSceneName);
    }
}
