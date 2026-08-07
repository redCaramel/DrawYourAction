using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; // DOTween 네임스페이스

public class SlateAnimationController : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private CanvasGroup slateCanvasGroup;
    [SerializeField] private RectTransform slateRect;         // 슬레이트 전체
    [SerializeField] private RectTransform slateHead;         // 슬레이트 윗부분 (짝짝이)
    [SerializeField] private TextMeshProUGUI slateInfoText;   // 텍스트 (예: ACT 1 - SCENE 2)

    [Header("사운드")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clapSound;             // 슬레이트 치는 소리

    [Header("애니메이션 설정")]
    [SerializeField] private float openAngle = 30f;           // 벌려지는 각도
    [SerializeField] private float duration = 0.5f;

    private void Awake()
    {
        // 시작할 때는 슬레이트를 숨겨둠
        slateCanvasGroup.alpha = 0f;
        slateCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// 연출 시작 함수 (스테이지 시작 시 호출)
    /// </summary>

    public void PlayStartSlate(string actTitle, System.Action onComplete = null)
    {
        StartCoroutine(Co_PlaySlateSequence(actTitle, onComplete));
    }

    private IEnumerator Co_PlaySlateSequence(string actTitle, System.Action onComplete)
    {
        // 1. UI 활성화 및 데이터 세팅
        slateInfoText.text = actTitle;
        slateCanvasGroup.alpha = 1f;
        slateCanvasGroup.blocksRaycasts = true;

        // 초기 위치/회전 상태 세팅
        slateRect.anchoredPosition = Vector2.zero; // 화면 중앙
        slateHead.localRotation = Quaternion.Euler(0, 0, openAngle); // 입 벌리기

        // 2. 잠시 대기 후 슬레이트 내려치기 (Clap!)
        yield return new WaitForSeconds(0.3f);

        // 짝짝이 내리기 (0도 방향으로 빠르게)
        slateHead.DOLocalRotate(Vector3.zero, 0.12f).SetEase(Ease.InQuad);

        yield return new WaitForSeconds(0.12f);

        // 효과음 재생 & 펀치 감성 (흔들림)
        if (audioSource != null && clapSound != null)
        {
            audioSource.PlayOneShot(clapSound);
        }
        slateRect.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.15f);

        // 3. 0.5초 대기 후 화면 아래로 사라짐
        yield return new WaitForSeconds(0.5f);

        // 아래로 내려가며 페이드 아웃
        slateRect.DOAnchorPosY(-1000f, duration).SetEase(Ease.InBack);
        slateCanvasGroup.DOFade(0f, duration).OnComplete(() =>
        {
            slateCanvasGroup.blocksRaycasts = false;
            // 4. 연출 종료 후 콜백 실행 (게임 시작)
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// 스테이지 클리어 연출 (필요 시 사용)
    /// </summary>
    public void PlayEndSlate(System.Action onComplete = null)
    {
        slateInfoText.text = "CUT! OK!";
        slateCanvasGroup.alpha = 1f;
        slateRect.anchoredPosition = new Vector2(0, -1000f);

        // 아래에서 위로 올라온 뒤 슬레이트 치기
        slateRect.DOAnchorPosY(0f, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            slateHead.DOLocalRotate(Vector3.zero, 0.1f).OnComplete(() =>
            {
                if (audioSource != null && clapSound != null) audioSource.PlayOneShot(clapSound);
                slateRect.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.15f);
                
                DOVirtual.DelayedCall(0.8f, () => { onComplete?.Invoke(); });
            });
        });
    }
}
