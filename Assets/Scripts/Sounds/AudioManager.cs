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
    private AudioSource loopSfxPlayer; // 이동 중 run 등, 상태가 유지되는 동안 계속 재생/정지되는 루프 SFX 전용 채널

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
        loopSfxPlayer.volume = VolumeManager.instance.sfxVolume;
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

        GameObject loopSfxObject = new GameObject("LoopSFXPlayer");
        loopSfxObject.transform.parent = transform;
        loopSfxPlayer = loopSfxObject.AddComponent<AudioSource>();
        loopSfxPlayer.playOnAwake = false;
        loopSfxPlayer.loop = true;
        loopSfxPlayer.volume = VolumeManager.instance.sfxVolume;
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

    /// <summary>
    /// 상태가 유지되는 동안 반복 재생되어야 하는 SFX(예: run)를 재생한다.
    /// 이미 같은 클립이 재생 중이면 재시작하지 않는다.
    /// </summary>
    public void PlayLoopSFX(SFXType sfx)
    {
        AudioClip clip = SFXLibrary.instance?.GetClip(sfx);
        if (clip == null) return;
        if (loopSfxPlayer.isPlaying && loopSfxPlayer.clip == clip) return;

        loopSfxPlayer.clip = clip;
        loopSfxPlayer.Play();
    }

    public void StopLoopSFX()
    {
        if (loopSfxPlayer.isPlaying) loopSfxPlayer.Stop();
    }
}