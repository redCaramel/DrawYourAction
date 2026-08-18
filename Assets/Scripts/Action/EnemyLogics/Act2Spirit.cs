using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attatch this Script to the Act2 Spirit enemy object, along with Mission2_KillAllEnemy.
[RequireComponent(typeof(Mission2_KillAllEnemy))]
public class Act2Spirit : MonoBehaviour
{
    [Header("타겟")]
    [SerializeField] private Transform player; // 비워두면 PlayerController.instance를 따라간다.

    [Header("공격")]
    [SerializeField, Range(0f, 1f)] private float atk3Chance = 0.5f; // atk1 대신 atk3를 사용할 확률
    [SerializeField] private float atk3OnlyDistance = 5f; // player와의 거리가 이 값을 넘어서면 atk1 대신 무조건 atk3만 사용한다.
    [SerializeField] private float attackCooldown = 1.5f; // 한 atk가 끝난 후 다음 atk를 시작하기까지의 대기 시간
    [SerializeField] private string idleStateName = "SpiritIdleAnim";
    [SerializeField] private string atk1_2StateName = "SpiritAtk1_2Anim";
    [SerializeField] private string atk3_2StateName = "SpiritAtk3_2Anim";

    [Header("히트박스 - atk1")]
    [SerializeField] private GameObject atk1HitboxObject; // atk1 실제 충돌 판정 오브젝트. SpiritAtk1_2Anim 동안에만 활성화된다. 바라보는 방향의 전방에 위치한다.
    [SerializeField] private GameObject atk1WarningObject; // atk1 경고 표시용 오브젝트. SpiritAtk1_1Anim 동안에만 표시된다. 충돌 판정은 없다.
    [SerializeField] private float atk1HitboxDuration = 0.2f;

    [Header("히트박스 - atk3")]
    [SerializeField] private GameObject atk3HitboxObject; // atk3 실제 충돌 판정 오브젝트. SpiritAtk3_2Anim 동안에만 활성화된다. player의 x좌표에 소환되고 y좌표는 고정이다.
    [SerializeField] private GameObject atk3WarningObject; // atk3 경고 표시용 오브젝트. SpiritAtk3_1Anim 동안에만 표시된다. atk3를 시작한 시점의 player x좌표에 고정된다.
    [SerializeField] private float atk3HitboxDuration = 0.2f;
    [SerializeField] private float atk3HitboxPopUpDistance = 1f; // 히트박스가 목표 위치보다 이만큼 아래에서 시작해 튀어오른다.
    [SerializeField] private float atk3HitboxPopUpDuration = 0.1f; // 아래에서 목표 위치까지 튀어오르는 데 걸리는 시간 (atk3HitboxDuration보다 짧아야 한다)

    [Header("히트박스 - atk2")]
    [SerializeField] private Transform atk2SpawnOrigin; // atk2 첫 프리팹이 소환되는 위치. 비워두면 자기 자신의 위치를 사용한다.
    [SerializeField] private GameObject atk2WarningPrefab; // atk2 경고 표시용 프리팹. 충돌 판정은 없다.
    [SerializeField] private GameObject atk2HitboxPrefab; // atk2 실제 충돌 판정 프리팹. SpiritAtkHitbox가 부착되어 있어야 한다.
    [SerializeField] private int atk2SpawnCount = 5; // 전방으로 소환할 프리팹 개수
    [SerializeField] private Vector3 atk2SpawnInterval = new Vector3(1f, 0f, 0f); // 소환 순서가 하나 늘어날 때마다 atk2SpawnOrigin에 누적되는 위치 오프셋
    [SerializeField] private bool atk2MirrorIntervalWithFacing = false; // true면 바라보는 방향(facingDir)에 따라 atk2SpawnInterval.x의 부호를 반전시킨다.
    [SerializeField] private float atk2SpawnCadence = 0.3f; // 새 경고 오브젝트를 전방에 추가로 소환하는 시간 간격. 이전에 소환된 오브젝트의 진행 상태와는 무관하다.
    [SerializeField] private float atk2WarningDuration = 0.5f; // 경고 오브젝트 하나가 소환된 뒤, 그 오브젝트만 사라지고 같은 위치에 히트박스가 소환되기까지 걸리는 시간 (다른 오브젝트와 독립적으로 진행)
    [SerializeField] private float atk2HitboxDuration = 0.2f; // 히트박스 프리팹이 소환된 채로 유지되는 시간
    [SerializeField] private float atk2HitboxPopUpDistance = 1f; // atk3 히트박스와 마찬가지로, 히트박스가 목표 위치보다 이만큼 아래에서 시작해 튀어오른다.
    [SerializeField] private float atk2HitboxPopUpDuration = 0.1f; // 아래에서 목표 위치까지 튀어오르는 데 걸리는 시간 (atk2HitboxDuration보다 짧아야 한다)

