using UnityEditor.Timeline.Actions;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigid;
    private SpriteRenderer _sprite;
    private Collider2D _collider;
    private Animator _anim;
    private float playerSpeed;
    private float playerJumpPower;
    private int dir;
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
        move = MovementType.LeftNormal;
        _anim.SetFloat("Speed", 1);
        return move;
    }
    private MovementType ActionRightNormal()
    {
        MovementType move;
        _sprite.flipX = false;
        dir = 1;
        move = MovementType.RightNormal;
        _anim.SetFloat("Speed", 1);
        return move;
    }

    private JumpType ActionJumpNormal()
    {
        jumpRequested = true;
        return jumpTime > 0 ? JumpType.JumpNormal : JumpType.Idle;
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
