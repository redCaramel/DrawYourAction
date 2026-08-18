using System.Collections.Generic;
using UnityEngine;


public class ScriptImporter : MonoBehaviour
{
    public static List<ScriptData> scripts { get; private set; }
    void Awake()
    {
        
        scripts = ScriptExporter.ExportedScripts;
        Debug.Log(scripts.Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
