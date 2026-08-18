using UnityEngine;

public class Mission3_Time : MonoBehaviour, MissionManagerInterface, IMissionDataConsumer
{
    [SerializeField] private float timeLimit = 5f;

    private bool cleared = false;
    private bool isTiming = false;
    private float startTime;
    private MissionData missionData; // ActionMissionManager/PreviewMissonManager가 Init() 시점에 주입

    public bool IsTiming => isTiming;

    public void SetMissionData(MissionData data)
    {
        missionData = data;
    }

    private void Start()
    {
        var cutSceneManager = InitialCutSceneManager.instance;
        if (cutSceneManager != null && cutSceneManager.isCutSceneShowing)
        {
            // 컷씬 재생 중이면 컷씬이 끝나는 시점에 맞춰 타이머를 시작
            cutSceneManager.OnCutSceneEnded += HandleCutSceneEnded;
        }
        else
        {
            StartTimer();
        }
    }

    private void OnDestroy()
    {
        if (InitialCutSceneManager.instance != null)
        {
            InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        }
    }

    private void HandleCutSceneEnded()
    {
        InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        StartTimer();
    }

    private void Update()
    {
        if (!isTiming) return;

        float elapsed = Time.time - startTime;
        float remaining = timeLimit - elapsed;

        if (missionData != null)
        {
            missionData.currentValue = Mathf.Max(0, Mathf.CeilToInt(remaining));
        }

        // 남은 시간이 0이 되고 1초가 더 지나면 자동으로 타이머를 종료
        if (elapsed >= timeLimit + 1f)
        {
            StopTimer();
        }
    }

    public void StartTimer()
    {
        startTime = Time.time;
        isTiming = true;
    }

    public void StopTimer()
    {
        if (isTiming && Time.time - startTime <= timeLimit)
        {
            cleared = true;
        }
        else
        {
            ActionMissionResultManager.instance.MissionFailure();
        }
        isTiming = false;
    }

    public bool isClear()
    {
        return cleared;
    }
}
