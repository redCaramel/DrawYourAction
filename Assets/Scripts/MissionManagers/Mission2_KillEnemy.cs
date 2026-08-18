using System.Collections;
using UnityEngine;

// Attatch this Script to Enemy Objects
public class Mission2_KillAllEnemy : MonoBehaviour, MissionManagerInterface
{
    [SerializeField] private int maxHp = 3;
    [SerializeField] private Animator anim;
    [SerializeField] private EnemyHealthBar healthBar;
    [SerializeField] private bool damageAnim;
    public bool enable = true;

    [Header("피격 시 스프라이트 반짝임 (damageAnim이 true일 때만 동작)")]
    [SerializeField] private SpriteRenderer sprite; // 비워두면 GetComponent<SpriteRenderer>()로 자동 할당
    [SerializeField] private Color damageFlashColor = Color.white; // 피격 순간 밝게 빛날 색
    [SerializeField] private float damageFlashDuration = 0.15f; // 밝아진 색이 원래 색으로 돌아오는 데 걸리는 시간

    [SerializeField] private int hp;
    private bool isDead = false;
    private Color originalSpriteColor;
    private Coroutine damageFlashRoutine;

    private void Awake()
    {
        healthBar.SetHealth(maxHp);
        anim.SetBool("Dead", false);
        hp = maxHp;

        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) originalSpriteColor = sprite.color;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || !enable) return;
        anim.SetTrigger("Damaged");
        Debug.Log("asdf");
        hp -= damage;
        healthBar.TakeDamage(damage);

        if (damageAnim && sprite != null)
        {
            if (damageFlashRoutine != null) StopCoroutine(damageFlashRoutine);
            damageFlashRoutine = StartCoroutine(DamageFlashRoutine());
        }

        if (hp <= 0)
        {
            anim.SetBool("Dead", true);
            isDead = true;
        }
    }

    // 스프라이트를 즉시 밝은 색으로 바꿨다가, 짧은 시간 동안 원래 색으로 되돌린다.
    private IEnumerator DamageFlashRoutine()
    {
        sprite.color = damageFlashColor;

        float elapsed = 0f;
        while (elapsed < damageFlashDuration)
        {
            elapsed += Time.deltaTime;
            sprite.color = Color.Lerp(damageFlashColor, originalSpriteColor, elapsed / damageFlashDuration);
            yield return null;
        }

        sprite.color = originalSpriteColor;
        damageFlashRoutine = null;
    }

    public bool isClear()
    {
        return isDead;
    }

    // maxHp에 대한 현재 hp의 비율 (0~1). Act2Spirit의 hp 구간별(75%, 50%, 25%) atk2 발동 판정에 사용된다.
    public float HpRatio => maxHp > 0 ? (float)hp / maxHp : 0f;
}
