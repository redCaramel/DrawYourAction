using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private Button settingButton; // 볼륨 설정 팝업을 여는 버튼
    [SerializeField] private GameObject volumeSettingPopup;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button confirmButton; // 변경사항 확정
    [SerializeField] private Button cancelButton; // 변경사항 취소

    private float bgmVolumeBeforeEdit;
    private float sfxVolumeBeforeEdit;

    void Awake()
    {
        settingButton.onClick.AddListener(OpenVolumeSetting);
        bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        confirmButton.onClick.AddListener(ConfirmVolumeSetting);
        cancelButton.onClick.AddListener(CancelVolumeSetting);
    }

    private void OpenVolumeSetting()
    {
        bgmVolumeBeforeEdit = VolumeManager.instance.bgmVolume;
        sfxVolumeBeforeEdit = VolumeManager.instance.sfxVolume;

        bgmVolumeSlider.SetValueWithoutNotify(bgmVolumeBeforeEdit);
        sfxVolumeSlider.SetValueWithoutNotify(sfxVolumeBeforeEdit);

        StageEntryManager.instance?.SetStageButtonsInteractable(false);
        StageMapMover.LockInput();

        RectTransform popupRect = volumeSettingPopup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = volumeSettingPopup.GetComponent<CanvasGroup>();

        volumeSettingPopup.SetActive(true);

        popupRect.DOKill();
        canvasGroup.DOKill();

        popupRect.localScale = Vector3.one*0.8f;
        canvasGroup.alpha = 0f;

        canvasGroup.DOFade(1f, 0.15f);

        popupRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }

    private void CloseVolumeSetting()
    {
        RectTransform popupRect = volumeSettingPopup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = volumeSettingPopup.GetComponent<CanvasGroup>();

        popupRect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.DOFade(0f, 0.15f);

        popupRect.DOScale(0.8f, 0.25f).SetEase(Ease.InBack)
            .OnComplete(() => volumeSettingPopup.SetActive(false));

        StageEntryManager.instance?.SetStageButtonsInteractable(true);
        StageMapMover.UnlockInput();
    }

    private void OnBGMVolumeChanged(float value)
    {
        VolumeManager.instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        VolumeManager.instance.SetSFXVolume(value);
    }

    private void ConfirmVolumeSetting()
    {
        CloseVolumeSetting();
    }

    private void CancelVolumeSetting()
    {
        VolumeManager.instance.SetBGMVolume(bgmVolumeBeforeEdit);
        VolumeManager.instance.SetSFXVolume(sfxVolumeBeforeEdit);

        bgmVolumeSlider.SetValueWithoutNotify(bgmVolumeBeforeEdit);
        sfxVolumeSlider.SetValueWithoutNotify(sfxVolumeBeforeEdit);

        CloseVolumeSetting();
    }
}
