using System.Collections.Generic;
using UnityEngine;

// Attatch this Script to the "Kill All Enemy" Mission Object.
// Mission2_KillAllEnemy가 붙은 적 오브젝트들을 모아서 전체 처치 진행도를 관리한다.
public class Mission2_KillAllEnemyManager : MonoBehaviour, MissionManagerInterface, IMissionDataConsumer
{
    [SerializeField] private List<Mission2_KillAllEnemy> enemies;
    private MissionData missionData; // ActionMissionManager/PreviewMissonManager가 Init() 시점에 주입

    private int clearedCount;
    private bool cleared = false;

    public void SetMissionData(MissionData data)
    {
        missionData = data;
    }

    /// <summary>런타임에 새로 소환된 적(예: GoblinSpawner가 생성한 goblin)을 감시 목록에 추가한다.</summary>
    public void AddEnemy(Mission2_KillAllEnemy enemy)
    {
        if (enemy == null) return;
        enemies.Add(enemy);
    }

    private void Update()
    {
        if (cleared) return;

        UpdateProgress();
    }

    private void UpdateProgress()
    {
        clearedCount = 0;
        foreach (Mission2_KillAllEnemy enemy in enemies)
        {
            if (enemy != null && enemy.isClear())
            {
                clearedCount++;
            }
        }

        if (missionData != null)
        {
            missionData.currentValue = clearedCount;
        }

        if (enemies.Count > 0 && clearedCount >= enemies.Count)
        {
            cleared = true;
        }
    }

    public bool isClear()
    {
        return cleared;
    }
}
