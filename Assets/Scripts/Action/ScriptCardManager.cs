using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScriptCardManager : MonoBehaviour
{

    [SerializeField] private int handSize = 3;
    private List<ScriptData> masterDeck = new List<ScriptData>();

    private List<ScriptData> drawScript = new List<ScriptData>();
    private List<ScriptData> handScript = new List<ScriptData>();

    void Start()
    {
        initMasterDect(ScriptImporter.scripts);
        InitializeDeck();
    }

    public void initMasterDect(List<ScriptData> scripts)
    {
        for(int i = 0;i < scripts.Count;i++)
        {
            masterDeck.Add(scripts[i]);
            Debug.Log(scripts[i]);
        }
    }

    public void InitializeDeck()
    {
        drawScript.Clear();
        handScript.Clear();

        drawScript.AddRange(masterDeck);

        for (int i = 0; i < handSize; i++)
        {
            if (drawScript.Count > 0)
            {
                ScriptData temp = drawScript[0];
                drawScript.RemoveAt(0);
                handScript.Add(temp);
            }
        }

        UpdateNextCardPreview();
    }

    public void UseCard(ScriptData cardData)
    {
        if (!handScript.Contains(cardData))
        {
            Debug.LogWarning("손에 없는 카드는 사용할 수 없습니다.");
            return;
        }

        

        ActionExecuter.instance.StartLoading(cardData.actions);
        handScript.Remove(cardData);
        Debug.Log($"[카드 사용] {cardData.name}");

        drawScript.Add(cardData);

        if (drawScript.Count > 0)
        {
            ScriptData nextCard = drawScript[0];
            drawScript.RemoveAt(0);
            handScript.Add(nextCard);
            
            Debug.Log($"[카드 보충] 덱 위에서 '{nextCard.name}' 카드가 손패로 들어왔습니다.");
        }

        Debug.Log($"현재 손패: {string.Join(", ", handScript)} | 대기 중인 덱 수: {drawScript.Count}");
        UpdateNextCardPreview();
    }
    public void UseCard(int index)
    {
        if (index >= handSize)
        {
            Debug.LogWarning("인덱스 초과");
            return;
        }
        ScriptData cardData = handScript.ElementAt(index);

        ActionExecuter.instance.StartLoading(cardData.actions);
        handScript.Remove(cardData);
        Debug.Log($"[카드 사용] {cardData.name}");

        drawScript.Add(cardData);

        if (drawScript.Count > 0)
        {
            ScriptData nextCard = drawScript[0];
            drawScript.RemoveAt(0);
            handScript.Add(nextCard);
            
            Debug.Log($"[카드 보충] 덱 위에서 '{nextCard.name}' 카드가 손패로 들어왔습니다.");
        }

        Debug.Log($"현재 손패: {string.Join(", ", handScript)} | 대기 중인 덱 수: {drawScript.Count}");
        UpdateNextCardPreview();
    }
    private void UpdateNextCardPreview()
    {
        if (drawScript.Count > 0)
        {
            Debug.Log($"🔮 다음에 드로우될 카드(Next): {drawScript[0].name}");
        }
    }
}
