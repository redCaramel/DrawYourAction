using UnityEngine;
using System.Collections.Generic;

public class ActionMissionManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> missionObjects;
    [SerializeField] private ActionUIManager ui;

    private List<MissionManagerInterface> activeMissions = new List<MissionManagerInterface>();
    private List<MissionData> missionDataList;
    private List<int> lastCurrentValues = new List<int>();
    private bool levelCompleted = false;

    private Mission3_Time timeMission;
    private bool timeMissionStopped = false;

    /// <summary>missionObjects가 모두 클리어되어 스테이지가 끝났을 때(ExecuteFinalClear) 발생.</summary>
    public event System.Action OnAllMissionsCleared;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ActionMissionManager instance {get; private set;}

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

    void Start()
    {
        foreach (var obj in missionObjects)
        {
            if (obj != null)
            {
                MissionManagerInterface mission = obj.GetComponent<MissionManagerInterface>();
                if (mission != null)
                {
                    activeMissions.Add(mission);
                    if (mission is Mission3_Time m3) timeMission = m3;
                }
                else
                {
                    Debug.LogWarning($"{obj.name} dont have script implenented MissionManagerInterface.");
                }
            }
        }
    }

    public void Init(List<MissionData> missionData)
    {
        missionDataList = missionData;
        if (missionDataList.Count != missionObjects.Count)
        {
            Debug.LogWarning("PreviewMissonManager: missionObjects and missionData count mismatch.");
        }

        InjectMissionData();

        lastCurrentValues.Clear();
        foreach (var data in missionDataList)
        {
            lastCurrentValues.Add(data != null ? data.currentValue : 0);
        }
    }

    // missionObjects[i]가 IMissionDataConsumer를 구현하고 있으면, 같은 인덱스의 MissionData를 넘겨준다.
    private void InjectMissionData()
    {
        int count = Mathf.Min(missionObjects.Count, missionDataList.Count);
        for (int i = 0; i < count; i++)
        {
            if (missionObjects[i] == null) continue;

            IMissionDataConsumer consumer = missionObjects[i].GetComponent<IMissionDataConsumer>();
            if (consumer != null)
            {
                consumer.SetMissionData(missionDataList[i]);
            }
        }
    }

    void Update()
    {
        if (levelCompleted) return;

        SyncMissionProgress();
        CheckNonTimerMissionsCleared();

        if (CheckAllMissionsCleared())
        {
            ExecuteFinalClear();
        }
    }

    private void SyncMissionProgress()
    {
        if (missionDataList == null) return;

        bool changed = false;
        int count = Mathf.Min(activeMissions.Count, missionDataList.Count);
        for (int i = 0; i < count; i++)
        {
            bool cleared = activeMissions[i].isClear();
            if (missionDataList[i].isCleared != cleared)
            {
                missionDataList[i].isCleared = cleared;
                changed = true;
            }

            int currentValue = missionDataList[i].currentValue;
            if (i < lastCurrentValues.Count && lastCurrentValues[i] != currentValue)
            {
                lastCurrentValues[i] = currentValue;
                changed = true;
            }
        }

        if (changed)
        {
            ui.Refresh();
        }
    }

    // Mission3(타이머)를 제외한 모든 미션이 클리어되면 타이머를 강제로 멈춘다.
    private void CheckNonTimerMissionsCleared()
    {
        if (timeMission == null || timeMissionStopped || !timeMission.IsTiming) return;

        foreach (var mission in activeMissions)
        {
            if (ReferenceEquals(mission, timeMission)) continue;
            if (!mission.isClear()) return;
        }

        timeMissionStopped = true;
        timeMission.StopTimer();
    }

    private bool CheckAllMissionsCleared()
    {
        if (activeMissions.Count == 0) return false;

        foreach (var mission in activeMissions)
        {
            if (!mission.isClear())
            {
                return false; 
            }
        }

        return true;
    }

    private void ExecuteFinalClear()
    {
        levelCompleted = true;
        ActionMissionResultManager.instance.MissonSuccess();
        OnAllMissionsCleared?.Invoke();
    }
}
