using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CreateRecordInstance : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private TMP_InputField instanceCountInputField;

    private const string NextSceneName = "RecordDevelopingScene";

    public static int InstanceCount { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        InstanceCount = 0;
    }

    void Awake()
    {
        createButton.onClick.AddListener(OnCreateButtonClicked);
    }

    private void OnCreateButtonClicked()
    {
        if (!int.TryParse(instanceCountInputField.text, out int count)) return;

        InstanceCount = count;
        SceneManager.LoadScene(NextSceneName);
    }
}
