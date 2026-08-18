using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScriptSettingManager : MonoBehaviour
{
    
    [SerializeField] private Vector2 StartVector;
    [SerializeField] private float XSpacing;
    [SerializeField] private float YSpacing;
    [SerializeField] private GameObject SlotPrefab;
    [SerializeField] private GameObject contentParent;
    private int instanceCount;
    private Color redC = new Color(156f/255f, 109/255f, 109/255f);

    private const int SlotsPerRow = 6;

    private void Start()
    {   
        instanceCount = StageImporter.scriptCount;
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

            if (i < StageImporter.handCount)
            {
                slot.GetComponent<Image>().color = redC;
            }
        }
    }

}
