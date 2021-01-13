using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XylophoneController : BaseManager
{
    [SerializeField] private List<GameObject> keyHolders;
    
    protected override void OnAwake()
    {        
        float time = 1f;
        foreach(var obj in keyHolders)
        {
            int childCount = obj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                obj.transform.GetChild(i).GetComponent<XylophoneKeyController>().waitTime = time;
                buttonCallbackLookup.Add(obj.transform.GetChild(i).gameObject, KeyPressedCallback);
                time += 0.1f;
            }
        }        
    }

    private void KeyPressedCallback(GameObject key)
    {
        key.GetComponent<AudioSource>().Play();
    }
}
