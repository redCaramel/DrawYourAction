using System.Collections.Generic;
using UnityEngine;

public struct ScriptData
{
    public List<Action> actions;
    public string name;
    public int maxDuration;
    public int status;

    public ScriptData(string name = "")
    {
        actions = new List<Action>();
        this.name = name;
        maxDuration = 0;
        status = 0;
    }
    public ScriptData(string name = "", int dur = 1, int st= 0)
    {
        actions = new List<Action>();
        this.name = name;
        maxDuration = dur;
        status = st;
    }

    // TODO - more Identifier!
}