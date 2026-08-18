using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct ScriptData
{
    public List<Action> actions;
    public string name;
    public int maxDuration;
    public int status;
    public int color;
    public Sprite thumbnail;

    public ScriptData(string name = "")
    {
        actions = new List<Action>();
        this.name = name;
        maxDuration = 0;
        status = 0;
        color = 8;
        Texture2D clearTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        clearTex.SetPixel(0, 0, Color.clear); // (0, 0, 0, 0)
        clearTex.Apply();
        thumbnail = Sprite.Create(
            clearTex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );
    }
    public ScriptData(string name = "", int dur = 1, int st= 0)
    {
        actions = new List<Action>();
        this.name = name;
        maxDuration = dur;
        status = st;
        color = 8;
        Texture2D clearTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        clearTex.SetPixel(0, 0, Color.clear); // (0, 0, 0, 0)
        clearTex.Apply();
        thumbnail = Sprite.Create(
            clearTex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f)
        );
    }

    // TODO - more Identifier!
}