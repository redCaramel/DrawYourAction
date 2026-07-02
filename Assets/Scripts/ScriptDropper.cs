using UnityEngine;
using UnityEngine.EventSystems;

public class ScriptDropper : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null) return;

        ScriptDragger item = dragged.GetComponent<ScriptDragger>();
        if (item == null) return;

        // 슬롯 안에 이미 아이템이 있으면 처리 방식은 자유 (예: 막기, 교체 등)
        if (transform.childCount > 0)
        {
            Debug.Log("이미 아이템이 있는 슬롯입니다.");
            return;
        }

        // 드래그된 아이템을 이 슬롯의 자식으로 설정
        dragged.transform.SetParent(transform);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }
}