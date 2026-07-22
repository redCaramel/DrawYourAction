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
    }

    // ----------------------------------------------------

    [Header("[프리팹 및 트랜스폼 설정]")]
    [SerializeField] private GameObject cardPrefab;       // 생성할 CardUI 프리팹
    [SerializeField] private Transform handParent;        // 카드가 들어갈 UI 부모 (Canvas 내부의 Panel/RectTransform)

    [Header("[카드 생성 및 배치 설정]")]
    private int initialHandSize;     // 초기에 들고 시작할 카드 수
    [SerializeField] private float cardSpacing = 200f;    // 카드 간의 가로 간격
    [SerializeField] private float handCenterY = -350f;   // 손패의 Y축 기본 위치
    [SerializeField] private float spawnInterval = 0.15f; // 카드 생성 시 차례대로 튀어나오는 시간 간격

    // 실시간 관리용 데이터 및 생성된 CardUI 리스트
    private List<ScriptData> drawPile = new List<ScriptData>();
    private List<ScriptDragManager> spawnedCards = new List<ScriptDragManager>();
    // 슬롯(인덱스)별 고정 좌표. 카드가 교체되어도 다른 슬롯의 카드는 움직이지 않음
    private List<Vector2> slotPositions = new List<Vector2>();

    private void Start()
    {
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

        // 2. ScriptImporter가 불러온 스크립트 데이터를 덱으로 복사 (순서 유지)
        drawPile.AddRange(ScriptImporter.scripts ?? new List<ScriptData>());

        // 3. 손패 슬롯들의 고정 좌표를 미리 계산 (이후 카드가 교체되어도 이 좌표는 변하지 않음)
        int countToSpawn = Mathf.Min(initialHandSize, drawPile.Count);
        CalculateSlotPositions(countToSpawn);

        // 4. countToSpawn만큼 코루틴을 통해 순차적으로 생성 및 연출
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
    /// 초기에 카드가 하나씩 차례대로 아래에서 튀어나오는 연출 코루틴
    /// </summary>
    private IEnumerator Co_SpawnInitialHand(int countToSpawn)
    {
        for (int i = 0; i < countToSpawn; i++)
        {
            // 덱 맨 위 데이터 추출
            ScriptData cardData = drawPile[0];
            drawPile.RemoveAt(0);

            // 프리팹 생성 및 데이터 연동
            ScriptDragManager cardUI = SpawnCard(cardData);
            spawnedCards.Add(cardUI);

            // 자신의 슬롯 고정 위치로만 등장 연출 (다른 카드는 영향 없음)
            cardUI.AnimateSpawn(slotPositions[i]);

            // 약간의 시차를 두어 릴레이로 튀어나오게 만듦
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 카드를 사용했을 때 호출되어 해당 카드를 제거하고, 같은 슬롯 위치에 새 카드로 교체합니다.
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

        // 4. 덱에 남은 카드가 있다면 같은 슬롯에 새 카드로 교체
        if (drawPile.Count > 0)
        {
            ScriptData nextCardData = drawPile[0];
            drawPile.RemoveAt(0);

            ScriptDragManager newCardUI = SpawnCard(nextCardData);
            spawnedCards[slotIndex] = newCardUI;

            // 비워진 슬롯의 고정 위치로만 등장 연출
            newCardUI.AnimateSpawn(slotPositions[slotIndex]);
        }
        else
        {
            // 보충할 카드가 없다면 해당 슬롯은 빈 자리로 남김
            spawnedCards[slotIndex] = null;
        }
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

    /// <summary>
    /// 현재 생성된 CardUI들을 중앙 정렬 기준으로 가로 위치를 계산하여 정렬합니다.
    /// </summary>
    private void UpdateCardPositions()
    {
        int cardCount = spawnedCards.Count;
        if (cardCount == 0) return;

        // 중앙 기준 좌우 배치 공식
        // 예: 3장일 때 -> -cardSpacing, 0, +cardSpacing
        float startX = -((cardCount - 1) * cardSpacing) / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            Vector2 targetPos = new Vector2(startX + (i * cardSpacing), handCenterY);
            
            // 기존 CardUI의 DOTween 연출 실행
            spawnedCards[i].AnimateSpawn(targetPos);
        }
    }
}