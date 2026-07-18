using System.Collections.Generic;
using UnityEngine;

public class ScriptSettingManager : MonoBehaviour
{
    
    [SerializeField] private Vector2 StartVector;
    [SerializeField] private float XSpacing;
    [SerializeField] private float YSpacing;
    [SerializeField] private GameObject SlotPrefab;
    [SerializeField] private GameObject contentParent;
    private int instanceCount;

    private const int SlotsPerRow = 6;

    private void Start()
    {   
        instanceCount = CreateRecordInstance.InstanceCount;
        init();
    }

    private void init()
    {
        for (int i = 0; i < instanceCount; i++)
        {
            int row = i / SlotsPerRow;
            int col = i % SlotsPerRow;

            GameObject slot = Instantiate(SlotPrefab, contentParent.transform);
            RectTransform rect = slot.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(StartVector.x + col * XSpacing, StartVector.y -  row * YSpacing);
            slot.GetComponent<ScriptDropper>().slotIndex = i;
        }
    }

}
