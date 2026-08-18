using UnityEngine;

public class StatManager : MonoBehaviour
{
    // Add Necessary Stats under here
    [SerializeField] public float playerSpeed = 7;
    [SerializeField] public float playerJumpPower = 15;
    [SerializeField] public float playerGravity = 4;
    [SerializeField] public int playerJumpTime = 2;
    [SerializeField] public int playerAtk = 1;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify under here
    private static StatManager _instance;

    // 씬에 StatManager가 없어도 어디서든 참조할 수 있도록,
    // 처음 접근되는 시점에 없으면 기본값(인스펙터 기본값)으로 자동 생성한다.
    public static StatManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject(nameof(StatManager));
                _instance = go.AddComponent<StatManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        _instance = null;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
