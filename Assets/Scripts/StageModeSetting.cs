using System.Collections.Generic;
using UnityEngine;


public class StageModeSetting : MonoBehaviour
{
    public static bool isPreview = false;

    public static void setMode(bool mode)
    {
        isPreview = mode;
    }
}