    private Animator anim;
    private SpriteRenderer sprite;
    private Mission2_KillAllEnemy mission;
    private Coroutine actionRoutineHandle;
    private Coroutine atk1HitboxRoutine;
    private Coroutine atk3HitboxRoutine;

    private Vector3 atk1HitboxLocalPos; // atk1HitboxObject의 원래(오른쪽 기준) 로컬 위치
    private Vector3 atk1WarningLocalPos; // atk1WarningObject의 원래(오른쪽 기준) 로컬 위치
    private float atk3HitboxWorldY, atk3HitboxWorldZ; // atk3HitboxObject의 고정 y/z (원래 위치에서 가져옴)
    private float atk3WarningWorldY, atk3WarningWorldZ; // atk3WarningObject의 고정 y/z (원래 위치에서 가져옴)

    private SpiritAtkHitbox atk1Hitbox;
    private SpiritAtkHitbox atk3Hitbox;
    private Animator atk1HitboxAnim; // atk1HitboxObject에 부착된 Animator. 공격 순간에 "atk" 트리거를 발동시킨다.
    private readonly List<Coroutine> atk2InstanceRoutines = new List<Coroutine>(); // 현재 진행 중인 atk2 경고/히트박스 인스턴스별 코루틴 (동시에 여러 개 존재할 수 있다)
    private readonly List<GameObject> atk2ActiveInstances = new List<GameObject>(); // 현재 씬에 소환되어 있는 atk2 경고/히트박스 프리팹 인스턴스
    private bool atk2TriggerFired; // atk2 진행 중 첫 히트박스가 소환되어 atk2 트리거를 이미 발동시켰는지
    private int atk2AliveCount; // 생애주기가 아직 끝나지 않은 atk2 인스턴스 개수. 0이 되면 atk2 행동이 끝난다.

    // hp 비율이 75%, 50%, 25%로 줄어들 때마다 하나씩 쌓이고, atk2를 한 번 수행할 때마다 하나씩 소모된다.
    private static readonly float[] hpAtk2Thresholds = { 0.75f, 0.5f, 0.25f };
    private int nextHpThresholdIndex = 0;
    private int pendingAtk2Count = 0;

    private bool isActing = false; // StartAction() 이후 공격 루프를 수행 중인지
    private bool isAttacking = false; // atk1/atk2/atk3 모션을 진행 중인지 (방향전환 정지)
    private bool isDead = false;
    private int facingDir = 1; // 마지막으로 바라본 방향 (-1: 왼쪽, 1: 오른쪽)

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        mission = GetComponent<Mission2_KillAllEnemy>();
        anim.speed = 1f;

        if (player == null && PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }

        if (atk1HitboxObject != null)
        {
            atk1HitboxLocalPos = atk1HitboxObject.transform.localPosition;
            atk1Hitbox = atk1HitboxObject.GetComponent<SpiritAtkHitbox>();
            atk1HitboxAnim = atk1HitboxObject.GetComponent<Animator>();
            atk1HitboxObject.SetActive(false);
        }

        if (atk1WarningObject != null)
        {
            atk1WarningLocalPos = atk1WarningObject.transform.localPosition;
            atk1WarningObject.SetActive(false);
        }

        if (atk3HitboxObject != null)
        {
            atk3HitboxWorldY = atk3HitboxObject.transform.position.y;
            atk3HitboxWorldZ = atk3HitboxObject.transform.position.z;
            atk3Hitbox = atk3HitboxObject.GetComponent<SpiritAtkHitbox>();
            atk3HitboxObject.SetActive(false);
        }

