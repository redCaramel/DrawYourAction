using System.Collections.Generic;
using UnityEngine;

public struct ScriptData
{
    public List<Action> actions;
    public string name;
    public int status;

    public ScriptData(string name = "")
    {
        actions = new List<Action>();
        this.name = name;
        status = 0;
    }

    // TODO - more Identifier!
}