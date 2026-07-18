using UnityEngine;

public class RecordUIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] contents;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static RecordUIManager instance {get; private set;}

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
        ShowContent(0);
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    public void ShowContent(int index)
    {
        for(int i = 0;i < contents.Length;i++)
        {
            contents[i].SetActive(i == index);
        }
    }
}
