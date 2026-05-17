using System.Collections.Generic;
using UnityEngine;

public class ActionRecorder : MonoBehaviour
{
    [SerializeField] private List<MovementType> Movements = new List<MovementType>();
    [SerializeField] private List<JumpType> Jumps= new List<JumpType>();
    [SerializeField] private List<AttackType> Attacks= new List<AttackType>();
    private bool recording = false;
    private float recordTime = 0f;
    private float currentTime = 0f;

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

    public void Update()
    {
        if(recording)
        {
            Debug.Log(currentTime);
            currentTime -= Time.deltaTime;
            if(currentTime <= 0f)
            {
                recording = false;
            }
        }
    }

    public void ApplyAction(MovementType act)
    {
        Movements.Add(act);
        //Debug.Log("move" + act);
    }
    public void ApplyAction(JumpType act)
    {
        Jumps.Add(act);
        //Debug.Log("jump" +act);
    }
    public void ApplyAction(AttackType act)
    {
        Attacks.Add(act);
        //Debug.Log("atk" +act);
    }
    public void StartRecording(int time)
    {
        recording = true;
        currentTime = time;
        recordTime = time;
    }
    public bool isRecording()
    {
        return recording;
    }
}
