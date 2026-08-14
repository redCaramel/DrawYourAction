using UnityEngine;

public class AudioManager: MonoBehaviour
{
    public static AudioManager instance;
    [Header("#BGM")]
    public AudioClip bgm;
    private AudioSource bgmPlayer;

    [Header("#SFX")]
    public int channel;
    private AudioSource[] sfxPlayers;
    private int channelIndex;

    void Awake()
    {
        instance = this;    
        Init();
    }
    void Start()
    {
        bgmPlayer.Play();
    }
    void Update()
    {
        bgmPlayer.volume = VolumeManager.instance.bgmVolume;
        for (int i = 0;i < sfxPlayers.Length;i++)
        {
            sfxPlayers[i].volume = VolumeManager.instance.sfxVolume;
        }
    }
    void Init()
    {
        GameObject bgmObject = new GameObject("BGMPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = VolumeManager.instance.bgmVolume;
        bgmPlayer.clip = bgm;

        GameObject sfxObject = new GameObject("SFXPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channel];
        for (int i = 0;i < sfxPlayers.Length;i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].volume = VolumeManager.instance.sfxVolume;
        }
    }

    public void PlaySFX(SFXType sfx)
    {
        AudioClip clip = SFXLibrary.instance?.GetClip(sfx);
        if (clip == null) return;

        for (int i = 0;i < sfxPlayers.Length;i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;
            if(sfxPlayers[loopIndex].isPlaying) continue;
            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = clip;
            sfxPlayers[loopIndex].Play();
            return;
        }
        // 재생 가능한 채널이 없으면 현재 채널에 덮어써서 재생한다.
        sfxPlayers[channelIndex].clip = clip;
        sfxPlayers[channelIndex].Play();
    }
}