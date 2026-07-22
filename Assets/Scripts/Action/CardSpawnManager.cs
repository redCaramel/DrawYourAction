using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSpawnManager : MonoBehaviour
{
    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static CardSpawnManager instance {get; private set;}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (InitialCutSceneManager.instance != null)
        {
            InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        }
    }

    // ----------------------------------------------------

    [Header("[프리팹 및 트랜스폼 설정]")]
    [SerializeField] private GameObject cardPrefab;       // 생성할 CardUI 프리팹
    [SerializeField] private Transform handParent;        // 카드가 들어갈 UI 부모 (Canvas 내부의 Panel/RectTransform)

    [Header("[카드 생성 및 배치 설정]")]
    private int initialHandSize;     // 초기에 들고 시작할 카드 수
    [SerializeField] private float cardSpacing = 200f;    // 카드 간의 가로 간격
    [SerializeField] private float handCenterY = -350f;   // 손패의 Y축 기본 위치
    [SerializeField] private float spawnInterval = 0.15f; // 초기 손패를 순차적으로 채울 때의 시간 간격

    [Header("[다음 카드 미리보기 설정]")]
    [SerializeField] private Vector2 previewPosition = new Vector2(-750f, 400f); // 좌측 상단 대기 위치
    [SerializeField] private float previewAlpha = 0.4f;   // 대기 중인 카드의 흐림 정도

    // 실시간 관리용 데이터 및 생성된 CardUI 리스트
    private List<ScriptData> drawPile = new List<ScriptData>();
    private List<ScriptDragManager> spawnedCards = new List<ScriptDragManager>();
    // 슬롯(인덱스)별 고정 좌표. 카드가 교체되어도 다른 슬롯의 카드는 움직이지 않음
    private List<Vector2> slotPositions = new List<Vector2>();

    // 좌측 상단에 희미하게 대기 중인, 다음에 뽑힐 카드
    private ScriptDragManager previewCard;

    private void Start()
    {
        var cutSceneManager = InitialCutSceneManager.instance;
        if (cutSceneManager != null && cutSceneManager.isCutSceneShowing)
        {
            // 컷씬 재생 중에는 카드를 띄우지 않고, 컷씬이 끝나는 시점에 맞춰 등장시킴
            cutSceneManager.OnCutSceneEnded += HandleCutSceneEnded;
        }
        else
        {
            InitializeAndSpawnHand();
        }
    }

    private void HandleCutSceneEnded()
    {
        InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        InitializeAndSpawnHand();
    }

    /// <summary>
    /// 덱 데이터를 초기화하고 초기 손패 카드를 Instantiate합니다.
    /// </summary>
    public void InitializeAndSpawnHand()
    {
        initialHandSize = CreateRecordInstance.handCount;

        // 1. 기존 데이터 및 남아있는 카드 UI 제거
        drawPile.Clear();
        foreach (var cardUI in spawnedCards)
        {
            if (cardUI != null) Destroy(cardUI.gameObject);
        }
        spawnedCards.Clear();

        if (previewCard != null)
        {
            Destroy(previewCard.gameObject);
            previewCard = null;
        }

        // 2. ScriptImporter가 불러온 스크립트 데이터를 덱으로 복사 (순서 유지)
        drawPile.AddRange(ScriptImporter.scripts ?? new List<ScriptData>());

        // 3. 손패 슬롯들의 고정 좌표를 미리 계산 (이후 카드가 교체되어도 이 좌표는 변하지 않음)
        int countToSpawn = Mathf.Min(initialHandSize, drawPile.Count);
        CalculateSlotPositions(countToSpawn);

        // 4. 첫 미리보기 카드를 좌측 상단에 준비
        PrepareNextPreview();

        // 5. countToSpawn만큼 코루틴을 통해 순차적으로 슬롯을 채움
        StartCoroutine(Co_SpawnInitialHand(countToSpawn));
    }

    /// <summary>
    /// 손패 슬롯 각각의 고정 좌표를 중앙 정렬 기준으로 계산합니다.
    /// </summary>
    private void CalculateSlotPositions(int slotCount)
    {
        slotPositions.Clear();
        if (slotCount == 0) return;

        // 중앙 기준 좌우 배치 공식
        // 예: 3장일 때 -> -cardSpacing, 0, +cardSpacing
        float startX = -((slotCount - 1) * cardSpacing) / 2f;

        for (int i = 0; i < slotCount; i++)
        {
            slotPositions.Add(new Vector2(startX + (i * cardSpacing), handCenterY));
        }
    }

    /// <summary>
    /// 초기 손패 슬롯을 순차적으로 채우는 코루틴. 각 슬롯은 대기 중이던 미리보기 카드를 끌어와 채워짐
    /// </summary>
    private IEnumerator Co_SpawnInitialHand(int countToSpawn)
    {
        for (int i = 0; i < countToSpawn; i++)
        {
            spawnedCards.Add(null); // 슬롯 자리 확보
            DrawCardIntoSlot(i);

            // 약간의 시차를 두어 카드가 하나씩 채워지도록 함
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 좌측 상단에 다음으로 뽑힐 카드를 희미하게 미리 배치합니다. (아직 덱에서 제거하지 않음)
    /// </summary>
    private void PrepareNextPreview()
    {
        if (drawPile.Count == 0)
        {
            previewCard = null;
            return;
        }

        ScriptData nextData = drawPile[0];
        previewCard = SpawnCard(nextData);
        previewCard.ShowAsPreview(previewPosition, previewAlpha);
    }

    /// <summary>
    /// 대기 중이던 미리보기 카드를 실제로 덱에서 꺼내 지정된 슬롯으로 끌어와 사용 가능하게 만듭니다.
    /// 이후 곧바로 다음 미리보기 카드를 준비합니다.
    /// </summary>
    private void DrawCardIntoSlot(int slotIndex)
    {
        if (previewCard == null) return; // 더 이상 뽑을 카드가 없으면 슬롯은 빈 채로 남음

        drawPile.RemoveAt(0);

        ScriptDragManager drawnCard = previewCard;
        spawnedCards[slotIndex] = drawnCard;
        drawnCard.DrawIntoSlot(slotPositions[slotIndex]);

        PrepareNextPreview();
    }

    /// <summary>
    /// 카드를 사용했을 때 호출되어 해당 카드를 제거하고, 대기 중이던 미리보기 카드를 같은 슬롯으로 끌어옵니다.
    /// 사용되지 않은 나머지 카드들은 제자리에 그대로 유지됩니다.
    /// </summary>
    public void OnCardUsed(ScriptDragManager usedCard)
    {
        // 1. 사용된 카드가 있던 슬롯(인덱스)을 먼저 확인
        int slotIndex = spawnedCards.IndexOf(usedCard);

        // 2. 사용된 카드 UI 제거
        Destroy(usedCard.gameObject);

        // 3. 사용한 카드를 덱 맨 아래로 추가
        drawPile.Add(usedCard.Data);

        if (slotIndex < 0) return; // 안전장치: 슬롯을 찾지 못하면 교체하지 않음

        spawnedCards[slotIndex] = null;

        // 덱이 바닥나 미리보기가 비어 있던 상태였다면, 방금 돌아온 카드로 다시 준비
        if (previewCard == null)
        {
            PrepareNextPreview();
        }

        // 4. 대기 중이던 미리보기 카드를 빈 슬롯으로 끌어옴
        DrawCardIntoSlot(slotIndex);
    }

    /// <summary>
    /// 프리팹을 Instantiate하고 ScriptData를 카드 인스턴스에 연동합니다.
    /// </summary>
    private ScriptDragManager SpawnCard(ScriptData data)
    {
        GameObject newCardObj = Instantiate(cardPrefab, handParent);
        ScriptDragManager cardUI = newCardObj.GetComponent<ScriptDragManager>();
        cardUI.Setup(data);
        return cardUI;
    }
}
