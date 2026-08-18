using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupButtonListener : MonoBehaviour
{
    [SerializeField] private Button s_againBtn; // 성공 - 재촬영
    [SerializeField] private Button s_backBtn; // 성공 - 나가기
    [SerializeField] private Button f_againBtn; // 실패 - 그대로 다시
    [SerializeField] private Button f_recordBtn; // 실패 - 녹화 다시
    [SerializeField] private Button f_backBtn; // 실패 - 나가기
    [SerializeField] private Button f_previewBtn; // 실패 - 프리뷰다시
    [SerializeField] private Button p_recordBtn; // 프리뷰 - 녹화
    [SerializeField] private Button p_backBtn; // 프리뷰 - 나가기
    [SerializeField] private Button p_againBtn; //프리뷰 - 프리뷰다시
    [SerializeField] private Button settingBtn;
    [SerializeField] private GameObject SuccessPopup;
    [SerializeField] private GameObject PreviewPopup;
    [SerializeField] private GameObject VolumeSettingPopup;
    [SerializeField] private List<Button> settingPopupBtns;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button volumeConfirmBtn; // 변경사항 확정
    [SerializeField] private Button volumeCancelBtn; // 변경사항 취소
    private bool isSettingOn = false;
    private float bgmVolumeBeforeEdit;
    private float sfxVolumeBeforeEdit;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static PopupButtonListener instance {get; private set;}

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else Destroy(gameObject);

        s_againBtn.onClick.AddListener(s_again);
        s_backBtn.onClick.AddListener(s_back);
        f_againBtn.onClick.AddListener(f_again);
        f_recordBtn.onClick.AddListener(f_record);
        f_backBtn.onClick.AddListener(f_back);
        f_previewBtn.onClick.AddListener(f_preview);
        p_recordBtn.onClick.AddListener(p_record);
        p_backBtn.onClick.AddListener(p_back);
        p_againBtn.onClick.AddListener(p_again);
        settingBtn.onClick.AddListener(openSetting);
        settingPopupBtns[0].onClick.AddListener(closeSetting);
        settingPopupBtns[1].onClick.AddListener(f_again);
        settingPopupBtns[2].onClick.AddListener(f_record);
        settingPopupBtns[3].onClick.AddListener(f_preview);
        settingPopupBtns[4].onClick.AddListener(f_back);
        settingPopupBtns[5].onClick.AddListener(closeSetting);
        settingPopupBtns[6].onClick.AddListener(p_again);
        settingPopupBtns[7].onClick.AddListener(p_record);
        settingPopupBtns[8].onClick.AddListener(p_back);
        settingPopupBtns[9].onClick.AddListener(activateSetting);
        settingPopupBtns[10].onClick.AddListener(activateSetting);

        bgmVolumeSlider.onValueChanged.AddListener(onBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(onSFXVolumeChanged);
        volumeConfirmBtn.onClick.AddListener(confirmVolumeSetting);
        volumeCancelBtn.onClick.AddListener(cancelVolumeSetting);
    }
    private void LockPlayerControl()
    {
        ActionControlModeManager.LockInput();
        ScriptDragManager.LockInput();
    }
    private void UnlockPlayerControl()
    {
        ActionControlModeManager.UnlockInput();
        ScriptDragManager.UnlockInput();
    }
    private void s_again()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        UnlockPlayerControl();
        SceneManager.LoadScene(StageImporter.sceneName);
    }
    private void s_back()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        UnlockPlayerControl();
        SceneManager.LoadScene("StageScene");
    }
    private void f_again()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        UnlockPlayerControl();
        SceneManager.LoadScene(StageImporter.sceneName);
    }
    private void f_record()
    {
        UnlockPlayerControl();
        StageModeSetting.setMode(false);
        SceneManager.LoadScene("RecordDevelopingScene");
    }
    private void f_back()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        UnlockPlayerControl();
        SceneManager.LoadScene("StageScene");
    }
    private void f_preview()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        UnlockPlayerControl();
        StageModeSetting.setMode(true);
        SceneManager.LoadScene(StageImporter.sceneName);
    }
    private void p_record()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        UnlockPlayerControl();
        StageModeSetting.setMode(false);
        SceneManager.LoadScene("RecordDevelopingScene");
    }
    private void p_back()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        UnlockPlayerControl();
        SceneManager.LoadScene("StageScene");
    }
    private void p_again()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        UnlockPlayerControl();
        StageModeSetting.setMode(true);
        SceneManager.LoadScene(StageImporter.sceneName);
    }
    private void openSetting()
    {
        if(isSettingOn) return;
        AudioManager.instance.PlaySFX(SFXType.button2);
        if(ActionControlModeManager.IsInputLocked || ScriptDragManager.IsInputLocked) return;
        isSettingOn = true;
        LockPlayerControl();
        Debug.Log("asdf");
        GameObject popup = StageModeSetting.isPreview ? PreviewPopup : SuccessPopup;

        RectTransform popupRect = popup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();

        popup.SetActive(true);

        popupRect.DOKill();
        canvasGroup.DOKill();

        popupRect.localScale = Vector3.one*0.8f;
        canvasGroup.alpha = 0f;

        canvasGroup.DOFade(1f, 0.15f);

        popupRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
    public void closeSetting()
    {
        AudioManager.instance.PlaySFX(SFXType.button1);
        GameObject popup = StageModeSetting.isPreview ? PreviewPopup : SuccessPopup;

        RectTransform popupRect = popup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();

        popupRect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.DOFade(0f, 0.15f);

        popupRect.DOScale(0.8f, 0.25f).SetEase(Ease.InBack)
            .OnComplete(() => popup.SetActive(false));

        isSettingOn = false;
        UnlockPlayerControl();
    }
    public void activateSetting()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        bgmVolumeBeforeEdit = VolumeManager.instance.bgmVolume;
        sfxVolumeBeforeEdit = VolumeManager.instance.sfxVolume;

        bgmVolumeSlider.SetValueWithoutNotify(bgmVolumeBeforeEdit);
        sfxVolumeSlider.SetValueWithoutNotify(sfxVolumeBeforeEdit);

        RectTransform popupRect = VolumeSettingPopup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = VolumeSettingPopup.GetComponent<CanvasGroup>();

        VolumeSettingPopup.SetActive(true);

        popupRect.DOKill();
        canvasGroup.DOKill();

        popupRect.localScale = Vector3.one*0.8f;
        canvasGroup.alpha = 0f;

        canvasGroup.DOFade(1f, 0.15f);

        popupRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
    private void closeVolumeSetting()
    {
        AudioManager.instance.PlaySFX(SFXType.button2);
        RectTransform popupRect = VolumeSettingPopup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = VolumeSettingPopup.GetComponent<CanvasGroup>();

        popupRect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.DOFade(0f, 0.15f);

        popupRect.DOScale(0.8f, 0.25f).SetEase(Ease.InBack)
            .OnComplete(() => VolumeSettingPopup.SetActive(false));
    }
    private void onBGMVolumeChanged(float value)
    {
        VolumeManager.instance.SetBGMVolume(value);
    }
    private void onSFXVolumeChanged(float value)
    {
        VolumeManager.instance.SetSFXVolume(value);
    }
    private void confirmVolumeSetting()
    {
        closeVolumeSetting();
    }
    private void cancelVolumeSetting()
    {
        VolumeManager.instance.SetBGMVolume(bgmVolumeBeforeEdit);
        VolumeManager.instance.SetSFXVolume(sfxVolumeBeforeEdit);

        bgmVolumeSlider.SetValueWithoutNotify(bgmVolumeBeforeEdit);
        sfxVolumeSlider.SetValueWithoutNotify(sfxVolumeBeforeEdit);

        closeVolumeSetting();
    }
}
