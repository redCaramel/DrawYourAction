using UnityEditor.Timeline.Actions;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigid;
    private SpriteRenderer _sprite;
    private Collider2D _collider;
    private float playerSpeed;
    private float playerJumpPower;
    private int dir;
    private int jumpTime;
    private int jumpTimeMax;
    private bool isGrounded = true;

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
        ActionRecorder.instance.StartRecording(5);
    }
    private void SetDirZero()
    {
        dir = 0;
    }
    private MovementType ActionLeftNormal()
    {
         MovementType move;
        _sprite.flipX = true;
        dir = -1;
        move = MovementType.LeftNormal;
        return move;
    }
    private MovementType ActionRightNormal()
    {
        MovementType move;
        _sprite.flipX = false;
        dir = 1;
        move = MovementType.RightNormal;
        return move;
    }

    private JumpType ActionJumpNormal()
    {
        JumpType jump = JumpType.Idle;
        if (jumpTime > 0)
        {
            jumpTime--;
            _rigid.linearVelocity = new Vector2(_rigid.linearVelocity.x, 0f);
            _rigid.AddForce(Vector2.up * playerJumpPower, ForceMode2D.Impulse);
            jump = JumpType.JumpNormal;
        }

        return jump;
    }
    public void ExecuteAction(Action act)
    {
        if(act.move == MovementType.Idle) SetDirZero();
        else if(act.move == MovementType.LeftNormal) ActionLeftNormal();
        else if(act.move == MovementType.RightNormal) ActionRightNormal();
        
        if(act.jump == JumpType.JumpNormal) ActionJumpNormal();
    }
    private void FixedUpdate()
    {
        isGrounded = CheckGrounded();
        if(isGrounded) jumpTime = jumpTimeMax;
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
