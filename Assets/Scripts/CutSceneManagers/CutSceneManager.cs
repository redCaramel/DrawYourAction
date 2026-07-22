using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutSceneManager : MonoBehaviour, CutSceneManagerInterface
{
    [SerializeField] private CutSceneCameraMover cameraMover;
    [SerializeField] private List<CutSceneCameraMover.CutSceneStep> steps;

    private bool finished;

    void Awake()
    {
        if (cameraMover == null) cameraMover = GetComponent<CutSceneCameraMover>();
    }

    void OnEnable()
    {
        finished = false;
        StartCoroutine(Co_Play());
    }

    // steps를 CutSceneCameraMover에 묶어서 넘기고, 모두 끝나면 컷씬을 종료 처리
    private IEnumerator Co_Play()
    {
        yield return cameraMover.PlaySteps(steps);
        finished = true;
    }

    public bool isFinished()
    {
        return finished;
    }
}

// Register this GameObject in InitialCutSceneManager's cutSceneObjects list at the matching index
// CutSceneCameraMover 컴포넌트를 같은 GameObject에 함께 붙여야 함
