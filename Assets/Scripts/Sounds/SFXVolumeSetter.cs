using UnityEngine;

public class SFXVolumeSetter : MonoBehaviour
{
    void Update()
    {
        GetComponent<AudioSource>().volume = VolumeManager.instance.sfxVolume;
    }
}
