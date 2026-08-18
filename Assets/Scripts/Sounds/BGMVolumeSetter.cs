using UnityEngine;

public class BGMVolumeSetter : MonoBehaviour
{
    void Update()
    {
        GetComponent<AudioSource>().volume = VolumeManager.instance.bgmVolume;
    }
}
