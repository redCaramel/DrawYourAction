using UnityEngine;

public class DebugScriptUser : MonoBehaviour
{
    [SerializeField] ScriptCardManager cardManager;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            cardManager.UseCard(0);
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            cardManager.UseCard(1);
        }
        if(Input.GetKeyDown(KeyCode.D))
        {
            cardManager.UseCard(2);
        }

    }
}
