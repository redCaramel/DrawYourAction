using System.Collections.Generic;
using UnityEngine;

public class InitialCutSceneManager : MonoBehaviour
{
    public bool isCutSceneShowing;

    public int CutSceneNum;

    [SerializeField] private List<GameObject> cutSceneObjects;
    [SerializeField] private PlayerCameraController playerCameraController;

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
            FinishCutScene();
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

        OnCutSceneEnded?.Invoke();
    }
}
