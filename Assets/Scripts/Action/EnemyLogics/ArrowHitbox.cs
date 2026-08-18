using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Act2 화살(arrow) 프리팹에 부착하는 공격 판정 컴포넌트.
/// GoblinAtkHitbox/GolemAtkHitbox와 마찬가지로 Player의 Collider2D가 Trigger가 아니므로,
/// 트리거 이벤트 대신 충돌(Collision) 이벤트로 감지한다. 부딪힌 오브젝트가 Player 태그를 가지고 있으면
/// 미션을 실패 처리한다.
/// 다른 Hitbox류 클래스와 다른 점: Ground 레이어에 한 번이라도 닿으면 화살이 바닥에 박힌 것으로 간주해서,
/// 그 이후로는 Player와의 충돌 판정을 하지 않는다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ArrowHitbox : MonoBehaviour
{
    private readonly HashSet<Collider2D> detected = new HashSet<Collider2D>();
    [SerializeField] private Animator playerAnim; // 프리팹이라 Inspector에서 지정할 수 없으므로, 비워두면 PlayerController.instance에서 자동으로 가져온다.

    private Collider2D col;
    private bool hasHitGround = false; // Ground 레이어에 닿은 적이 있으면 true. 이후로는 Player 판정을 하지 않는다.

    /// <summary>공격 판정을 다시 켤 때 이전에 감지했던 목록을 비운다.</summary>
    public void ClearDetected()
    {
        detected.Clear();
    }

    void Awake()
    {
        col = GetComponent<Collider2D>();

        if (playerAnim == null && PlayerController.instance != null)
        {
            playerAnim = PlayerController.instance.GetComponent<Animator>();
        }

        if (playerAnim != null) playerAnim.SetBool("dead", false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        if (!detected.Add(other)) return;

        // Ground 레이어에 닿으면 화살이 바닥에 박힌 것으로 간주하고, 이후로는 Player와의 충돌 판정을 하지 않는다.
        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Ground")) != 0)
        {
            Debug.Log("asf");
            hasHitGround = true;
            if (col != null) col.enabled = false;
            return;
        }

        if (hasHitGround) return;

        if (other.CompareTag("Player"))
        {
            if (playerAnim != null) playerAnim.SetTrigger("death");
            if (ActionMissionResultManager.instance != null)
            {
                ActionMissionResultManager.instance.MissionFailure();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        detected.Remove(collision.collider);
    }
}
