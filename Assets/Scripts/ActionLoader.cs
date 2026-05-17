using System.Collections.Generic;
using UnityEngine;

public class ActionLoader : MonoBehaviour
{
    [SerializeField] private List<MovementType> Movements;
    [SerializeField] private List<JumpType> Jumps;
    [SerializeField] private List<AttackType> Attacks;
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
            if(Movements.Count > 0 && Jumps.Count > 0 && Attacks.Count > 0) {
                PlayerController.instance.ExecuteAction(Movements[0], Jumps[0], Attacks[0]);
                Movements.RemoveAt(0);
                Jumps.RemoveAt(0);
                Attacks.RemoveAt(0);
            }
            else loading = false;
            
        }
    }

    public void StartLoading(List<MovementType> a, List<JumpType> b, List<AttackType> c)
    {
        Movements = a;
        Jumps = b;
        Attacks = c;
        loading = true;
    }
    public bool isLoading()
    {
        return loading;
    }
}
