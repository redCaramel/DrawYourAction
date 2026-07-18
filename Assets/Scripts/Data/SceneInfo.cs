using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SceneInfo
{
    public int sceneNum; 
    public string content;
    public List<string> missonList;
    public List<int> missonType;
}