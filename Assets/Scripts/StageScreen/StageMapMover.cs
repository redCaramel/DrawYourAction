using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageMapMover : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private bool isDragging;
    private Vector3 dragOriginWorld;

    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverButton())
        {
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
        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        worldCamera.transform.position = position;
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
