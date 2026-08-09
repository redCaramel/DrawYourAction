using UnityEngine;

// Attatch this Script to Enemy Objects
public class Mission2_KillAllEnemy : MonoBehaviour, MissionManagerInterface
{
    [SerializeField] private int maxHp = 3;
    [SerializeField] private Animator anim;
    [SerializeField] private EnemyHealthBar healthBar;

    private int hp;
    private bool isDead = false;

    private void Awake()
    {
        healthBar.SetHealth(maxHp);
        anim.SetBool("Dead", false);
        hp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        anim.SetTrigger("Damaged");
        hp -= damage;
        healthBar.TakeDamage(damage);
        if (hp <= 0)
        {
            anim.SetBool("Dead", true);
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        // TODO: 사망 애니메이션/이펙트 등 필요 시 여기에 추가
        gameObject.SetActive(false);
    }

    public bool isClear()
    {
        return isDead;
    }
}
