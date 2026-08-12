using System.Collections;
using UnityEngine;

/// <summary>
/// Act2 화살 트랩 스포너.
/// InitialCutSceneManager의 컷씬이 끝나면(FinishCutScene -> OnCutSceneEnded) StartArrow()가 호출되어 동작을 시작한다.
/// 이후 주기적으로 플레이어(player)를 기준으로 일정 거리(spawnDistance) 바깥의 지점에서 arrow 프리팹을 소환해
/// 플레이어를 향한 방향으로 발사한다. 발사 주기는 시간이 지날수록 spawnInterval에서 minSpawnInterval까지
/// 점점 좁아지고, 한 번에 발사되는 화살 개수도 minArrowCount에서 maxArrowCount까지 점점 늘어난다.
/// 화살은 일정한 속도(arrowSpeed)로 직진하다가 Ground 레이어에 닿으면
/// 그 자리에서 groundStickDuration만큼 유지된 뒤 사라진다. 발사 직전에는 warningBox 프리팹으로 화살의
/// 이동 경로를 잠깐 표시해 예고한다. ActionMissionManager의 missionObjects가 모두 클리어되면
/// (OnAllMissionsCleared) 더 이상 화살을 발사하지 않는다.
/// arrowCutSceneActivate가 켜져 있으면, 이 컴포넌트가 활성화된 후 cutSceneArrowDelay만큼 지난 시점에
/// cutSceneArrowSpawnPoint 위치에서 정확히 아래쪽으로 향하는 화살을 한 번 발사한다(경고 포함, 위 주기적
/// 발사와는 별개의 1회성 연출).
/// </summary>
public class Act2Arrows : MonoBehaviour
{
    [Header("타겟 및 프리팹")]
    [SerializeField] private Transform player; // 화살이 노리는 대상. 비워두면 PlayerController.instance를 따라간다.
    [SerializeField] private GameObject arrowPrefab; // 소환할 화살 프리팹 (Collider2D + ArrowHitbox 필요)
    [SerializeField] private GameObject warningBoxPrefab; // 화살이 지나갈 경로를 미리 표시하는 경고용 프리팹

    [Header("발사 주기")]
    [SerializeField] private float spawnInterval = 2f; // 발사를 시작했을 때(경과 시간 0)의 발사 주기
    [SerializeField] private float minSpawnInterval = 0.5f; // 시간이 지나며 좁아지는 발사 주기의 하한
    [SerializeField] private float intervalShrinkDuration = 30f; // spawnInterval에서 minSpawnInterval까지 줄어드는 데 걸리는 시간
    [SerializeField] private float firstSpawnDelay = 1f; // StartArrow() 호출 이후 첫 화살이 발사되기까지의 대기 시간

    [Header("동시 발사 개수")]
    [SerializeField] private int minArrowCount = 1; // 발사를 시작했을 때(경과 시간 0) 한 번에 발사되는 화살 개수
    [SerializeField] private int maxArrowCount = 3; // 시간이 지나며 늘어나는 화살 개수의 상한
    [SerializeField] private float countGrowDuration = 30f; // minArrowCount에서 maxArrowCount까지 늘어나는 데 걸리는 시간

    [Header("거리 / 속도")]
    [SerializeField] private float spawnDistance = 10f; // 플레이어로부터 이 거리만큼 떨어진 지점에서 화살이 소환된다
    [SerializeField] private float arrowSpeed = 8f; // 화살이 날아가는 속도

    [Header("소환 각도 (도, 0=오른쪽, 90=위쪽 기준 반시계 방향)")]
    [SerializeField] private float minSpawnAngle = 0f;
    [SerializeField] private float maxSpawnAngle = 360f;

    [Header("경고 표시")]
    [SerializeField] private float warningDuration = 0.5f; // 발사 전 경로 경고(warningBox)가 표시되는 시간

    [Header("착지 후 처리")]
    [SerializeField] private float groundStickDuration = 2f; // Ground에 닿은 뒤 사라지기까지 유지되는 시간
    [SerializeField] private float maxFlightTime = 6f; // Ground에 닿지 못했을 때를 대비한 안전 소멸 시간

    [Header("컷씬용 특수 화살 (1회성)")]
    [SerializeField] private bool arrowCutSceneActivate = false; // 켜져 있을 때만 아래의 1회성 연출 화살을 발동한다
    [SerializeField] private Transform cutSceneArrowSpawnPoint; // 이 화살이 소환될 정확한 위치
    [SerializeField] private float cutSceneArrowDelay = 2f; // 이 컴포넌트가 활성화된 후 화살이 발사되기까지의 대기 시간

