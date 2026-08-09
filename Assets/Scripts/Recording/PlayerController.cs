using System.Collections;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private GameObject atkRangeObject; // 공격 판정 오브젝트 (Collider2D + AttackHitbox 필요)
    [SerializeField] private float atkRangeDuration = 0.2f; // 판정 오브젝트가 켜져있는 시간

    private Rigidbody2D _rigid;
    private SpriteRenderer _sprite;
    private Collider2D _collider;
    private Animator _anim;
    private AttackHitbox _atkHitbox;
    private Coroutine _atkRangeRoutine;
    private Vector3 _atkRangeLocalPos; // atkRangeObject의 원래(오른쪽 기준) 로컬 위치
    private float playerSpeed;
    private float playerJumpPower;
    private int dir;
    private int facingDir = 1; // 플레이어가 바라보는 방향 (-1: 왼쪽, 1: 오른쪽). dir과 달리 Idle에서도 유지된다.
    private int jumpTime;
    private int jumpTimeMax;
    private bool isGrounded = true;
    private bool jumpRequested;
    private bool isControlLocked = false;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static PlayerController instance {get; private set;}

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
            _rigid = gameObject.GetComponent<Rigidbody2D>();
            _sprite = gameObject.GetComponent<SpriteRenderer>();
            _collider = gameObject.GetComponent<Collider2D>();
            _anim = gameObject.GetComponent<Animator>();

            if (atkRangeObject != null)
            {
                _atkRangeLocalPos = atkRangeObject.transform.localPosition;
                _atkHitbox = atkRangeObject.GetComponent<AttackHitbox>();
                atkRangeObject.SetActive(false);
            }
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
    void ResetStats()
    {
        playerSpeed = StatManager.instance.playerSpeed;
        playerJumpPower = StatManager.instance.playerJumpPower;
        jumpTimeMax = StatManager.instance.playerJumpTime;
        _rigid.gravityScale = StatManager.instance.playerGravity;
    }
    void Start()
    {
        ResetStats();
    }
    private void SetDirZero()
    {
        _anim.SetFloat("Speed", 0);
        dir = 0;
    }
    private MovementType ActionLeftNormal()
    {
         MovementType move;
        _sprite.flipX = true;
        dir = -1;
        facingDir = -1;
        move = MovementType.LeftNormal;
        _anim.SetFloat("Speed", 1);
        return move;
    }
    private MovementType ActionRightNormal()
    {
        MovementType move;
        _sprite.flipX = false;
        dir = 1;
        facingDir = 1;
        move = MovementType.RightNormal;
        _anim.SetFloat("Speed", 1);
        return move;
    }

    private JumpType ActionJumpNormal()
    {
        jumpRequested = true;
        return jumpTime > 0 ? JumpType.JumpNormal : JumpType.Idle;
    }
    private AttackType ActionAtkNormal()
    {
        AnimatorStateInfo stateInfo = _anim.GetCurrentAnimatorStateInfo(0);

        // 이미 A1, A2, A3 중 하나가 재생 중이라면 실행하지 않음 (Tag 활용)
        if (stateInfo.IsTag("atk")) return AttackType.Idle;
        AttackType atk;
        atk = AttackType.AttackNormal;
        _anim.SetTrigger("Attack");
        ShowAttackRange();
        return atk;
    }
    private void ShowAttackRange()
    {
        if (atkRangeObject == null) return;

        if (_atkRangeRoutine != null) StopCoroutine(_atkRangeRoutine);
        _atkRangeRoutine = StartCoroutine(AttackRangeRoutine());
    }
    private IEnumerator AttackRangeRoutine()
    {
        // 바라보는 방향(facingDir)에 맞춰 판정 오브젝트 위치를 좌우로 뒤집는다.
        atkRangeObject.transform.localPosition = new Vector3(_atkRangeLocalPos.x * facingDir, _atkRangeLocalPos.y, _atkRangeLocalPos.z);

        _atkHitbox?.ClearDetected();
        atkRangeObject.SetActive(true);

        yield return new WaitForSeconds(atkRangeDuration);

        atkRangeObject.SetActive(false);
        _atkRangeRoutine = null;
    }
    public void StopMovement()
    {
        SetDirZero();
    }
    /// <summary>
    /// 플레이어의 조작(입력 반영)을 잠그거나 푼다.
    /// 잠글 때는 즉시 그 자리에서 정지시킨다. (미션 성공/실패 시 등)
    /// </summary>
    public void SetControlLocked(bool locked)
    {
        isControlLocked = locked;
        if (isControlLocked)
        {
            jumpRequested = false;
            SetDirZero();
            if (_rigid != null) _rigid.linearVelocity = Vector2.zero;
        }
    }
    public void ExecuteAction(Action act)
    {
        if (isControlLocked) return;

        if(act.move == MovementType.Idle) SetDirZero();
        else if(act.move == MovementType.LeftNormal) ActionLeftNormal();
        else if(act.move == MovementType.RightNormal) ActionRightNormal();

        if(act.jump == JumpType.JumpNormal) ActionJumpNormal();

        if(act.atk == AttackType.AttackNormal) ActionAtkNormal();
    }
    private void FixedUpdate()
    {
        if (isControlLocked)
        {
            _rigid.linearVelocity = Vector2.zero;
            return;
        }

        isGrounded = CheckGrounded();
        if(isGrounded) jumpTime = jumpTimeMax;

        if (jumpRequested)
        {
            jumpRequested = false;
            if (jumpTime > 0)
            {
                jumpTime--;
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocity.x, 0f);
                _rigid.AddForce(Vector2.up * playerJumpPower, ForceMode2D.Impulse);
                isGrounded = false;
                _anim.SetTrigger("Jump");
            }
        }

        _anim.SetBool("Ground", isGrounded);
        _rigid.linearVelocity = new Vector2(playerSpeed * dir, _rigid.linearVelocityY);
    }
    private bool CheckGrounded()
    {
        if (_rigid.linearVelocity.y > 0.1f) 
            return false;

        Vector2 startPosition = new Vector2(_collider.bounds.center.x, _collider.bounds.min.y);

        RaycastHit2D rayHit = Physics2D.BoxCast(startPosition, new Vector2(0.6f, 0.1f), 0f, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));

        return rayHit.collider != null;
    }
}
