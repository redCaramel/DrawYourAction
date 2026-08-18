using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Act2 Spirit(적) 공격 판정 오브젝트에 부착하는 컴포넌트.
/// GolemAtkHitbox와 마찬가지로 Player의 Collider2D가 Trigger가 아니므로, 트리거 이벤트 대신
/// 충돌(Collision) 이벤트로 감지한다. 이 오브젝트는 공격 순간에만 활성화되므로(경고 표시는 별도의
/// 경고용 오브젝트가 담당), 활성화되는 즉시 판정이 이루어진다.
/// 부딪힌 오브젝트가 Player 태그를 가지고 있으면 Spirit의 행동을 멈추고 미션을 실패 처리한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SpiritAtkHitbox : MonoBehaviour
{
    [SerializeField] private Act2Spirit spirit;
    [SerializeField] private Animator playerAnim; // 비워두면 PlayerController.instance에서 자동으로 가져온다.
    private readonly HashSet<Collider2D> detected = new HashSet<Collider2D>();

    /// <summary>공격 판정을 다시 켤 때 이전에 감지했던 목록을 비운다.</summary>
    public void ClearDetected()
    {
        detected.Clear();
    }

    /// <summary>atk2처럼 프리팹으로 런타임에 소환되어 Inspector에서 미리 연결할 수 없는 경우, 소환 직후 owner를 지정한다.</summary>
    public void SetSpirit(Act2Spirit owner)
    {
        spirit = owner;
    }

    private void Awake()
    {
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

        if (other.CompareTag("Player"))
        {
            if (spirit != null) spirit.StopAction();
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