    private Coroutine spawnRoutine;
    private bool isFiring = false;
    private float firingStartTime; // StartArrow()가 호출된 시각. 발사 주기를 줄이는 데 기준이 되는 경과 시간을 계산할 때 사용

    private void Awake()
    {
        if (player == null && PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }
    }

    private void OnEnable()
    {
        if (arrowCutSceneActivate)
        {
            StartCoroutine(Co_CutSceneArrow());
        }
    }

    private void Start()
    {
        var cutSceneManager = InitialCutSceneManager.instance;
        if (cutSceneManager != null && cutSceneManager.isCutSceneShowing)
        {
            // 컷씬 재생 중에는 대기하고, 컷씬이 끝나는 시점(FinishCutScene)에 맞춰 StartArrow를 호출한다.
            cutSceneManager.OnCutSceneEnded += HandleCutSceneEnded;
        }
        else
        {
            StartArrow();
        }

        if (ActionMissionManager.instance != null)
        {
            // 모든 미션이 클리어되면 더 이상 화살을 발사하지 않는다.
            ActionMissionManager.instance.OnAllMissionsCleared += HandleAllMissionsCleared;
        }
    }

    private void OnDestroy()
    {
        if (InitialCutSceneManager.instance != null)
        {
            InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        }

        if (ActionMissionManager.instance != null)
        {
            ActionMissionManager.instance.OnAllMissionsCleared -= HandleAllMissionsCleared;
        }

        StopArrow();
    }

    private void HandleCutSceneEnded()
    {
        InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        StartArrow();
    }

    private void HandleAllMissionsCleared()
    {
        ActionMissionManager.instance.OnAllMissionsCleared -= HandleAllMissionsCleared;
        StopArrow();
    }

    /// <summary>컷씬이 끝난 시점에 호출되어 화살을 주기적으로 발사하기 시작한다.</summary>
    public void StartArrow()
    {
        if (isFiring) return;
        if (arrowPrefab == null || player == null) return;

        isFiring = true;
        firingStartTime = Time.time;
        spawnRoutine = StartCoroutine(Co_SpawnLoop());
    }

    /// <summary>진행 중인 화살 발사를 멈춘다.</summary>
    public void StopArrow()
    {
        isFiring = false;

        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator Co_SpawnLoop()
    {
        if (firstSpawnDelay > 0f) yield return new WaitForSeconds(firstSpawnDelay);

        while (isFiring)
        {
            yield return StartCoroutine(Co_FireVolley());
            yield return new WaitForSeconds(GetCurrentSpawnInterval());
        }
    }

    // 발사를 시작한 뒤 경과한 시간(firingStartTime 기준)에 비례해 발사 주기를 spawnInterval에서
    // minSpawnInterval까지 선형으로 좁힌다. intervalShrinkDuration이 지나면 minSpawnInterval로 고정된다.
    private float GetCurrentSpawnInterval()
    {
        if (intervalShrinkDuration <= 0f) return minSpawnInterval;

        float elapsed = Time.time - firingStartTime;
        float t = Mathf.Clamp01(elapsed / intervalShrinkDuration);
        return Mathf.Lerp(spawnInterval, minSpawnInterval, t);
    }

    // 발사를 시작한 뒤 경과한 시간(firingStartTime 기준)에 비례해 한 번에 발사되는 화살 개수를
    // minArrowCount에서 maxArrowCount까지 늘린다. countGrowDuration이 지나면 maxArrowCount로 고정된다.
    private int GetCurrentArrowCount()
    {
        if (countGrowDuration <= 0f) return maxArrowCount;

        float elapsed = Time.time - firingStartTime;
        float t = Mathf.Clamp01(elapsed / countGrowDuration);
        return Mathf.RoundToInt(Mathf.Lerp(minArrowCount, maxArrowCount, t));
    }

    // 현재 시점의 화살 개수(GetCurrentArrowCount)만큼 소환 위치/방향을 각각 무작위로 골라, warningBox로
    // 경로를 동시에 예고한 뒤 한꺼번에 화살을 소환해 발사한다.
    private IEnumerator Co_FireVolley()
    {
        if (player == null || arrowPrefab == null) yield break;

        int count = GetCurrentArrowCount();
        Vector2[] spawnPositions = new Vector2[count];
        Vector2[] fireDirs = new Vector2[count];
        GameObject[] warnings = new GameObject[count];

        // ===== 경고 표시: 화살들이 지나갈 경로를 warningBox로 미리 동시에 표시 =====
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(minSpawnAngle, maxSpawnAngle) * Mathf.Deg2Rad;
            Vector2 offsetDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPos = (Vector2)player.position + offsetDir * spawnDistance;
            Vector2 fireDir = ((Vector2)player.position - spawnPos).normalized;

            spawnPositions[i] = spawnPos;
            fireDirs[i] = fireDir;

            if (warningBoxPrefab != null)
            {
                float fireAngleDeg = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
                warnings[i] = Instantiate(warningBoxPrefab, spawnPos, Quaternion.Euler(0f, 0f, fireAngleDeg));
            }
        }

        if (warningDuration > 0f) yield return new WaitForSeconds(warningDuration);

        for (int i = 0; i < count; i++)
        {
            if (warnings[i] != null) Destroy(warnings[i]);
        }
        // ============================================================

        // ===== 발사: 화살들을 각자의 fireDir 방향으로 동시에 진행시킨다 =====
        for (int i = 0; i < count; i++)
        {
            float fireAngleDeg = Mathf.Atan2(fireDirs[i].y, fireDirs[i].x) * Mathf.Rad2Deg;
            GameObject arrow = Instantiate(arrowPrefab, spawnPositions[i], Quaternion.Euler(0f, 0f, fireAngleDeg));
            StartCoroutine(Co_MoveArrow(arrow, fireDirs[i]));
        }
        // ============================================================
    }

