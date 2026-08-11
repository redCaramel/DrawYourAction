using System.Collections;
using UnityEngine;

// Attatch this Script to the Act1 Golem enemy object, along with Mission2_KillAllEnemy.
[RequireComponent(typeof(Mission2_KillAllEnemy))]
public class Act1Golem : MonoBehaviour
{
    [Header("타겟")]
    [SerializeField] private Transform player; // 비워두면 PlayerController.instance를 따라간다.

    [Header("이동")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.5f; // 이 범위 안에 들어오면 멈추고 공격을 시작한다.

    [Header("공격")]
    [SerializeField, Range(0f, 1f)] private float atk2Chance = 0.3f; // atk1 대신 atk2를 사용할 확률
    [SerializeField] private float attackCooldown = 1.5f; // 공격이 끝난 후 다시 공격하기까지 제한되는 시간
    [SerializeField] private string idleStateName = "GolemIdleAnim";
    [SerializeField] private string atk1_2StateName = "GolemAtk1_2Anim";
    [SerializeField] private string atk2_2StateName = "GolemAtk2_2Anim";

    [Header("히트박스")]
    [SerializeField] private GameObject atk1HitboxObject; // atk1 실제 충돌 판정 오브젝트. 공격 순간(GolemAtk1_2Anim)에만 활성화된다. 바라보는 방향의 전방에 위치한다.
    [SerializeField] private GameObject atk1WarningObject; // atk1 경고 표시용 오브젝트. 공격 대기시간(GolemAtk1_1Anim) 동안에만 표시된다. 충돌 판정은 없다.
    [SerializeField] private float atk1HitboxDuration = 0.2f;
    [SerializeField] private GameObject atk2HitboxObject; // atk2 실제 충돌 판정 오브젝트(자신을 중심으로). 공격 순간(GolemAtk2_2Anim)에만 활성화된다.
    [SerializeField] private GameObject atk2WarningObject; // atk2 경고 표시용 오브젝트(자신을 중심으로). 공격 대기시간(GolemAtk2_1Anim) 동안에만 표시된다. 충돌 판정은 없다.
    [SerializeField] private float atk2HitboxDuration = 0.2f;
    [SerializeField] private GameObject atk2FrontHitboxObject; // atk2 실제 충돌 판정 오브젝트(전방 추가분). atk1처럼 바라보는 방향의 전방에 위치한다.
    [SerializeField] private GameObject atk2FrontWarningObject; // atk2FrontHitboxObject에 대응하는 경고 표시용 오브젝트. 충돌 판정은 없다.
    [SerializeField] private float atk2FrontHitboxDuration = 0.2f;

    private Animator anim;
    private SpriteRenderer sprite;
    private Mission2_KillAllEnemy mission;
    private Coroutine attackRoutineHandle;
    private Coroutine atk1HitboxRoutine;
    private Coroutine atk2HitboxRoutine;
    private Coroutine atk2FrontHitboxRoutine;
    private Vector3 atk1HitboxLocalPos; // atk1HitboxObject의 원래(오른쪽 기준) 로컬 위치
    private Vector3 atk1WarningLocalPos; // atk1WarningObject의 원래(오른쪽 기준) 로컬 위치
    private Vector3 atk2FrontHitboxLocalPos; // atk2FrontHitboxObject의 원래(오른쪽 기준) 로컬 위치
    private Vector3 atk2FrontWarningLocalPos; // atk2FrontWarningObject의 원래(오른쪽 기준) 로컬 위치
    private GolemAtkHitbox atk1Hitbox;
    private GolemAtkHitbox atk2Hitbox;
    private GolemAtkHitbox atk2FrontHitbox;

    private bool isActing = false; // StartAction() 이후 이동/전투 로직을 수행 중인지
    private bool isAttacking = false; // 멈춰서 atk1/atk2 모션을 진행 중인지 (이동/방향전환 정지)
    private bool isDead = false;
    private int facingDir = 1; // 마지막으로 바라본 방향 (-1: 왼쪽, 1: 오른쪽). 정지 중에도 유지된다.
    private float nextAttackAllowedTime = 0f; // 이 시간이 되기 전까지는 공격을 시작할 수 없다 (attackCooldown)

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        mission = GetComponent<Mission2_KillAllEnemy>();

        if (player == null && PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }

        if (atk1HitboxObject != null)
        {
            atk1HitboxLocalPos = atk1HitboxObject.transform.localPosition;
            atk1Hitbox = atk1HitboxObject.GetComponent<GolemAtkHitbox>();
            atk1HitboxObject.SetActive(false);
        }

        if (atk1WarningObject != null)
        {
            atk1WarningLocalPos = atk1WarningObject.transform.localPosition;
            atk1WarningObject.SetActive(false);
        }

        if (atk2HitboxObject != null)
        {
            atk2Hitbox = atk2HitboxObject.GetComponent<GolemAtkHitbox>();
            atk2HitboxObject.SetActive(false);
        }

        if (atk2WarningObject != null)
        {
            atk2WarningObject.SetActive(false);
        }

        if (atk2FrontHitboxObject != null)
        {
            atk2FrontHitboxLocalPos = atk2FrontHitboxObject.transform.localPosition;
            atk2FrontHitbox = atk2FrontHitboxObject.GetComponent<GolemAtkHitbox>();
            atk2FrontHitboxObject.SetActive(false);
        }

        if (atk2FrontWarningObject != null)
        {
            atk2FrontWarningLocalPos = atk2FrontWarningObject.transform.localPosition;
            atk2FrontWarningObject.SetActive(false);
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

        if (!isActing || isAttacking || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            // 범위 안에 들어오면 멈춰 선다. 쿨다운이 끝난 경우에만 공격을 시작한다.
            if (Time.time >= nextAttackAllowedTime)
            {
                bool useAtk2 = Random.value < atk2Chance;
                attackRoutineHandle = StartCoroutine(useAtk2 ? Atk2Routine() : Atk1Routine());
            }
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    // 골렘(적)을 활성화시켜 Player를 향해 이동/전투를 시작하게 한다.
    public void StartAction()
    {
        if (isDead) return;
        isActing = true;
    }

    // 이동/공격/히트박스 등 진행 중인 모든 행동을 즉시 멈춘다.
    public void StopAction()
    {
        isActing = false;
        isAttacking = false;

        if (attackRoutineHandle != null)
        {
            StopCoroutine(attackRoutineHandle);
            attackRoutineHandle = null;
        }

        if (atk1HitboxRoutine != null)
        {
            StopCoroutine(atk1HitboxRoutine);
            atk1HitboxRoutine = null;
        }

        if (atk2HitboxRoutine != null)
        {
            StopCoroutine(atk2HitboxRoutine);
            atk2HitboxRoutine = null;
        }

        if (atk2FrontHitboxRoutine != null)
        {
            StopCoroutine(atk2FrontHitboxRoutine);
            atk2FrontHitboxRoutine = null;
        }

        if (atk1HitboxObject != null) atk1HitboxObject.SetActive(false);
        if (atk1WarningObject != null) atk1WarningObject.SetActive(false);
        if (atk2HitboxObject != null) atk2HitboxObject.SetActive(false);
        if (atk2WarningObject != null) atk2WarningObject.SetActive(false);
        if (atk2FrontHitboxObject != null) atk2FrontHitboxObject.SetActive(false);
        if (atk2FrontWarningObject != null) atk2FrontWarningObject.SetActive(false);
    }

    private void MoveTowardsPlayer()
    {
        facingDir = player.position.x >= transform.position.x ? 1 : -1;
        UpdateFacing(facingDir);

        transform.position += Vector3.right * facingDir * moveSpeed * Time.deltaTime;
    }

    private void UpdateFacing(int dir)
    {
        if (sprite != null) sprite.flipX = dir < 0;
    }

    // 제자리에 멈춰 atk1 트리거를 발동시키고, GolemAtk1_1Anim -> GolemAtk1_2Anim이 모두 끝날 때까지 대기한 뒤 이동을 재개한다.
    private IEnumerator Atk1Routine()
    {
        isAttacking = true;
        int atkFacingDir = facingDir; // atk1을 시작하는 시점의 방향으로 히트박스 방향을 고정한다.
        anim.SetTrigger("atk1");

        // ===== 공격 대기시간(GolemAtk1_1Anim) 구간: 경고용 오브젝트만 표시하고, 실제 판정은 없다 =====
        BeginAtk1Hitbox(atkFacingDir);
        // ============================================================================

        // GolemAtk1_1Anim이 끝나고 GolemAtk1_2Anim으로 넘어가는 시점까지 대기 (= 공격 대기시간 종료, 공격 순간 시작)
        yield return StartCoroutine(WaitForStateEnter(atk1_2StateName));

        // ===== GolemAtk1_2Anim 재생 구간(공격 순간): 경고 표시를 끄고 실제 판정 히트박스를 활성화한다 =====
        ShowAtk1Hitbox(atkFacingDir);
        // ============================================================================

        // GolemAtk1_2Anim이 끝나고 Idle로 돌아오는 시점까지 대기 (= GolemAtk1_2Anim 종료 시점)
        yield return StartCoroutine(WaitForStateEnter(idleStateName));

        isAttacking = false;
        nextAttackAllowedTime = Time.time + attackCooldown;
    }

    // 제자리에 멈춰 atk2 트리거를 발동시키고, GolemAtk2_1Anim -> GolemAtk2_2Anim이 모두 끝날 때까지 대기한 뒤 이동을 재개한다.
    private IEnumerator Atk2Routine()
    {
        isAttacking = true;
        int atkFacingDir = facingDir; // atk2를 시작하는 시점의 방향으로 전방 히트박스(atk2FrontHitboxObject) 방향을 고정한다.
        anim.SetTrigger("atk2");

        // ===== 공격 대기시간(GolemAtk2_1Anim) 구간: 경고용 오브젝트만 표시하고, 실제 판정은 없다 =====
        BeginAtk2Hitbox(atkFacingDir);
        // ============================================================================

        // GolemAtk2_1Anim이 끝나고 GolemAtk2_2Anim으로 넘어가는 시점까지 대기 (= 공격 대기시간 종료, 공격 순간 시작)
        yield return StartCoroutine(WaitForStateEnter(atk2_2StateName));

        // ===== GolemAtk2_2Anim 재생 구간(공격 순간): 경고 표시를 끄고 실제 판정 히트박스를 활성화한다 =====
        ShowAtk2Hitbox(atkFacingDir);
        // ============================================================================

        // GolemAtk2_2Anim이 끝나고 Idle로 돌아오는 시점까지 대기 (= GolemAtk2_2Anim 종료 시점)
        yield return StartCoroutine(WaitForStateEnter(idleStateName));

        isAttacking = false;
        nextAttackAllowedTime = Time.time + attackCooldown;
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

    // 공격 대기시간(GolemAtk1_1Anim) 동안 atk1 경고용 오브젝트를 지정된 방향(atk1 시작 시점의 방향)의
    // 전방에 맞춰 표시한다. 충돌 판정이 있는 실제 히트박스는 아직 켜지 않는다.
    private void BeginAtk1Hitbox(int dir)
    {
        if (atk1WarningObject == null) return;

        // atk1을 시작한 시점의 방향(dir)에 맞춰 경고 오브젝트를 전방(오른쪽: +x, 왼쪽: -x)에 위치시킨다.
        // 원래 로컬 위치의 부호가 에디터에서 어느 쪽으로 배치됐는지와 무관하게 크기(절댓값)만 사용해 방향을 강제한다.
        atk1WarningObject.transform.localPosition = new Vector3(Mathf.Abs(atk1WarningLocalPos.x) * dir, atk1WarningLocalPos.y, atk1WarningLocalPos.z);
        atk1WarningObject.SetActive(true);
    }

    // 공격 대기시간(GolemAtk2_1Anim) 동안 atk2 경고용 오브젝트 두 개(자신 중심 + 전방)를 모두 표시한다.
    // 전방 경고 오브젝트는 atk1처럼 atk2를 시작한 시점의 방향(dir)에 맞춰 위치시킨다. 충돌 판정이 있는
    // 실제 히트박스는 아직 켜지 않는다.
    private void BeginAtk2Hitbox(int dir)
    {
        if (atk2WarningObject != null) atk2WarningObject.SetActive(true);

        if (atk2FrontWarningObject == null) return;

        // atk2를 시작한 시점의 방향(dir)에 맞춰 전방 경고 오브젝트를 전방(오른쪽: +x, 왼쪽: -x)에 위치시킨다.
        atk2FrontWarningObject.transform.localPosition = new Vector3(Mathf.Abs(atk2FrontWarningLocalPos.x) * dir, atk2FrontWarningLocalPos.y, atk2FrontWarningLocalPos.z);
        atk2FrontWarningObject.SetActive(true);
    }

    // 공격 순간(GolemAtk1_2Anim)에 atk1 경고 표시를 끄고, atk1을 시작한 시점의 방향(dir)에 맞춰
    // 실제 판정 히트박스를 잠시 활성화한다.
    private void ShowAtk1Hitbox(int dir)
    {
        if (atk1WarningObject != null) atk1WarningObject.SetActive(false);
        if (atk1HitboxObject == null) return;

        atk1HitboxObject.transform.localPosition = new Vector3(Mathf.Abs(atk1HitboxLocalPos.x) * dir, atk1HitboxLocalPos.y, atk1HitboxLocalPos.z);
        if (atk1Hitbox != null) atk1Hitbox.ClearDetected();

        if (atk1HitboxRoutine != null) StopCoroutine(atk1HitboxRoutine);
        atk1HitboxRoutine = StartCoroutine(HitboxRoutine(atk1HitboxObject, atk1HitboxDuration, () => atk1HitboxRoutine = null));
    }

    // 공격 순간(GolemAtk2_2Anim)에 atk2 경고 표시(자신 중심 + 전방)를 모두 끄고, 실제 판정 히트박스
    // 두 개(자신 중심 + atk1처럼 방향(dir)에 맞춘 전방)를 잠시 함께 활성화한다.
    private void ShowAtk2Hitbox(int dir)
    {
        if (atk2WarningObject != null) atk2WarningObject.SetActive(false);
        if (atk2FrontWarningObject != null) atk2FrontWarningObject.SetActive(false);

        if (atk2HitboxObject != null)
        {
            if (atk2Hitbox != null) atk2Hitbox.ClearDetected();

            if (atk2HitboxRoutine != null) StopCoroutine(atk2HitboxRoutine);
            atk2HitboxRoutine = StartCoroutine(HitboxRoutine(atk2HitboxObject, atk2HitboxDuration, () => atk2HitboxRoutine = null));
        }

        if (atk2FrontHitboxObject != null)
        {
            atk2FrontHitboxObject.transform.localPosition = new Vector3(Mathf.Abs(atk2FrontHitboxLocalPos.x) * dir, atk2FrontHitboxLocalPos.y, atk2FrontHitboxLocalPos.z);
            if (atk2FrontHitbox != null) atk2FrontHitbox.ClearDetected();

            if (atk2FrontHitboxRoutine != null) StopCoroutine(atk2FrontHitboxRoutine);
            atk2FrontHitboxRoutine = StartCoroutine(HitboxRoutine(atk2FrontHitboxObject, atk2FrontHitboxDuration, () => atk2FrontHitboxRoutine = null));
        }
    }

    // 판정 오브젝트를 켠 뒤, 지정된 시간이 지나면 다시 끄고 완료 콜백으로 코루틴 핸들을 정리한다.
    private IEnumerator HitboxRoutine(GameObject hitboxObject, float duration, System.Action onComplete)
    {
        hitboxObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        hitboxObject.SetActive(false);
        onComplete?.Invoke();
    }

    // isClear()가 true가 되면 Dead 트리거를 발동시킨다.
    private void Die()
    {
        isDead = true;
        isActing = false;
        anim.SetTrigger("Dead");
    }
}
