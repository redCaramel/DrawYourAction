using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScriptDragger : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private int scriptIndex;
    public int ScriptIndex => scriptIndex;
    public Transform OriginalParent => originalParent;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;
    private Canvas canvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();    

        // CanvasGroup이 없으면 자동으로 추가
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(ActionRecorder.instance.getScript(ScriptIndex).status!=2) return;
        originalParent = transform.parent;
        originalPosition = rectTransform.anchoredPosition;

        // 드래그 중엔 다른 UI의 레이캐스트를 막지 않도록 (드롭 대상 감지를 위해 필수)
        canvasGroup.blocksRaycasts = false;

        // 다른 UI 위에 그려지도록 최상위로 이동
        transform.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(ActionRecorder.instance.getScript(ScriptIndex).status!=2) return;
        // 마우스 이동량만큼 위치 이동
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(ActionRecorder.instance.getScript(ScriptIndex).status!=2) return;
        canvasGroup.blocksRaycasts = true;

        // 유효한 슬롯에 드롭되지 않았다면 원래 자리로 복귀
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}