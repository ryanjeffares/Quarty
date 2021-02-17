using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XylophoneController : MonoBehaviour
{
    [SerializeField] private List<GameObject> keyHolders;    

    private void Awake()
    {        
        float time = 1f;
        foreach(var obj in keyHolders)
        {
            int childCount = obj.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                obj.transform.GetChild(i).GetComponent<XylophoneKeyController>().waitTime = time;                
                time += 0.1f;
            }
        }        
    }
}
