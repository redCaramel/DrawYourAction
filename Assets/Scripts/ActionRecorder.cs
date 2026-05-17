using System.Collections.Generic;
using UnityEngine;

public class ActionRecorder : MonoBehaviour
{
    private List<MovementType> Movements = new List<MovementType>();
    private List<JumpType> Jumps= new List<JumpType>();
    private List<AttackType> Attacks= new List<AttackType>();

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify here
    public static ActionRecorder instance {get; private set;}

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
    public void ApplyAction(MovementType act)
    {
        Movements.Add(act);
        Debug.Log("move" + act);
    }
    public void ApplyAction(JumpType act)
    {
        Jumps.Add(act);
        Debug.Log("jump" +act);
    }
    public void ApplyAction(AttackType act)
    {
        Attacks.Add(act);
        Debug.Log("atk" +act);
    }

}
