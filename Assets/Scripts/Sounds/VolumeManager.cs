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
        if (Mathf.Approximately(bgmVolume, vol)) return;
        bgmVolume = vol;
        AutoSave();
    }
    public void SetSFXVolume(float vol)
    {
        if (Mathf.Approximately(sfxVolume, vol)) return;
        sfxVolume = vol;
        AutoSave();
    }

    // 세이브 파일을 불러와 볼륨 값만 반영할 때 사용 (자동 저장을 유발하지 않음)
    public void LoadVolume(float bgm, float sfx)
    {
        bgmVolume = bgm;
        sfxVolume = sfx;
    }

    private void AutoSave()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.SaveCurrentState();
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
