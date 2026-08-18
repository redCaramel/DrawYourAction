using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int currentStage = 0;
    public float bgmVolume = 1;
    public float sfxVolume = 1;

    // 기본 생성자
    public SaveData()
    {
        
    }
}