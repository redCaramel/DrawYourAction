using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageMapMover : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;
    [SerializeField] private float zoomSpeed = 5f;

    private bool isDragging;
    private Vector3 dragOriginWorld;

    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static StageMapMover instance { get; private set; }

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
        UnlockInput();
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    // ----------------------------------------------------

    public float MinZoom => minZoom;
    public float MaxZoom => maxZoom;

    public static bool IsInputLocked { get; private set; } = false;

    public static void LockInput()
    {
        IsInputLocked = true;
    }
    public static void UnlockInput()
    {
        IsInputLocked = false;
    }

    void Update()
    {
        if (IsInputLocked)
        {
            isDragging = false;
            return;
        }

        if (Input.GetMouseButtonDown(0) && !IsPointerOverButton())
        {
            PopupAnim.HidePopup();
            isDragging = true;
            dragOriginWorld = ScreenToWorld(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 currentWorld = ScreenToWorld(Input.mousePosition);
            Vector3 delta = dragOriginWorld - currentWorld;
            MoveCamera(delta);
        }

        float scroll = Input.mouseScrollDelta.y;
        if (!Mathf.Approximately(scroll, 0f) && !IsPointerOverButton())
        {
            Zoom(-scroll * zoomSpeed);
        }
    }

    private Vector3 ScreenToWorld(Vector3 screenPosition)
    {
        screenPosition.z = -worldCamera.transform.position.z;
        return worldCamera.ScreenToWorldPoint(screenPosition);
    }

    private void MoveCamera(Vector3 delta)
    {
        Vector3 position = worldCamera.transform.position;
        position += delta;
        worldCamera.transform.position = ClampPosition(position, worldCamera.orthographicSize);
    }

    private void Zoom(float sizeDelta)
    {
        if (!worldCamera.orthographic) return;

        worldCamera.orthographicSize = Mathf.Clamp(worldCamera.orthographicSize + sizeDelta, minZoom, maxZoom);
        worldCamera.transform.position = ClampPosition(worldCamera.transform.position, worldCamera.orthographicSize);
    }

    // orthoSize 기준 카메라 뷰(가로/세로 절반 크기)가 항상 minBounds~maxBounds 안에 머무르도록 위치를 보정한다
    public Vector3 ClampPosition(Vector3 position, float orthoSize)
    {
        float halfHeight = orthoSize;
        float halfWidth = orthoSize * worldCamera.aspect;

        position.x = ClampAxis(position.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
        position.y = ClampAxis(position.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
        return position;
    }

    // 카메라 뷰가 bound보다 큰 축(min > max)이면 그 축은 bound 중앙에 고정한다
    private static float ClampAxis(float value, float min, float max)
    {
        return min <= max ? Mathf.Clamp(value, min, max) : (min + max) * 0.5f;
    }

    // 드래그 시작 지점이 Button 위인 경우 클릭 동작과 충돌하지 않도록 카메라 이동을 막는다
    private bool IsPointerOverButton()
    {
        if (EventSystem.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            if (result.gameObject.GetComponentInParent<Button>() != null)
                return true;
        }
        return false;
    }
}
