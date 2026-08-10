using UnityEngine;

// Attatch this Script to Enemy Objects
public class Mission2_KillAllEnemy : MonoBehaviour, MissionManagerInterface
{
    [SerializeField] private int maxHp = 3;
    [SerializeField] private Animator anim;
    [SerializeField] private EnemyHealthBar healthBar;
    public bool enable = true;

    [SerializeField] private int hp;
    private bool isDead = false;

    private void Awake()
    {
        healthBar.SetHealth(maxHp);
        anim.SetBool("Dead", false);
        hp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || !enable) return;
        anim.SetTrigger("Damaged");
        Debug.Log("asdf");
        hp -= damage;
        healthBar.TakeDamage(damage);
        if (hp <= 0)
        {
            anim.SetBool("Dead", true);
            isDead = true;
        }
    }

    public bool isClear()
    {
        return isDead;
    }
}
