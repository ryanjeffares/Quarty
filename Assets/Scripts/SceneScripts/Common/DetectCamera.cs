using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCamera : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        GameObject.Find("Main Camera").GetComponent<Wilberforce.Colorblind>().Type = (int)Persistent.settings.valueSettings["ColourblindMode"];
    }
}
