using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEditor.AssetImporters;
using UnityEngine;

public class PopupAnim : MonoBehaviour
{
    [SerializeField] private RectTransform popup; // 재사용되는 단일 팝업 오브젝트 (우측 anchor 설정 필수, World Space Canvas 소속)
    [SerializeField] private List<GameObject> buttons; // num번째 버튼 오브젝트
    [SerializeField] private Camera worldCamera; // btn과 popup을 비추는 월드 카메라 (World Space Canvas 기준)
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float focusOrthoSize = 3f; // 버튼 포커스 시 목표로 하는 orthographicSize (이 값까지 확대)

    [SerializeField] private TextMeshProUGUI sceneNum;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI missions;
    [SerializeField] private TextMeshProUGUI scriptTimes;

    // popup이 카메라 대비 유지해야 하는 상대 위치 (Start 시점, 즉 디자인상의 초기 배치를 기준으로 고정)
    private Vector3 popupOffsetFromCamera;
    private bool isVisible;
    private CanvasGroup popupCanvasGroup;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static PopupAnim instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }
    private void Awake()
    {
        if (instance == null)
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

    private void Start()
    {
        if (popup != null && worldCamera != null)
        {
            popupOffsetFromCamera = popup.position - worldCamera.transform.position;
        }
        if (popup != null)
        {
            popupCanvasGroup = popup.GetComponent<CanvasGroup>();
            if (popupCanvasGroup == null)
            {
                popupCanvasGroup = popup.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    // 슬라이드 애니메이션이 끝난 뒤에도, 카메라가 계속 움직이는 동안(드래그 등) popup이 항상 같은 상대 위치를 유지하도록 추적
    private void LateUpdate()
    {
        if (!isVisible || popup == null || worldCamera == null) return;
        if (DOTween.IsTweening(popup)) return; // 슬라이드 트윈 중에는 트윈이 위치를 담당
        popup.position = worldCamera.transform.position + popupOffsetFromCamera;
    }

    public static void ShowPopup(int num)
    {
        if (instance == null)
        {
            Debug.LogWarning("[PopupAnim] instance가 존재하지 않아 팝업을 실행할 수 없습니다.");
            return;
        }
        instance.importData();
        instance.Play(num - 1);
    }

    public static void HidePopup()
    {
        if (instance == null)
        {
            Debug.LogWarning("[PopupAnim] instance가 존재하지 않아 팝업을 닫을 수 없습니다.");
            return;
        }
        instance.Hide();
    }

    private void Play(int index)
    {
        if (buttons == null || index < 0 || index >= buttons.Count || buttons[index] == null)
        {
            Debug.LogWarning($"[PopupAnim] buttons 리스트에서 인덱스 {index}를 찾을 수 없습니다.");
            return;
        }
        if (popup == null)
        {
            Debug.LogWarning("[PopupAnim] popup이 할당되지 않았습니다.");
            return;
        }
        if (worldCamera == null)
        {
            Debug.LogWarning("[PopupAnim] worldCamera가 할당되지 않았습니다.");
            return;
        }
        if (!worldCamera.orthographic)
        {
            Debug.LogWarning("[PopupAnim] worldCamera가 orthographic이 아니면 카메라 정렬 계산이 정확하지 않습니다.");
        }

        GameObject btn = buttons[index];

        popup.DOKill();
        worldCamera.transform.DOKill();
        DOTween.Kill(worldCamera);
        popupCanvasGroup.DOKill();

        // 버튼에 포커스를 맞추며 확대: 목표 orthographicSize(고정 지점)를 먼저 정하고, 그 크기 기준으로 목표 카메라 위치를 계산한다
        StageMapMover mapMover = StageMapMover.instance;
        float targetOrthoSize = worldCamera.orthographicSize;
        if (worldCamera.orthographic)
        {
            targetOrthoSize = mapMover != null
                ? Mathf.Clamp(focusOrthoSize, mapMover.MinZoom, mapMover.MaxZoom)
                : Mathf.Max(focusOrthoSize, 0.01f);
        }

        // btn이 화면의 x축 좌측 25%, y축 상단 50% 지점에 오도록(목표 확대 크기 기준) 카메라를 이동
        float desiredScreenX = Screen.width * 0.25f;
        float desiredScreenY = Screen.height * 0.5f;
        float worldUnitsPerPixel = targetOrthoSize * 2f / Screen.height;

        Vector3 btnWorldPos = btn.transform.position;
        float targetCamX = btnWorldPos.x - (desiredScreenX - Screen.width * 0.5f) * worldUnitsPerPixel;
        float targetCamY = btnWorldPos.y - (desiredScreenY - Screen.height * 0.5f) * worldUnitsPerPixel;

        Vector3 targetCamPos = new Vector3(targetCamX, targetCamY, worldCamera.transform.position.z);
        if (mapMover != null)
        {
            targetCamPos = mapMover.ClampPosition(targetCamPos, targetOrthoSize);
        }

        worldCamera.transform.DOMoveX(targetCamPos.x, slideDuration).SetEase(Ease.OutCubic);
        worldCamera.transform.DOMoveY(targetCamPos.y, slideDuration).SetEase(Ease.OutCubic);
        if (worldCamera.orthographic)
        {
            worldCamera.DOOrthoSize(targetOrthoSize, slideDuration).SetEase(Ease.OutCubic);
        }

        // popup은 캔버스 로컬 좌표가 아니라 카메라 기준 월드 좌표로 위치를 계산해야
        // 카메라가 맵 어디에 있든(이동 중이어도) 항상 카메라 우측에 일정하게 나타난다.
        isVisible = true;
        popup.gameObject.SetActive(true);

        Vector3 restPos = targetCamPos + popupOffsetFromCamera;
        float popupWorldWidth = popup.rect.width * popup.lossyScale.x;
        Vector3 startPos = restPos + new Vector3(popupWorldWidth, 0f, 0f);

        popup.position = startPos;
        popup.DOMove(restPos, slideDuration).SetEase(Ease.OutCubic);

        popupCanvasGroup.alpha = 0f;
        popupCanvasGroup.DOFade(1f, slideDuration).SetEase(Ease.OutCubic);
    }

    // 팝업을 카메라 기준 우측 바깥으로 슬라이드시켜 화면 밖으로 내보낸 뒤 비활성화한다
    private void Hide()
    {
        if (popup == null || !popup.gameObject.activeSelf) return;

        isVisible = false;
        popup.DOKill();
        popupCanvasGroup.DOKill();

        float popupWorldWidth = popup.rect.width * popup.lossyScale.x;
        float cameraWorldWidth = worldCamera.orthographicSize * 2f * worldCamera.aspect;
        Vector3 hiddenPos = popup.position + new Vector3(cameraWorldWidth + popupWorldWidth, 0f, 0f);

        popup.DOMove(hiddenPos, slideDuration).SetEase(Ease.InCubic)
            .OnComplete(() => popup.gameObject.SetActive(false));
        popupCanvasGroup.DOFade(0f, slideDuration).SetEase(Ease.InCubic);
    }

    private void importData()
    {
        sceneNum.text = $"#{StageImporter.stageCount}";
        title.text = StageImporter.title;
        string temp = "";
        for(int i = 0;i < StageImporter.missionList.Count;i++)
        {
            MissionData mission = StageImporter.missionList[i];
            temp += $"- {mission.mainText}\n";
        }
        missions.text = temp;
        temp = "- ";
        int[] times = new int[31];
        for(int i = 0;i < StageImporter.cardTime.Count;i++)
        {
            int time = StageImporter.cardTime[i];
            times[time]++;
        }
        for(int i = 0;i < 31;i++)
        {
            if(times[i] > 0)
            {
                temp += $"{i}초({times[i]}) ";
            }
        }
        scriptTimes.text = temp;
    }
}
