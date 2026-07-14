using UnityEngine;
using UnityEngine.EventSystems;

public class ScriptDropper : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public int SlotIndex => slotIndex;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null) return;

        ScriptDragger draggedItem = dragged.GetComponent<ScriptDragger>();
        if (draggedItem == null) return;
        if(ScriptDataManager.instance.getScript(draggedItem.ScriptIndex).status!=3) return;

        Transform draggedOriginalParent = draggedItem.OriginalParent;

        if (transform.childCount > 0)
        {
            Transform existingChild = transform.GetChild(0);
            
            existingChild.SetParent(draggedOriginalParent);
            existingChild.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            ScriptDragger existingItem = existingChild.GetComponent<ScriptDragger>();
            ScriptDropper originalDropper = draggedOriginalParent != null
                ? draggedOriginalParent.GetComponent<ScriptDropper>()
                : null;
            if (originalDropper != null && existingItem != null)
            {
                ScriptArrManager.instance.SetScriptAtSlot(originalDropper.SlotIndex, existingItem.ScriptIndex);
            }
        }

        // 드래그된 아이템을 이 슬롯의 자식으로 설정
        dragged.transform.SetParent(transform);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        ScriptArrManager.instance.SetScriptAtSlot(slotIndex, draggedItem.ScriptIndex);
    }
}
