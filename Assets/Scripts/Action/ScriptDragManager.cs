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
    [SerializeField] private float drawDuration = 0.35f; // 좌측 상단 대기 위치에서 손패 슬롯으로 끌려오는 시간
    [SerializeField] private float previewScale = 0.6f;  // 미리보기 상태일 때의 축소 비율

    [Header("카드 데이터 표시")]
    [SerializeField] private TMP_Text titleText; // 카드에 표시할 스크립트 이름

    private Vector2 originalPosition;
    private Canvas parentCanvas;
    private bool isUsingCard = false;
    private bool isInteractable = false; // 좌측 상단 미리보기 상태에서는 입력을 받지 않음

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
    /// [1] 다음에 뽑힐 카드를 좌측 상단에 희미하게 미리 배치 (아직 사용 불가 상태)
    /// </summary>
    public void ShowAsPreview(Vector2 previewPosition, float previewAlpha)
    {
        isUsingCard = false;
        isInteractable = false;

        rectTransform.DOKill();
        canvasGroup.DOKill();

        originalPosition = previewPosition;
        rectTransform.anchoredPosition = previewPosition;
        rectTransform.localScale = Vector3.one * previewScale;
        canvasGroup.alpha = previewAlpha;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// [2] 좌측 상단에서 대기하던 카드를 손패의 빈 슬롯으로 끌어와 사용 가능하게 만드는 연출
    /// </summary>
    public void DrawIntoSlot(Vector2 targetPosition)
    {
        originalPosition = targetPosition;
        isUsingCard = false;
        isInteractable = true;

        rectTransform.DOKill();
        canvasGroup.DOKill();

        canvasGroup.blocksRaycasts = true;
        rectTransform.DOAnchorPos(targetPosition, drawDuration).SetEase(Ease.OutBack);
        rectTransform.DOScale(1f, drawDuration).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, drawDuration);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isUsingCard || !isInteractable) return;

        // 클릭 시 약간 커지는 연출 (피드백)
        rectTransform.DOScale(1.05f, 0.1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isUsingCard || !isInteractable) return;

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
        if (isUsingCard || !isInteractable) return;

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
    /// [3] 상단으로 던져서 사용될 때 연출
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
            // 카드 사용 처리 후, 매니저를 통해 다음 카드를 빈 슬롯으로 끌어오게 함
            CardSpawnManager.instance.OnCardUsed(this);
        });
    }
}
