using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 골렘(적) 공격 판정 오브젝트에 부착하는 컴포넌트.
/// Player의 Collider2D가 Trigger가 아니므로, 트리거 이벤트 대신 충돌(Collision) 이벤트로 감지한다.
/// 부딪힌 오브젝트(또는 그 부모)가 Player 태그를 가지고 있으면 미션을 실패 처리한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GoblinAtkHitbox : MonoBehaviour
{
    [SerializeField] private Act1Goblin goblin;
    private readonly HashSet<Collider2D> detected = new HashSet<Collider2D>();
    [SerializeField] private Animator playerAnim;
    /// <summary>공격 판정을 다시 켤 때 이전에 감지했던 목록을 비운다.</summary>
    public void ClearDetected()
    {
        detected.Clear();
    }
    void Awake()
    {
        playerAnim.SetBool("dead", false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D other = collision.collider;
        if (!detected.Add(other)) return;

        if (other.CompareTag("Player"))
        {
            goblin.StopAction();
            playerAnim.SetTrigger("death");
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
