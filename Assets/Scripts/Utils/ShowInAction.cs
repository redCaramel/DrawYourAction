using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// InitialCutSceneManager의 초기 컷씬이 끝나면(FinishCutScene 호출 시) targets에 등록된 오브젝트들을 모두 활성화하고
/// 위로 떠오르는 연출과 함께 등장시킨다.
/// </summary>
public class ShowInAction : MonoBehaviour
{
    [Tooltip("컷씬이 끝났을 때 활성화 및 상승 연출을 적용할 오브젝트들. 비워두면 이 스크립트가 붙은 오브젝트를 사용")]
    [SerializeField] private List<GameObject> targets;

    [Tooltip("위로 떠오르는 거리")]
    [SerializeField] private float riseDistance = 1f;

    [Tooltip("떠오르는 데 걸리는 시간")]
    [SerializeField] private float riseDuration = 1f;

    [SerializeField] private Ease riseEase = Ease.OutCubic;

    private List<GameObject> Targets => (targets != null && targets.Count > 0)
        ? targets
        : new List<GameObject> { gameObject };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Awake 타이밍 의존성을 피하기 위해 모든 Awake가 끝난 뒤인 Start에서 구독
        if (InitialCutSceneManager.instance != null)
        {
            InitialCutSceneManager.instance.OnCutSceneEnded += HandleCutSceneEnded;
        }
        else
        {
            Debug.LogWarning($"{name}: InitialCutSceneManager.instance가 존재하지 않아 컷씬 종료 이벤트를 구독하지 못했습니다.");
        }
    }

    private void OnDestroy()
    {
        if (InitialCutSceneManager.instance != null)
        {
            InitialCutSceneManager.instance.OnCutSceneEnded -= HandleCutSceneEnded;
        }
    }

    private void HandleCutSceneEnded()
    {
        foreach (GameObject obj in Targets)
        {
            if (obj == null) continue;

            obj.SetActive(true);

            Vector3 startPos = obj.transform.position;
            Vector3 endPos = startPos + Vector3.up * riseDistance;

            obj.transform.position = startPos;
            obj.transform.DOMove(endPos, riseDuration).SetEase(riseEase);
        }
    }
}
