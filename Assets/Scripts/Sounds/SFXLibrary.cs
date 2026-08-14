using System;
using System.Collections.Generic;
using UnityEngine;

// SFX(효과음)를 AudioManager와 분리하여 보관하는 저장용 클래스입니다.
// Resources 폴더에 이 타입의 에셋을 만들어두면(Create > Audio > SFX Library),
// 어느 씬에서든 SFXLibrary.instance 로 접근해 AudioClip을 가져올 수 있습니다.
[CreateAssetMenu(fileName = "SFXLibrary", menuName = "Audio/SFX Library")]
public class SFXLibrary : ScriptableObject
{
    [Serializable]
    public class SFXEntry
    {
        public SFXType type;
        public AudioClip clip;
    }

    [SerializeField] private List<SFXEntry> sfxList = new List<SFXEntry>();

    private Dictionary<SFXType, AudioClip> sfxDict;

    // ----------------------------------------------------
    // Creating Instance
    // Don't modify under here
    private const string ResourcePath = "SFXLibrary";
    private static SFXLibrary _instance;

    // Resources/SFXLibrary.asset을 어디서든 로드해서 쓸 수 있도록 지연 생성한다.
    public static SFXLibrary instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SFXLibrary>(ResourcePath);
                if (_instance == null)
                {
                    Debug.LogError($"SFXLibrary를 찾을 수 없습니다. Resources 폴더에 '{ResourcePath}.asset'을 생성해주세요. " +
                                   $"(Project 창에서 Create > Audio > SFX Library)");
                }
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        _instance = null;
    }

    private void OnEnable()
    {
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        sfxDict = new Dictionary<SFXType, AudioClip>();
        foreach (SFXEntry entry in sfxList)
        {
            if (entry.clip == null) continue;
            if (sfxDict.ContainsKey(entry.type))
            {
                Debug.LogWarning($"SFXLibrary에 {entry.type} 항목이 중복 등록되어 있습니다.");
                continue;
            }
            sfxDict.Add(entry.type, entry.clip);
        }
    }

    public AudioClip GetClip(SFXType type)
    {
        if (sfxDict == null)
        {
            BuildDictionary();
        }
        if (sfxDict.TryGetValue(type, out AudioClip clip))
        {
            return clip;
        }
        Debug.LogWarning($"SFXLibrary에 {type} 클립이 등록되어 있지 않습니다.");
        return null;
    }
}
