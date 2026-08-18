using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// enemies에 등록된 모든 오브젝트는 Mission2_KillAllEnemy 컴포넌트를 가지고 있어야 한다.
public class Act1Sandback : MonoBehaviour
{
    [Header("적 리스트")]
    [SerializeField] private List<GameObject> enemies;

    [Header("등장 연출")]
    [SerializeField] private float popUpDistance = 1.5f; // 아래에서 튀어나오는 거리
    [SerializeField] private float popUpDuration = 0.4f;  // 튀어나오는 데 걸리는 시간

    [Header("퇴장 연출")]
    [SerializeField] private float fadeOutDuration = 0.3f; // 이전 무리가 서서히 사라지는 시간

    // 무리별 등장 수를 4 -> 3 -> 4 -> 3 ... 순서로 고정 반복한다.
    private static readonly int[] batchSizes = { 4, 3 };

    private int nextIndex = 0;
    private int batchCount = 0;
    private readonly List<Mission2_KillAllEnemy> currentBatch = new();
    private readonly Dictionary<GameObject, Vector3> originalPositions = new();

    private void Awake()
    {
        // 등장 전에는 모두 비활성화 상태로 대기시키고, 원래 위치를 미리 기억해둔다.
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            originalPositions[enemy] = enemy.transform.position;
            enemy.SetActive(false);
        }
    }

    private void Start()
    {
        ShowEnemy();
    }

    private void Update()
    {
        if (currentBatch.Count == 0) return;

        foreach (var mission in currentBatch)
        {
            if (mission != null && !mission.isClear()) return;
        }

        // 현재 등장한 적이 모두 클리어되면 다음 인덱스부터 다음 무리를 등장시킨다.
        nextIndex += batchSizes[batchCount % batchSizes.Length];
        batchCount++;
        ShowEnemy();
    }

    // List의 nextIndex부터 batchSizes 순서(4 -> 3 -> 4 -> 3 ...)에 맞는 개수만큼 Enable시키며 등장시킨다.
    public void ShowEnemy()
    {
        // 이전 무리는 서서히 사라지게 하며 정리한다.
        foreach (var mission in currentBatch)
        {
            if (mission != null)
            {
                StartCoroutine(FadeOutAndDisable(mission.gameObject));
            }
        }
        currentBatch.Clear();

        int size = batchSizes[batchCount % batchSizes.Length];
        int count = 0;
        for (int i = nextIndex; i < enemies.Count && count < size; i++, count++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null) continue;

            if (!enemy.TryGetComponent(out Mission2_KillAllEnemy mission))
            {
                Debug.LogWarning($"{enemy.name}에 Mission2_KillAllEnemy 컴포넌트가 없습니다.");
                continue;
            }

            currentBatch.Add(mission);
            StartCoroutine(PopUp(enemy));
        }
    }

    // 오브젝트를 아래쪽 위치에서 Enable시킨 뒤, 원래 위치까지 튀어오르게 한다.
    private IEnumerator PopUp(GameObject enemy)
    {
        Vector3 targetPos = originalPositions[enemy];
        Vector3 startPos = targetPos + Vector3.down * popUpDistance;

        enemy.transform.position = startPos;
        enemy.SetActive(true);

        float elapsed = 0f;
        while (elapsed < popUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popUpDuration);
            enemy.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        enemy.transform.position = targetPos;
    }

    // 오브젝트를 서서히 투명하게 만든 뒤 비활성화한다.
    private IEnumerator FadeOutAndDisable(GameObject enemy)
    {
        SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>();

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            SetRenderersAlpha(renderers, alpha);
            yield return null;
        }

        SetRenderersAlpha(renderers, 0f);
        enemy.SetActive(false);

        // 재사용될 경우를 대비해 알파를 원래대로 복원해둔다.
        SetRenderersAlpha(renderers, 1f);
    }

    private static void SetRenderersAlpha(SpriteRenderer[] renderers, float alpha)
    {
        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }
}
