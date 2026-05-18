using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionLoader : MonoBehaviour
{
    [SerializeField] private List<Action> Actions;
    private bool loading = false;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ActionLoader instance {get; private set;}

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
            Debug.Log("yay");
        }
        else Destroy(gameObject);
       
    }
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    // ----------------------------------------------------

    public void Update()
    {
        if(loading)
        {
            if(Actions != null && Actions.Count > 0) {
                PlayerController.instance.ExecuteAction(Actions[0]);
                Actions.RemoveAt(0);
            }
            else loading = false;
        }
    }

    public void StartLoading(List<Action> acts)
    {
        Actions = acts;
        loading = true;
    }
    public bool isLoading()
    {
        return loading;
    }
}