        if (atk3WarningObject != null)
        {
            atk3WarningWorldY = atk3WarningObject.transform.position.y;
            atk3WarningWorldZ = atk3WarningObject.transform.position.z;
            atk3WarningObject.SetActive(false);
        }

    }

    private void Start()
    {
        var cutSceneManager = InitialCutSceneManager.instance;
        if (cutSceneManager != null && cutSceneManager.isCutSceneShowing)
        {
            // 컷씬 재생 중에는 대기하고, 컷씬이 끝나는 시점에 맞춰 StartAction을 호출한다.
            cutSceneManager.OnCutSceneEnded += HandleCutSceneEnded;
        }
        else
        {
            StartAction();
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
        StartAction();
    }

    private void Update()
    {
        if (isDead) return;

        if (mission != null && mission.isClear())
        {
            StopAction();
            Die();
            return;
        }

        CheckHpThresholds();

        // 제자리에 고정되어 있지만, 공격 중이 아닐 때는 player 쪽을 계속 바라본다.
        if (!isAttacking && player != null)
        {
            facingDir = player.position.x >= transform.position.x ? 1 : -1;
            UpdateFacing(facingDir);
        }
    }

    // hp 비율이 75%/50%/25% 구간을 새로 통과했는지 확인하고, 통과했다면 atk2 예약 개수를 늘린다.
    private void CheckHpThresholds()
    {
        if (mission == null) return;

        float ratio = mission.HpRatio;
        while (nextHpThresholdIndex < hpAtk2Thresholds.Length && ratio <= hpAtk2Thresholds[nextHpThresholdIndex])
        {
            nextHpThresholdIndex++;
            pendingAtk2Count++;
        }
    }

    // 스피릿(적)을 활성화시켜 공격 루프를 시작하게 한다.
    public void StartAction()
    {
        if (isDead) return;
        isActing = true;

        if (actionRoutineHandle == null)
        {
            actionRoutineHandle = StartCoroutine(ActionLoop());
        }
    }

    // 공격/히트박스 등 진행 중인 모든 행동을 즉시 멈춘다.
    public void StopAction()
    {
        isActing = false;
        isAttacking = false;

        if (actionRoutineHandle != null)
        {
            StopCoroutine(actionRoutineHandle);
            actionRoutineHandle = null;
        }

        if (atk1HitboxRoutine != null)
        {
            StopCoroutine(atk1HitboxRoutine);
            atk1HitboxRoutine = null;
        }

        if (atk3HitboxRoutine != null)
        {
            StopCoroutine(atk3HitboxRoutine);
            atk3HitboxRoutine = null;
        }

        if (atk1HitboxObject != null) atk1HitboxObject.SetActive(false);
        if (atk1WarningObject != null) atk1WarningObject.SetActive(false);
        if (atk3HitboxObject != null) atk3HitboxObject.SetActive(false);
        if (atk3WarningObject != null) atk3WarningObject.SetActive(false);

        // atk2 진행 중이었다면, 개별적으로 돌고 있던 인스턴스별 코루틴을 모두 멈추고 소환되어 있던
        // 프리팹 인스턴스를 정리한다.
        foreach (var routine in atk2InstanceRoutines)
        {
            if (routine != null) StopCoroutine(routine);
        }
        atk2InstanceRoutines.Clear();

        foreach (var instance in atk2ActiveInstances)
        {
            if (instance != null) Destroy(instance);
        }
        atk2ActiveInstances.Clear();
    }

    private void UpdateFacing(int dir)
    {
        if (sprite != null) sprite.flipX = dir < 0;
    }

    // 제자리에 고정된 채로 atk1/atk2/atk3을 반복 수행하는 메인 루프. 한 atk가 끝날 때마다 attackCooldown만큼 대기한다.
    private IEnumerator ActionLoop()
    {
        while (isActing)
        {
            if (pendingAtk2Count > 0)
            {
                pendingAtk2Count--;
                yield return StartCoroutine(Atk2Routine());
            }
            else if (IsPlayerOutOfAtk3OnlyRange() || Random.value < atk3Chance)
            {
                yield return StartCoroutine(Atk3Routine());
            }
            else
            {
                yield return StartCoroutine(Atk1Routine());
            }

            yield return new WaitForSeconds(attackCooldown);
        }

        actionRoutineHandle = null;
    }

    // player와의 거리가 atk3OnlyDistance를 넘어서면 atk1 대신 무조건 atk3를 사용하도록 true를 반환한다.
    private bool IsPlayerOutOfAtk3OnlyRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) > atk3OnlyDistance;
    }

    // atk1 트리거를 발동시키고, SpiritAtk1_1Anim -> SpiritAtk1_2Anim이 모두 끝날 때까지 대기한다.
    private IEnumerator Atk1Routine()
    {
        isAttacking = true;
        anim.SetTrigger("atk1");

        // ===== 공격 대기시간(SpiritAtk1_1Anim) 구간: 경고용 오브젝트만 표시하고, 실제 판정은 없다 =====
        BeginAtk1Warning();
        // ============================================================================

        // SpiritAtk1_1Anim이 끝나고 SpiritAtk1_2Anim으로 넘어가는 시점까지 대기 (= 공격 대기시간 종료, 공격 순간 시작)
        yield return StartCoroutine(WaitForStateEnter(atk1_2StateName));

        // ===== SpiritAtk1_2Anim 재생 구간(공격 순간): 경고 표시를 끄고 실제 판정 히트박스를 활성화한다 =====
        ShowAtk1Hitbox();
        AudioManager.instance.PlaySFX(SFXType.smash1);
        // ============================================================================

        // SpiritAtk1_2Anim이 끝나고 Idle로 돌아오는 시점까지 대기 (= SpiritAtk1_2Anim 종료 시점)
        yield return StartCoroutine(WaitForStateEnter(idleStateName));

        isAttacking = false;
    }

    // atk3 트리거를 발동시키고, SpiritAtk3_1Anim -> SpiritAtk3_2Anim이 모두 끝날 때까지 대기한다.
    // atk1과 동일한 흐름이지만, 히트박스/경고는 방향이 아니라 atk3를 시작한 시점의 player x좌표에 고정된다 (y좌표는 원래 값 유지).
    private IEnumerator Atk3Routine()
    {
        isAttacking = true;
        float targetX = player != null ? player.position.x : transform.position.x; // atk3를 시작하는 시점의 player x좌표로 고정한다.
        anim.SetTrigger("atk3");

        // ===== 공격 대기시간(SpiritAtk3_1Anim) 구간: 경고용 오브젝트만 표시하고, 실제 판정은 없다 =====
        BeginAtk3Warning(targetX);
        // ============================================================================

        // SpiritAtk3_1Anim이 끝나고 SpiritAtk3_2Anim으로 넘어가는 시점까지 대기 (= 공격 대기시간 종료, 공격 순간 시작)
        yield return StartCoroutine(WaitForStateEnter(atk3_2StateName));

        // ===== SpiritAtk3_2Anim 재생 구간(공격 순간): 경고 표시를 끄고 실제 판정 히트박스를 활성화한다 =====
        ShowAtk3Hitbox(targetX);
        AudioManager.instance.PlaySFX(SFXType.smash1);
        // =========================================================
        // ============================================================================

        // SpiritAtk3_2Anim이 끝나고 Idle로 돌아오는 시점까지 대기 (= SpiritAtk3_2Anim 종료 시점)
        yield return StartCoroutine(WaitForStateEnter(idleStateName));

        isAttacking = false;
    }

    // atk2SpawnOrigin(비어있으면 자기 자신 위치)을 시작점으로, atk2SpawnCadence 간격마다 atk2SpawnInterval만큼
    // 누적된 위치에 경고 프리팹을 전방으로 계속 추가 소환한다(atk2SpawnCount개). 각 경고 오브젝트는 다른
    // 오브젝트의 진행 상태와 무관하게 독립적으로 자신의 타이머만으로 atk2WarningDuration 뒤에 사라지고
    // 같은 위치에 히트박스를 소환하며, 그 히트박스는 atk2HitboxDuration 뒤에 사라진다. atk2 트리거는
    // (소환 순서와 무관하게) 가장 먼저 활성화되는 히트박스 시점에 한 번만 발동시킨다. 마지막으로 소환된
    // 오브젝트까지 모든 인스턴스의 생애주기가 끝나면 행동을 종료한다.
    private IEnumerator Atk2Routine()
    {
        isAttacking = true;
        atk2TriggerFired = false;
        atk2AliveCount = 0;
        atk2InstanceRoutines.Clear();

        Vector3 origin = atk2SpawnOrigin != null ? atk2SpawnOrigin.position : transform.position;
        int mirrorDir = atk2MirrorIntervalWithFacing ? facingDir : 1;
        Vector3 intervalVec = new Vector3(atk2SpawnInterval.x * mirrorDir, atk2SpawnInterval.y, atk2SpawnInterval.z);

        for (int i = 0; i < atk2SpawnCount; i++)
        {
            Vector3 spawnPos = origin + intervalVec * i;
            atk2AliveCount++;
            atk2InstanceRoutines.Add(StartCoroutine(Atk2InstanceRoutine(spawnPos)));
            if(i==3) AudioManager.instance.PlaySFX(SFXType.scream);
            if (i < atk2SpawnCount - 1)
            {
                yield return new WaitForSeconds(atk2SpawnCadence);
            }
        }

        // 가장 마지막으로 소환된 오브젝트를 포함해, 진행 중인 인스턴스가 모두 끝날 때까지 대기한다.
        while (atk2AliveCount > 0)
        {
            yield return null;
        }

        atk2InstanceRoutines.Clear();
        attackCooldown -= 0.25f;
        anim.speed += 0.2f;

        isAttacking = false;
    }

    // atk2 경고/히트박스 오브젝트 하나의 생애주기(경고 표시 -> 히트박스 소환 -> 소멸)를 다른 인스턴스와
    // 독립적으로 처리한다.
    private IEnumerator Atk2InstanceRoutine(Vector3 spawnPos)
    {
        GameObject warningInstance = null;
        if (atk2WarningPrefab != null)
        {
            warningInstance = Instantiate(atk2WarningPrefab, spawnPos, Quaternion.identity);
            atk2ActiveInstances.Add(warningInstance);
        }

        yield return new WaitForSeconds(atk2WarningDuration);

        if (warningInstance != null)
        {
            atk2ActiveInstances.Remove(warningInstance);
            Destroy(warningInstance);
        }

        if (!atk2TriggerFired)
        {
            atk2TriggerFired = true;
            anim.SetTrigger("atk2");
        }

        if (atk2HitboxPrefab != null)
        {
            // atk3 히트박스와 마찬가지로, 목표 위치보다 atk2HitboxPopUpDistance만큼 아래에서 소환되어
            // atk2HitboxPopUpDuration 동안 목표 위치까지 솟아오른다.
            Vector3 startPos = spawnPos + Vector3.down * atk2HitboxPopUpDistance;
            GameObject hitboxInstance = Instantiate(atk2HitboxPrefab, startPos, Quaternion.identity);
            atk2ActiveInstances.Add(hitboxInstance);

            var hitboxComp = hitboxInstance.GetComponent<SpiritAtkHitbox>();
            if (hitboxComp != null)
            {
                hitboxComp.SetSpirit(this);
                hitboxComp.ClearDetected();
            }

            float popUpDuration = Mathf.Min(atk2HitboxPopUpDuration, atk2HitboxDuration);
            float elapsed = 0f;
            while (elapsed < popUpDuration)
            {
                elapsed += Time.deltaTime;
                hitboxInstance.transform.position = Vector3.Lerp(startPos, spawnPos, elapsed / popUpDuration);
                yield return null;
            }
            hitboxInstance.transform.position = spawnPos;

            float remaining = atk2HitboxDuration - popUpDuration;
            if (remaining > 0f)
            {
                yield return new WaitForSeconds(remaining);
            }

            atk2ActiveInstances.Remove(hitboxInstance);
            Destroy(hitboxInstance);
        }

        atk2AliveCount--;
    }

    // 지정된 이름의 애니메이터 상태로 진입할 때까지 대기한다.
    private IEnumerator WaitForStateEnter(string stateName)
    {
        AnimatorStateInfo stateInfo;
        do
        {
            yield return null;
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        }
        while (!stateInfo.IsName(stateName));
    }

    // 공격 대기시간(SpiritAtk1_1Anim) 동안 atk1 경고용 오브젝트를 원래(에디터에 배치된) 위치에 그대로 표시한다.
    // 바라보는 방향과 무관하게 항상 같은 위치에 소환된다. 충돌 판정이 있는 실제 히트박스는 아직 켜지 않는다.
    private void BeginAtk1Warning()
    {
        if (atk1WarningObject == null) return;

        atk1WarningObject.transform.localPosition = atk1WarningLocalPos;
        atk1WarningObject.SetActive(true);
    }

    // 공격 순간(SpiritAtk1_2Anim)에 atk1 경고 표시를 끄고, 원래(에디터에 배치된) 위치에 그대로
    // 실제 판정 히트박스를 잠시 활성화한다. 바라보는 방향과 무관하게 항상 같은 위치에 소환된다.
    private void ShowAtk1Hitbox()
    {
        if (atk1WarningObject != null) atk1WarningObject.SetActive(false);
        if (atk1HitboxObject == null) return;

        atk1HitboxObject.transform.localPosition = atk1HitboxLocalPos;
        if (atk1Hitbox != null) atk1Hitbox.ClearDetected();
        if (atk1HitboxAnim != null) atk1HitboxAnim.SetTrigger("atk");

        if (atk1HitboxRoutine != null) StopCoroutine(atk1HitboxRoutine);
        atk1HitboxRoutine = StartCoroutine(HitboxRoutine(atk1HitboxObject, atk1HitboxDuration, () => atk1HitboxRoutine = null));
    }

    // 공격 대기시간(SpiritAtk3_1Anim) 동안 atk3 경고용 오브젝트를 atk3를 시작한 시점의 player x좌표에
    // 맞춰 표시한다(y/z는 원래 값 유지). 충돌 판정이 있는 실제 히트박스는 아직 켜지 않는다.
    private void BeginAtk3Warning(float targetX)
    {
        if (atk3WarningObject == null) return;

        atk3WarningObject.transform.position = new Vector3(targetX, atk3WarningWorldY, atk3WarningWorldZ);
        atk3WarningObject.SetActive(true);
    }

    // 공격 순간(SpiritAtk3_2Anim)에 atk3 경고 표시를 끄고, atk3를 시작한 시점의 player x좌표(targetX)에
    // 맞춰 실제 판정 히트박스를 활성화한다(y/z는 원래 값 유지). 목표 위치에 바로 나타나지 않고, 그보다
    // 아래에서 시작해 짧은 시간 동안 빠르게 튀어오른다.
    private void ShowAtk3Hitbox(float targetX)
    {
        if (atk3WarningObject != null) atk3WarningObject.SetActive(false);
        if (atk3HitboxObject == null) return;

        Vector3 targetPos = new Vector3(targetX, atk3HitboxWorldY, atk3HitboxWorldZ);
        if (atk3Hitbox != null) atk3Hitbox.ClearDetected();

        if (atk3HitboxRoutine != null) StopCoroutine(atk3HitboxRoutine);
        atk3HitboxRoutine = StartCoroutine(Atk3HitboxPopUpRoutine(targetPos, () => atk3HitboxRoutine = null));
    }

    // atk3 히트박스를 목표 위치보다 atk3HitboxPopUpDistance만큼 아래에서 활성화시킨 뒤, atk3HitboxPopUpDuration
    // 동안 빠르게 목표 위치까지 이동시킨다. 이후 남은 시간(atk3HitboxDuration에서 튀어오른 시간을 뺀 만큼)
    // 동안 목표 위치에 유지되다가 비활성화된다.
    private IEnumerator Atk3HitboxPopUpRoutine(Vector3 targetPos, System.Action onComplete)
    {
        Vector3 startPos = targetPos + Vector3.down * atk3HitboxPopUpDistance;
        atk3HitboxObject.transform.position = startPos;
        atk3HitboxObject.SetActive(true);

        float popUpDuration = Mathf.Min(atk3HitboxPopUpDuration, atk3HitboxDuration);
        float elapsed = 0f;
        while (elapsed < popUpDuration)
        {
            elapsed += Time.deltaTime;
            atk3HitboxObject.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / popUpDuration);
            yield return null;
        }
        atk3HitboxObject.transform.position = targetPos;

        float remaining = atk3HitboxDuration - popUpDuration;
        if (remaining > 0f)
        {
            yield return new WaitForSeconds(remaining);
        }

        atk3HitboxObject.SetActive(false);
        onComplete?.Invoke();
    }

    // 판정 오브젝트를 켠 뒤, 지정된 시간이 지나면 다시 끄고 완료 콜백으로 코루틴 핸들을 정리한다.
    private IEnumerator HitboxRoutine(GameObject hitboxObject, float duration, System.Action onComplete)
    {
        hitboxObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        hitboxObject.SetActive(false);
        onComplete?.Invoke();
    }

    // isClear()가 true가 되면 Dead 파라미터를 활성화한다. (AcSpirit 컨트롤러의 Dead는 Bool 파라미터)
    private void Die()
    {
        isDead = true;
        isActing = false;
        anim.SetBool("Dead", true);
    }
}
