using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XylophoneController : BaseManager
{
    [SerializeField] private GameObject keys;
    
    protected override void OnAwake()
    {
        int childCount = keys.transform.childCount;
        float time = 1f;
        for (int i = 0; i < childCount; i++)
        {
            keys.transform.GetChild(i).GetComponent<XylophoneKeyController>().waitTime = time;
            buttonCallbackLookup.Add(keys.transform.GetChild(i).gameObject, KeyPressedCallback);
            time += 0.1f;
        }
    }

    private void KeyPressedCallback(GameObject key)
    {
        key.GetComponent<AudioSource>().Play();
    }
}
