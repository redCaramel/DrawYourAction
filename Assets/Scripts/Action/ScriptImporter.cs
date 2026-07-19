using System.Collections.Generic;
using UnityEngine;


public class ScriptImporter : MonoBehaviour
{
    List<ScriptData> scripts;
    void Start()
    {
        scripts = ScriptExporter.ExportedScripts;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
