using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class ScriptDragManager : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [Header("드래그 연출 설정")]
    [SerializeField] private float dragThresholdY = 150f; // 상단으로 얼마나 올려야 카드 사용으로 판정할지
    [SerializeField] private float fadeDuration = 0.25f;  // 사용 시 사라지는 시간

    [Header("등장 연출 설정")]
    [SerializeField] private float spawnOffsetY = -300f; // 아래에서 튀어나올 시작 위치 Y
    [SerializeField] private float spawnDuration = 0.35f; // 등장 애니메이션 시간

    [Header("카드 데이터 표시")]
    [SerializeField] private TMP_Text titleText; // 카드에 표시할 스크립트 이름

    private Vector2 originalPosition;
    private Canvas parentCanvas;
    private bool isUsingCard = false;

    public ScriptData Data { get; private set; }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (titleText == null) titleText = GetComponentInChildren<TMP_Text>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// ScriptImporter로부터 전달된 스크립트 데이터를 이 카드 인스턴스에 연동
    /// </summary>
    public void Setup(ScriptData data)
    {
        Data = data;
        if (titleText != null) titleText.text = data.name;
    }

    /// <summary>
    /// [1] 덱에서 손패 위치로 아래에서 위로 튀어나오는 등장 연출
    /// </summary>
    public void AnimateSpawn(Vector2 targetPosition)
    {
        originalPosition = targetPosition;
        isUsingCard = false;

        // 초기 상태 세팅 (손패 위치보다 아래, 투명도 0)
        rectTransform.anchoredPosition = targetPosition + new Vector2(0, spawnOffsetY);
        canvasGroup.alpha = 0f;

        // Kill()을 통해 기존 진행 중인 트윈이 있다면 중단
        rectTransform.DOKill();
        canvasGroup.DOKill();

        // 팝업 애니메이션: 아래에서 원래 위치로 위치 이동 + 알파값 페이드인
        rectTransform.DOAnchorPos(targetPosition, spawnDuration).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, spawnDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isUsingCard) return;
        
        // 클릭 시 약간 커지는 연출 (피드백)
        rectTransform.DOScale(1.05f, 0.1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isUsingCard) return;

        // 마우스 드래그 위치 반영
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;

        // 기준 위치(originalPosition)보다 위로 올라간 정도 계산
        float deltaY = rectTransform.anchoredPosition.y - originalPosition.y;

        if (deltaY > 0)
        {
            // 위로 올릴수록 점점 희미해짐 (임계값까지 비례하여 Alpha 감소)
            float alpha = Mathf.Clamp01(1f - (deltaY / dragThresholdY));
            canvasGroup.alpha = alpha;
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isUsingCard) return;

        float deltaY = rectTransform.anchoredPosition.y - originalPosition.y;

        // 임계값 이상 상단으로 드래그했을 경우 -> 카드 사용
        if (deltaY >= dragThresholdY)
        {
            UseCardSequence();
        }
        else
        {
            // 미달 시 원위치 복귀 (제자리로 튕겨 돌아옴)
            rectTransform.DOAnchorPos(originalPosition, 0.2f).SetEase(Ease.OutQuad);
            canvasGroup.DOFade(1f, 0.2f);
            rectTransform.DOScale(1f, 0.2f);
        }
    }

    /// <summary>
    /// [2] 상단으로 던져서 사용될 때 연출
    /// </summary>
    private void UseCardSequence()
    {
        isUsingCard = true;
        // 연동된 스크립트 데이터의 액션을 실제로 재생
        ActionExecuter.instance.StartLoading(Data.actions);
        // 연출: 위로 조금 더 날아가면서 완전히 희미해짐
        Vector2 targetDisappearPos = rectTransform.anchoredPosition + new Vector2(0, 100f);

        Sequence useSequence = DOTween.Sequence();
        useSequence.Join(rectTransform.DOAnchorPos(targetDisappearPos, fadeDuration).SetEase(Ease.OutCubic));
        useSequence.Join(canvasGroup.DOFade(0f, fadeDuration));

        useSequence.OnComplete(() =>
        {
            

            // 카드 사용 처리 후, 매니저를 통해 다음 카드를 아래에서 튀어나오게 함
            CardSpawnManager.instance.OnCardUsed(this);
        });
    }
}