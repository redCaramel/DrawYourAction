using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rigid;
    private SpriteRenderer renderer;
    private float playerSpeed;
    private float playerJumpPower;
    void Awake()
    {
        rigid = gameObject.GetComponent<Rigidbody2D>();
        renderer = gameObject.GetComponent<SpriteRenderer>();
    }
    void ResetStats()
    {
        playerSpeed = StatManager.instance.playerSpeed;
        playerJumpPower = StatManager.instance.playerJumpPower;
        Debug.Log(playerSpeed);
        Debug.Log(playerJumpPower);
    }
    void Start()
    {
        ResetStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
