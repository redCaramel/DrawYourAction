using System.Collections;
using UnityEngine;

// Attatch this Script to the Act1 Goblin enemy object, along with Mission2_KillAllEnemy.
[RequireComponent(typeof(Mission2_KillAllEnemy))]
public class Act1Goblin : MonoBehaviour
{
    [Header("타겟")]
    [SerializeField] private Transform player; // 비워두면 PlayerController.instance를 따라간다.

    [Header("이동")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.5f; // 이 범위 안에 들어오면 멈추고 공격을 시작한다.

    [Header("공격")]
    [SerializeField] private float attackDelay = 0.6f; // attack 트리거 후 atk 트리거까지 멈춰있는 시간
    [SerializeField] private string runStateName = "GoblinRunAnim"; // atk 애니메이션이 끝나고 복귀하는 이동 애니메이션 상태 이름
    [SerializeField] private GameObject atkHitboxObject; // 공격 판정 오브젝트 (Collider2D + GoblinAtkHitbox 필요)
    [SerializeField] private float atkHitboxDuration = 0.2f; // 판정 오브젝트가 켜져있는 시간

    [Header("사망 연출")]
    [SerializeField] private float deadDelay = 1f; // dead 트리거 후 사라지기 시작하기까지 대기 시간
    [SerializeField] private float fadeOutDuration = 1f; // 서서히 사라지는 데 걸리는 시간

    private Animator anim;
    private SpriteRenderer sprite;
    private Mission2_KillAllEnemy mission;
    private GoblinAtkHitbox atkHitbox;
    private Coroutine attackRoutineHandle;
    private Coroutine atkHitboxRoutine;
    private Vector3 atkHitboxLocalPos; // atkHitboxObject의 원래(오른쪽 기준) 로컬 위치

    private bool isActing = false; // StartAction() 이후 이동/전투 로직을 수행 중인지
    private bool isAttacking = false; // 멈춰서 attack~atk 모션을 진행 중인지 (이동/방향전환 정지)
    private bool isDead = false;
    private int facingDir = 1; // 마지막으로 바라본 방향 (-1: 왼쪽, 1: 오른쪽). 정지 중에도 유지된다.

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        mission = GetComponent<Mission2_KillAllEnemy>();

        if (player == null && PlayerController.instance != null)
        {
            player = PlayerController.instance.transform;
        }

        if (atkHitboxObject != null)
        {
            atkHitboxLocalPos = atkHitboxObject.transform.localPosition;
            atkHitbox = atkHitboxObject.GetComponent<GoblinAtkHitbox>();
            atkHitboxObject.SetActive(false);
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
            StartCoroutine(DieRoutine());
            return;
        }

        if (!isActing || isAttacking || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= attackRange)
        {
            attackRoutineHandle = StartCoroutine(AttackRoutine());
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
        anim.SetTrigger("start");
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

        if (atkHitboxRoutine != null)
        {
            StopCoroutine(atkHitboxRoutine);
            atkHitboxRoutine = null;
        }

        if (atkHitboxObject != null) atkHitboxObject.SetActive(false);
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

    // 제자리에 멈춰 attack -> (대기) -> atk 순서로 트리거를 발동시키고, atk2 애니메이션이 끝나면 이동을 재개한다.
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        anim.SetTrigger("attack");

        yield return new WaitForSeconds(attackDelay);

        anim.SetTrigger("atk");
        ShowAtkHitbox();
        yield return StartCoroutine(WaitForStateFinish(runStateName));

        isAttacking = false;
    }

    // 지정된 이름의 애니메이터 상태가 시작될 때까지 대기한다.
    private IEnumerator WaitForStateFinish(string stateName)
    {
        AnimatorStateInfo stateInfo;
        do
        {
            yield return null;
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        }
        while (!stateInfo.IsName(stateName));
    }

    // 바라보는 방향의 전방에 공격 판정 오브젝트를 잠시 활성화시킨다.
    private void ShowAtkHitbox()
    {
        if (atkHitboxObject == null) return;

        if (atkHitboxRoutine != null) StopCoroutine(atkHitboxRoutine);
        atkHitboxRoutine = StartCoroutine(AtkHitboxRoutine());
    }

    private IEnumerator AtkHitboxRoutine()
    {
        // 바라보는 방향(facingDir)에 맞춰 판정 오브젝트 위치를 좌우로 뒤집는다.
        atkHitboxObject.transform.localPosition = new Vector3(atkHitboxLocalPos.x * facingDir, atkHitboxLocalPos.y, atkHitboxLocalPos.z);

        if (atkHitbox != null) atkHitbox.ClearDetected();
        atkHitboxObject.SetActive(true);

        yield return new WaitForSeconds(atkHitboxDuration);

        atkHitboxObject.SetActive(false);
        atkHitboxRoutine = null;
    }

    // isClear()가 true가 되면 dead 트리거를 발동시키고, 일정 시간 후 서서히 사라지다 비활성화된다.
    private IEnumerator DieRoutine()
    {
        isDead = true;
        isActing = false;
        anim.SetTrigger("dead");

        yield return new WaitForSeconds(deadDelay);

        yield return StartCoroutine(FadeOutAndDisable());
    }

    private IEnumerator FadeOutAndDisable()
    {
        if (sprite == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        Color color = sprite.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            sprite.color = color;
            yield return null;
        }

        color.a = 0f;
        sprite.color = color;
        gameObject.SetActive(false);
    }
}
