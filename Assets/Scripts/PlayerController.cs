using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rigid;
    private SpriteRenderer _sprite;
    private Collider2D _collider;
    private float playerSpeed;
    private float playerJumpPower;
    private int dir;
    [SerializeField] private bool isGrounded = true;
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
        _rigid.gravityScale = StatManager.instance.playerGravity;
    }
    void Start()
    {
        ResetStats();
    }

    void Update()
    {
        if(!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) dir = 0;
        if(Input.GetKey(KeyCode.A)) {
            _sprite.flipX = true;
            dir = -1;
        }
        else if(Input.GetKey(KeyCode.D)) {
            _sprite.flipX = false;
            dir = 1;
        }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                _rigid.linearVelocity = new Vector2(_rigid.linearVelocity.x, 0f);
                _rigid.AddForce(Vector2.up*playerJumpPower, ForceMode2D.Impulse);
            }
            
        }
    }
     private void FixedUpdate()
    {
        isGrounded = CheckGrounded();
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
