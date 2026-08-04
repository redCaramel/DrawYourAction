using UnityEngine;

public class ProgressBarManager : MonoBehaviour
{
    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ProgressBarManager instance {get; private set;}

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
    [SerializeField] private GameObject progressBar;
    [SerializeField] private GameObject progressStick;

    private RectTransform progressBarRect;

    float maxProgress = 0f;
    float currentProgress = 0f;
    bool isRunning = false;

    private void Start()
    {
        progressBarRect = progressBar.GetComponent<RectTransform>();
        ResetProgress();
        SetMaxDuration(ScriptDataManager.instance.getScript(0).maxDuration);
    }

    private void Update()
    {
        if (!isRunning) return;

        currentProgress += Time.deltaTime;
        if (currentProgress >= maxProgress)
        {
            currentProgress = maxProgress;
            isRunning = false;
        }

        UpdateStickPosition();
    }

    private void UpdateStickPosition()
    {
        float ratio = maxProgress > 0f ? Mathf.Clamp01(currentProgress / maxProgress) : 0f;

        Vector3[] corners = new Vector3[4];
        progressBarRect.GetWorldCorners(corners); // 0: bottom-left, 3: bottom-right
        Vector3 leftEdge = corners[0];
        Vector3 rightEdge = corners[3];

        Vector3 targetPos = Vector3.Lerp(leftEdge, rightEdge, ratio);
        targetPos.y = progressStick.transform.position.y;
        progressStick.transform.position = targetPos;
    }

    public void ResetProgress()
    {
        isRunning = false;
        maxProgress = 0f;
        currentProgress = 0f;
        UpdateStickPosition();
    }
    public void SetMaxDuration(float duration)
    {
        ResetProgress();
        maxProgress = duration;
        currentProgress = 0f;
    }
    public void StartProgress()
    {
        isRunning = true;
    }
}
