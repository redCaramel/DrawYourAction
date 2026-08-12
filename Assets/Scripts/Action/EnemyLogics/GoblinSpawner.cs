using System.Collections;
using UnityEngine;

/// <summary>
/// 두 지점을 번갈아가며 goblin 프리팹을 주기적으로 소환하는 스포너.
/// InitialCutSceneManager의 컷씬이 끝나면(FinishCutScene -> OnCutSceneEnded) StartSpawning()이 호출되어 동작을 시작한다.
/// 소환된 goblin은 자기 자신의 GameObject에 붙어있는 Mission2_KillAllEnemyManager의 감시 목록(enemies)에
/// 자동으로 등록된다. maxSpawnCount만큼 소환하고 나면 더 이상 소환하지 않는다.
/// ActionMissionResultManager의 MissonSuccess/MissionFailure이 호출된 이후에는(OnMissionFinished)
/// 더 이상 소환하지 않는다.
/// </summary>
public class GoblinSpawner : MonoBehaviour
{
    [Header("프리팹 및 소환 지점")]
    [SerializeField] private GameObject goblinPrefab;
    [SerializeField] private Transform spawnPointA;
    [SerializeField] private Transform spawnPointB;

    [Header("소환 주기 / 시간")]
    [SerializeField] private float spawnInterval = 3f; // goblin을 소환하는 주기
    [SerializeField] private float firstSpawnDelay = 1f; // StartSpawning() 호출 이후 첫 소환까지의 대기 시간

    [Header("최대 소환 수")]
    [SerializeField] private int maxSpawnCount = 20; // 이 개수만큼 소환하고 나면 더 이상 소환하지 않는다

    private Mission2_KillAllEnemyManager killAllEnemyManager; // 같은 GameObject에 붙어있는 미션 매니저. 소환된 goblin을 자동으로 등록한다.
    private Coroutine spawnRoutine;
    private bool isSpawning = false;
    private bool spawnAtPointA = true; // 다음 소환을 두 지점 중 어느 쪽에서 할지 (번갈아가며 반전)
    private int spawnedCount = 0;

    private void Awake()
    {
        killAllEnemyManager = GetComponent<Mission2_KillAllEnemyManager>();
        if (killAllEnemyManager == null)
        {
            Debug.LogWarning($"{name}: Mission2_KillAllEnemyManager가 같은 GameObject에 없어 소환된 goblin을 등록할 수 없습니다.");
        }
    }

    private void Start()
    {
        var cutSceneManager = InitialCutSceneManager.instance;
        if (cutSceneManager != null && cutSceneManager.isCutSceneShowing)
        {
            // 컷씬 재생 중에는 대기하고, 컷씬이 끝나는 시점(FinishCutScene)에 맞춰 StartSpawning을 호출한다.
            cutSceneManager.OnCutSceneEnded += HandleCutSceneEnded;
        }
        else
        {
            StartSpawning();
        }

        if (ActionMissionResultManager.instance != null)
        {
            // 미션이 성공/실패로 끝나면 더 이상 소환하지 않는다.
            ActionMissionResultManager.instance.OnMissionFinished += HandleMissionFinished;
        }
    }

    private void OnDestroy()
    {
        if (InitialCutSceneManager.instance != null)
        {
            InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        }

        if (ActionMissionResultManager.instance != null)
        {
            ActionMissionResultManager.instance.OnMissionFinished -= HandleMissionFinished;
        }

        StopSpawning();
    }

    private void HandleCutSceneEnded()
    {
        InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        StartSpawning();
    }

    private void HandleMissionFinished()
    {
        if (ActionMissionResultManager.instance != null)
        {
            ActionMissionResultManager.instance.OnMissionFinished -= HandleMissionFinished;
        }

        StopSpawning();
    }

    /// <summary>컷씬이 끝난 시점에 호출되어 두 지점을 번갈아가며 goblin을 주기적으로 소환하기 시작한다.</summary>
    public void StartSpawning()
    {
        if (isSpawning) return;
        if (goblinPrefab == null || spawnPointA == null || spawnPointB == null) return;
        if (spawnedCount >= maxSpawnCount) return;

        isSpawning = true;
        spawnRoutine = StartCoroutine(Co_SpawnLoop());
    }

    /// <summary>진행 중인 goblin 소환을 멈춘다.</summary>
    public void StopSpawning()
    {
        isSpawning = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator Co_SpawnLoop()
    {
        if (firstSpawnDelay > 0f) yield return new WaitForSeconds(firstSpawnDelay);

        while (isSpawning)
        {
            SpawnGoblin();

            // maxSpawnCount만큼 소환했으면 더 이상 대기하지 않고 즉시 멈춘다.
            if (spawnedCount >= maxSpawnCount)
            {
                isSpawning = false;
                spawnRoutine = null;
                yield break;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // 두 지점을 번갈아가며 goblinPrefab을 소환하고, 소환된 goblin을 killAllEnemyManager의 감시 목록에 등록한다.
    private void SpawnGoblin()
    {
        Transform spawnPoint = spawnAtPointA ? spawnPointA : spawnPointB;
        spawnAtPointA = !spawnAtPointA;

        GameObject goblin = Instantiate(goblinPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedCount++;

        if (killAllEnemyManager != null)
        {
            Mission2_KillAllEnemy enemy = goblin.GetComponent<Mission2_KillAllEnemy>();
            if (enemy != null) killAllEnemyManager.AddEnemy(enemy);
        }
    }
}
