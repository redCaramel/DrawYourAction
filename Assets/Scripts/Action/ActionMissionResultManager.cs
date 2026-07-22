using UnityEngine;
using DG.Tweening;
public class ActionMissionResultManager : MonoBehaviour
{
    [SerializeField] GameObject SuccessPopup;
    [SerializeField] GameObject FailurePopup;
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
        RectTransform popupRect = SuccessPopup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = SuccessPopup.GetComponent<CanvasGroup>();

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

        popupRect.DOKill();
        canvasGroup.DOKill();

        popupRect.localScale = Vector3.one*0.8f;
        canvasGroup.alpha = 0f;

        canvasGroup.DOFade(1f, 0.15f);

        popupRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
}
