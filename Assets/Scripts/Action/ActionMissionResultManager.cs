using UnityEngine;
using DG.Tweening;
public class ActionMissionResultManager : MonoBehaviour
{
    [SerializeField] GameObject SuccessPopup;
    [SerializeField] GameObject FailurePopup;
    [SerializeField] GameObject PreviewPopup;
    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ActionMissionResultManager instance {get; private set;}

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
        SuccessPopup.SetActive(false);
        FailurePopup.SetActive(false);
        PreviewPopup.SetActive(false);

    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    public void MissonSuccess()
    {
        RectTransform popupRect;
        CanvasGroup canvasGroup;
        if(StageModeSetting.isPreview)
        {
            popupRect = PreviewPopup.GetComponent<RectTransform>();
            canvasGroup = PreviewPopup.GetComponent<CanvasGroup>();
            PreviewPopup.SetActive(true);
        }
        else
        {
            popupRect = SuccessPopup.GetComponent<RectTransform>();
            canvasGroup = SuccessPopup.GetComponent<CanvasGroup>();
            SuccessPopup.SetActive(true);
        }
        
        popupRect.DOKill();
        canvasGroup.DOKill();

        popupRect.localScale = Vector3.one*0.8f;
        canvasGroup.alpha = 0f;

        canvasGroup.DOFade(1f, 0.15f);

        popupRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
    public void MissionFailure()
    {
        RectTransform popupRect = FailurePopup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = FailurePopup.GetComponent<CanvasGroup>();
        FailurePopup.SetActive(true);

        popupRect.DOKill();
        canvasGroup.DOKill();

        popupRect.localScale = Vector3.one*0.8f;
        canvasGroup.alpha = 0f;

        canvasGroup.DOFade(1f, 0.15f);

        popupRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
}
