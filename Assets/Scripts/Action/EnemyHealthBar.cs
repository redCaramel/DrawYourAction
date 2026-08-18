using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image healthBarImage; // 원형 체력바 UI Image

    [Header("체력 설정")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("연출 설정")]
    [SerializeField] private float smoothSpeed = 5f; // 체력바 감소 애니메이션 속도
    private float targetFillAmount = 1f;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI(true); // 시작할 때는 즉시 100% 반영
    }

    void Update()
    {
        // 체력바가 부드럽게 감소하는 연출
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = Mathf.Lerp(
                healthBarImage.fillAmount, 
                targetFillAmount, 
                Time.deltaTime * smoothSpeed
            );
        }
        if(currentHealth==0) gameObject.SetActive(false);

    }
    public void SetHealth(int health)
    {
        maxHealth = health;
        currentHealth = health;
        UpdateHealthUI(true);
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth); // 0 ~ maxHealth 범위 제한

        UpdateHealthUI();
    }

    private void UpdateHealthUI(bool immediate = false)
    {
        targetFillAmount = currentHealth / maxHealth;

        if (immediate && healthBarImage != null)
        {
            healthBarImage.fillAmount = targetFillAmount;
        }
    }
}