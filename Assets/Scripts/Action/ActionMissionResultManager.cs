using UnityEngine;
using DG.Tweening;
public class ActionMissionResultManager : MonoBehaviour
{
    [SerializeField] GameObject SuccessPopup;
    [SerializeField] GameObject FailurePopup;
    [SerializeField] GameObject PreviewPopup;
    private bool isFinished = false;

    /// <summary>MissonSuccess() 또는 MissionFailure()가 호출되면(둘 중 먼저 호출되는 한 번만) 발생.</summary>
    public event System.Action OnMissionFinished;
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

    /// <summary>
    /// 카드 사용 / 키보드 조작 입력을 모두 막고, 플레이어를 그 자리에서 즉시 정지시킨다.
    /// </summary>
    private void LockPlayerControl()
    {
        // closeSetting()이 내부적으로 UnlockInput()을 호출하므로, 잠금 로직보다 먼저 실행해야
        // 아래에서 거는 잠금이 곧바로 풀리지 않는다.
        if (PopupButtonListener.instance != null)
            PopupButtonListener.instance.closeSetting();

        ActionControlModeManager.LockInput();
        ScriptDragManager.LockInput();

        if (ActionExecuter.instance != null)
            ActionExecuter.instance.StopLoading();

        if (PlayerController.instance != null)
            PlayerController.instance.SetControlLocked(true);
    }

    public void MissonSuccess()
    {
        if(isFinished) return;
        AudioManager.instance.PlaySFX(SFXType.success);
        isFinished = true;
        OnMissionFinished?.Invoke();
        LockPlayerControl();

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
            StageClearManager.activateStage(StageImporter.stageCount+1);
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
        if(isFinished) return;
        AudioManager.instance.PlaySFX(SFXType.fail);
        isFinished = true;
        OnMissionFinished?.Invoke();
        LockPlayerControl();

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
