using UnityEngine;

public class StatManager : MonoBehaviour
{
    // Add Necessary Stats under here
    [SerializeField] private float _playerSpeed;
    public float playerSpeed => _playerSpeed;
    [SerializeField] private float _playerJumpPower;
    public float playerJumpPower => _playerJumpPower;
    [SerializeField] private float _playerGravity;
    public float playerGravity => _playerGravity;
    [SerializeField] private int _playerJumpTime;
    public int playerJumpTime => _playerJumpTime;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify under here
    public static StatManager instance {get; private set;}

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
            DontDestroyOnLoad(this);
        }
        else Destroy(gameObject);
       
    }
}
