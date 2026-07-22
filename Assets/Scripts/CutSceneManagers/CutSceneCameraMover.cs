using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 컷씬용 카메라 이동/대기 로직을 모아둔 컴포넌트. CutSceneManager1, 2, 3...에서 이 메소드들을 묶어서 사용
public class CutSceneCameraMover : MonoBehaviour
{
    [System.Serializable]
    public struct CutSceneStep
    {
        public bool isWaitStep;        // true: n초 대기, false: 좌표 이동
        public Vector3 targetPosition; // 이동 단계에서 사용할 목표 좌표
        public float duration;         // 이동에 걸리는 시간 또는 대기 시간(초)
    }

    [SerializeField] private Transform cutSceneCamera;

    void Awake()
    {
        if (cutSceneCamera == null && Camera.main != null)
        {
            cutSceneCamera = Camera.main.transform;
        }
    }

    // 카메라를 duration초에 걸쳐 targetPosition으로 이동
    public IEnumerator MoveTo(Vector3 targetPosition, float duration)
    {
        if (cutSceneCamera == null) yield break;

        Vector3 startPosition = cutSceneCamera.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            cutSceneCamera.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        cutSceneCamera.position = targetPosition;
    }

    // seconds초 동안 대기
    public IEnumerator Wait(float seconds)
    {
        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);
        }
    }

    // steps를 등록된 순서대로 하나씩 재생 (이동/대기를 묶어서 처리)
    public IEnumerator PlaySteps(IEnumerable<CutSceneStep> steps)
    {
        foreach (var step in steps)
        {
            if (step.isWaitStep)
            {
                yield return Wait(step.duration);
            }
            else
            {
                yield return MoveTo(step.targetPosition, step.duration);
            }
        }
    }
}
