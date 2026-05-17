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
    void Awake()
    {
        _rigid = gameObject.GetComponent<Rigidbody2D>();
        _sprite = gameObject.GetComponent<SpriteRenderer>();
        _collider = gameObject.GetComponent<Collider2D>();
    }
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

    void Update()
    {
        MovementType move = MovementType.Idle;
        JumpType jump = JumpType.Idle;
        AttackType atk = AttackType.Idle;
        if(!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) dir = 0;
        if(Input.GetKey(KeyCode.A)) {
            _sprite.flipX = true;
            dir = -1;
            move = MovementType.LeftNormal;
        }
        else if(Input.GetKey(KeyCode.D)) {
            _sprite.flipX = false;
            dir = 1;
            move = MovementType.RightNormal;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpTime > 0)
            {
                jumpTime--;
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocity.x, 0f);
                _rigid.AddForce(Vector2.up*playerJumpPower, ForceMode2D.Impulse);
                jump = JumpType.JumpNormal;
            }
        }
        if(ActionRecorder.instance.isRecording())
        {
            ActionRecorder.instance.ApplyAction(move);
            ActionRecorder.instance.ApplyAction(jump);
            ActionRecorder.instance.ApplyAction(atk);
        }
        
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
