using UnityEngine;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] public float bgmVolume = 1f;
    [SerializeField] public float sfxVolume = 1f;

    // ----------------------------------------------------
    // Creating and Resetting Instance
    // Don't modify under here
    private static VolumeManager _instance;

    // 씬에 StatManager가 없어도 어디서든 참조할 수 있도록,
    // 처음 접근되는 시점에 없으면 기본값(인스펙터 기본값)으로 자동 생성한다.
    public static VolumeManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject(nameof(VolumeManager));
                _instance = go.AddComponent<VolumeManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public void SetBGMVolume(float vol)
    {
        bgmVolume = vol;
    }
    public void SetSFXVolume(float vol)
    {
        sfxVolume = vol;
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
