using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 플레이어 공격 판정 오브젝트에 부착하는 컴포넌트.
/// 콜라이더(Is Trigger) 안에 들어온 오브젝트를 감지해서 목록으로 들고 있는다.
/// 감지된 오브젝트(또는 그 부모)에 Mission2_KillAllEnemy가 있으면 데미지를 적용한다.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AttackHitbox : MonoBehaviour
{
    /// <summary>새로운 오브젝트가 판정 콜라이더 안으로 들어왔을 때 발생.</summary>
    public event Action<Collider2D> ObjectDetected;

    private readonly HashSet<Collider2D> detected = new HashSet<Collider2D>();

    public IReadOnlyCollection<Collider2D> DetectedObjects => detected;

    /// <summary>공격 판정을 다시 켤 때 이전에 감지했던 목록을 비운다.</summary>
    public void ClearDetected()
    {
        detected.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!detected.Add(other)) return;

        ObjectDetected?.Invoke(other);

        Mission2_KillAllEnemy enemy = other.GetComponentInParent<Mission2_KillAllEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(StatManager.instance.playerAtk);
        }

        BreakableObject breakAble = other.GetComponentInParent<BreakableObject>();
        if(breakAble != null && !breakAble.isBroken)
        {
            breakAble.Break();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        detected.Remove(other);
    }
}
