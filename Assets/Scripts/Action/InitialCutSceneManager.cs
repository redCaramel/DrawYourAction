using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class InitialCutSceneManager : MonoBehaviour
{
    public bool isCutSceneShowing;

    public int CutSceneNum;

    [SerializeField] private List<GameObject> cutSceneObjects;
    [SerializeField] private PlayerCameraController playerCameraController;

    [Header("엔딩 텍스트 연출")]
    [SerializeField] private GameObject endingText1;
    [SerializeField] private GameObject endingText2;
    [SerializeField] private GameObject endingText3;
    [SerializeField] private GameObject endingText4;
    [SerializeField] private float endingTextDisplayDuration = 1.5f; // 1~3번 텍스트가 각각 표시되는 시간
    [SerializeField] private float endingText4FadeDelay = 1.5f;      // 4번 텍스트가 표시된 후 사라지기 시작하기까지 대기 시간
    [SerializeField] private float endingText4FadeDuration = 1f;     // 4번 텍스트가 사라지는 데 걸리는 시간

    private GameObject activeCutSceneObject;
    private CutSceneManagerInterface activeCutScene;

    // 컷씬이 끝났을 때(또는 재생할 컷씬이 없을 때) 알림. 카드 등장처럼 컷씬 도중 막아야 하는 연출이 구독.
    public event System.Action OnCutSceneEnded;
    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static InitialCutSceneManager instance {get; private set;}

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
        isCutSceneShowing = true;

        foreach (var obj in cutSceneObjects)
        {
            if (obj != null) obj.SetActive(false);
        }

        if (endingText1 != null) endingText1.SetActive(false);
        if (endingText2 != null) endingText2.SetActive(false);
        if (endingText3 != null) endingText3.SetActive(false);
        if (endingText4 != null) endingText4.SetActive(false);

        // 컷씬이 끝날 때까지(FinishCutScene 호출 전) 카드 사용 / 키보드 조작 입력을 막는다.
        LockPlayerInput();
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    public void StartCutScene()
    {
        for(int i = 0;i < cutSceneObjects.Count;i++)
        {
            activeCutSceneObject = cutSceneObjects[i];
            activeCutScene = activeCutSceneObject.GetComponent<CutSceneManagerInterface>();
            if (activeCutScene == null)
            {
                Debug.LogWarning($"{activeCutSceneObject.name} dont have script implemented CutSceneManagerInterface.");
                FinishCutScene();
                return;
            }

            // 컷씬이 카메라를 직접 움직이는 동안 플레이어 추적 카메라 로직과 충돌하지 않도록 비활성화
            if (playerCameraController != null) playerCameraController.enabled = false;

            activeCutSceneObject.SetActive(true);
            isCutSceneShowing = true;
        }

        
    }

    void Update()
    {
        if (!isCutSceneShowing || activeCutScene == null) return;

        if (activeCutScene.isFinished())
        {
            PlayEndingTextsThenFinish();
        }
    }

    /// <summary>
    /// FinishCutScene()을 호출하기 전에 재생되는 엔딩 텍스트 연출.
    /// 1~3번 텍스트를 차례대로 표시한 뒤, 모두 비활성화하고 4번 텍스트를 표시함과 함께 FinishCutScene()을 호출한다.
    /// 4번 텍스트는 표시된 후 점점 사라진다.
    /// </summary>
    private void PlayEndingTextsThenFinish()
    {
        // Update()가 같은 프레임에 중복으로 진입하지 않도록 즉시 정리
        isCutSceneShowing = false;
        activeCutScene = null;
        int act = (StageImporter.stageCount-1)/5;
        endingText1.GetComponent<TextMeshProUGUI>().text = $"Act {act}";
        endingText2.GetComponent<TextMeshProUGUI>().text = $"Scene #{StageImporter.stageCount}";

        StartCoroutine(Co_PlayEndingTextsThenFinish());
    }

    private IEnumerator Co_PlayEndingTextsThenFinish()
    {
        GameObject[] sequenceTexts = { endingText1, endingText2, endingText3 };

        // 1~3번 텍스트를 차례대로 표시
        foreach (var textObj in sequenceTexts)
        {
            if (textObj == null) continue;
            textObj.SetActive(true);
            yield return new WaitForSeconds(endingTextDisplayDuration);
            textObj.SetActive(false);
        }

        // 1~3번 텍스트를 모두 비활성화한 상태로 4번 텍스트를 표시함과 함께 컷씬을 종료 처리
        CanvasGroup endingText4CanvasGroup = null;
        if (endingText4 != null)
        {
            endingText4CanvasGroup = endingText4.GetComponent<CanvasGroup>();
            if (endingText4CanvasGroup != null) endingText4CanvasGroup.alpha = 1f;
            endingText4.SetActive(true);
        }

        FinishCutScene();

        // 4번 텍스트는 표시된 후 점점 사라진다
        if (endingText4 != null)
        {
            yield return new WaitForSeconds(endingText4FadeDelay);

            if (endingText4CanvasGroup != null)
            {
                endingText4CanvasGroup.DOFade(0f, endingText4FadeDuration)
                    .OnComplete(() => endingText4.SetActive(false));
            }
            else
            {
                endingText4.SetActive(false);
            }
        }
    }

    private void FinishCutScene()
    {
        isCutSceneShowing = false;
        if (activeCutSceneObject != null) activeCutSceneObject.SetActive(false);
        activeCutSceneObject = null;
        activeCutScene = null;

        // 컷씬이 끝났으니 플레이어 추적 카메라 로직을 다시 활성화
        if (playerCameraController != null) playerCameraController.enabled = true;

        // 컷씬이 끝났으니 막아뒀던 카드 사용 / 키보드 조작 입력을 다시 허용
        UnlockPlayerInput();

        OnCutSceneEnded?.Invoke();
    }

    /// <summary>
    /// 컷씬이 재생되는 동안 카드 사용(Card 모드)과 키보드 조작(Record/Preview 모드) 입력을 모두 막는다.
    /// </summary>
    private void LockPlayerInput()
    {
        ActionControlModeManager.LockInput();
        ScriptDragManager.LockInput();
    }
    private void UnlockPlayerInput()
    {
        ActionControlModeManager.UnlockInput();
        ScriptDragManager.UnlockInput();
    }
}
