using System.Collections.Generic;
using UnityEngine;

public struct ScriptData
{
    public List<Action> actions;
    public string name;

    public ScriptData(string name = "")
    {
        actions = new List<Action>();
        this.name = name;
    }

    // TODO - more Identifier!
}