using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TItleStarter : MonoBehaviour
{
    // 타이틀에서 이동할 씬 이름
    private const string StageSceneName = "StageScene";

    [System.Serializable]
    private class TitleMenuButton
    {
        public Button button;
        public CanvasGroup canvasGroup; // 등장 애니메이션용 (버튼 텍스트 전체)
        public RectTransform rectTransform; // 위에서 내려오는 애니메이션용
        public CanvasGroup highlightGroup; // 마우스 오버 시 페이드 인/아웃되는 Highlight 오브젝트
    }

    [Header("Title Text")]
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private RectTransform titleRectTransform;

    [Header("Menu Buttons")]
    [SerializeField] private TitleMenuButton startButton; // 처음부터 시작
    [SerializeField] private TitleMenuButton continueButton; // 이어하기
    [SerializeField] private TitleMenuButton exitButton; // 나가기
    [SerializeField] private TextMeshProUGUI continueButtonText; // 세이브 파일 없을 시 회색으로 변경

    [Header("Intro Animation")]
    [SerializeField] private float dropDistance = 60f; // 위에서 내려오는 시작 거리
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float staggerDelay = 0.2f; // 항목 간 등장 간격
    [SerializeField] private Ease dropEase = Ease.OutCubic;

    [Header("Highlight Hover")]
    [SerializeField] private float highlightFadeDuration = 0.15f;

    [Header("Continue Button Color")]
    [SerializeField] private Color availableColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;

    private SaveData loadedSaveData; // Start에서 한 번 불러와 이어하기 버튼/볼륨 세팅에 재사용

    private void Awake()
    {
        startButton.button.onClick.AddListener(OnStartButtonClicked);
        continueButton.button.onClick.AddListener(OnContinueButtonClicked);
        exitButton.button.onClick.AddListener(OnExitButtonClicked);

        SetupHighlightHover(startButton);
        SetupHighlightHover(continueButton);
        SetupHighlightHover(exitButton);
    }

    private void Start()
    {
        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSaveFile();
        loadedSaveData = SaveManager.Instance != null ? SaveManager.Instance.LoadGame() : new SaveData();

        ApplyContinueButtonState(hasSave);
        ApplyVolumeSettings(loadedSaveData);
        PlayIntroAnimation();
    }

    // 세이브 파일이 없으면 이어하기 버튼을 비활성화하고 텍스트를 회색으로 표시
    private void ApplyContinueButtonState(bool hasSave)
    {
        continueButton.button.interactable = hasSave;
        if (continueButtonText != null)
        {
            continueButtonText.color = hasSave ? availableColor : disabledColor;
        }
    }

    // SaveManager의 LoadGame 결과를 기준으로 VolumeManager의 볼륨 값을 시작하자마자 반영
    // LoadVolume은 자동 저장을 유발하지 않으므로, 아직 세이브에서 불러오지 않은
    // StageClearManager.currentStage(기본값)가 세이브 파일을 덮어쓰는 일이 없다.
    private void ApplyVolumeSettings(SaveData data)
    {
        VolumeManager.instance.LoadVolume(data.bgmVolume, data.sfxVolume);
    }

    // 타이틀 텍스트 -> 시작 -> 이어하기 -> 나가기 순으로, 투명한 상태에서 위에서 내려오며 하나씩 페이드 인
    private void PlayIntroAnimation()
    {
        CanvasGroup[] groups = { titleCanvasGroup, startButton.canvasGroup, continueButton.canvasGroup, exitButton.canvasGroup };
        RectTransform[] rects = { titleRectTransform, startButton.rectTransform, continueButton.rectTransform, exitButton.rectTransform };

        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < groups.Length; i++)
        {
            CanvasGroup group = groups[i];
            RectTransform rect = rects[i];
            if (group == null || rect == null) continue;

            Vector2 targetPos = rect.anchoredPosition;
            Vector2 startPos = targetPos + Vector2.up * dropDistance;

            group.DOKill();
            rect.DOKill();
            group.alpha = 0f;
            rect.anchoredPosition = startPos;

            sequence.Insert(i * staggerDelay, group.DOFade(1f, dropDuration).SetEase(dropEase));
            sequence.Insert(i * staggerDelay, rect.DOAnchorPos(targetPos, dropDuration).SetEase(dropEase));
        }
    }

    // 버튼에 마우스를 올리면 Highlight가 페이드 인, 벗어나면 페이드 아웃되도록 연결
    private void SetupHighlightHover(TitleMenuButton target)
    {
        if (target.button == null || target.highlightGroup == null) return;

        target.highlightGroup.alpha = 0f;
        target.highlightGroup.blocksRaycasts = false; // 버튼의 마우스 이벤트를 가리지 않도록
        target.highlightGroup.interactable = false;

        ButtonHighlightHover hover = target.button.gameObject.AddComponent<ButtonHighlightHover>();
        hover.Setup(target.highlightGroup, highlightFadeDuration);
    }

    private void OnStartButtonClicked()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(SFXType.button2);
        StageClearManager.currentStage = 1;
        SceneManager.LoadScene(StageSceneName);
    }

    private void OnContinueButtonClicked()
    {
        if (!continueButton.button.interactable) return;

        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(SFXType.button2);
        StageClearManager.currentStage = Mathf.Max(1, loadedSaveData.currentStage);
        SceneManager.LoadScene(StageSceneName);
    }

    private void OnExitButtonClicked()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(SFXType.button1);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // 버튼의 Highlight CanvasGroup을 마우스 오버 여부에 따라 페이드시키는 헬퍼 컴포넌트
    private class ButtonHighlightHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private CanvasGroup highlight;
        private float fadeDuration;

        public void Setup(CanvasGroup highlightGroup, float duration)
        {
            highlight = highlightGroup;
            fadeDuration = duration;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlight.DOKill();
            highlight.DOFade(1f, fadeDuration);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlight.DOKill();
            highlight.DOFade(0f, fadeDuration);
        }
    }
}