    // arrowCutSceneActivate가 켜져 있을 때, 활성화 후 cutSceneArrowDelay만큼 지난 시점에 cutSceneArrowSpawnPoint
    // 위치에서 정확히 아래쪽(Vector2.down)으로 향하는 화살을 한 번 발사한다. 다른 화살과 마찬가지로
    // warningBoxPrefab으로 경로를 먼저 예고한 뒤 발사한다.
    private IEnumerator Co_CutSceneArrow()
    {
        if (cutSceneArrowSpawnPoint == null || arrowPrefab == null) yield break;

        if (cutSceneArrowDelay > 0f) yield return new WaitForSeconds(cutSceneArrowDelay);

        Vector2 spawnPos = cutSceneArrowSpawnPoint.position;
        Vector2 fireDir = Vector2.down;
        float fireAngleDeg = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;

        // ===== 경고 표시: 정확히 아래쪽으로 향하는 경로를 warningBox로 미리 표시 =====
        GameObject warning = null;
        if (warningBoxPrefab != null)
        {
            warning = Instantiate(warningBoxPrefab, spawnPos, Quaternion.Euler(0f, 0f, fireAngleDeg));
        }

        if (warningDuration > 0f) yield return new WaitForSeconds(warningDuration);

        if (warning != null) Destroy(warning);
        // ============================================================

        // ===== 발사: 화살을 소환해 정확히 아래쪽으로 진행시킨다 =====
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0f, 0f, fireAngleDeg));
        StartCoroutine(Co_MoveArrow(arrow, fireDir));
        // ============================================================
    }

    // 화살을 fireDir 방향으로 arrowSpeed만큼 진행시키다가 Ground 레이어에 닿으면 그 자리에서
    // groundStickDuration만큼 유지된 뒤 사라진다. Ground에 닿지 못한 채 maxFlightTime이 지나면
    // 안전장치로 그대로 소멸시킨다.
    private IEnumerator Co_MoveArrow(GameObject arrow, Vector2 fireDir)
    {
        LayerMask groundMask = LayerMask.GetMask("Ground");
        float elapsed = 0f;
        bool grounded = false;

        while (arrow != null && !grounded && elapsed < maxFlightTime)
        {
            float step = arrowSpeed * Time.deltaTime;
            Vector2 currentPos = arrow.transform.position;

            RaycastHit2D hit = Physics2D.Raycast(currentPos, fireDir, step, groundMask);
            if (hit.collider != null)
            {
                arrow.transform.position = hit.point;
                grounded = true;
            }
            else
            {
                arrow.transform.position = currentPos + fireDir * step;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        arrow.GetComponent<Collider2D>().enabled = false;
        if (arrow == null) yield break;

        yield return new WaitForSeconds(groundStickDuration);

        if (arrow != null) Destroy(arrow);
    }
}
