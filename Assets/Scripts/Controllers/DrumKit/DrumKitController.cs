using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class DrumKitController : BaseManager
{    
    [SerializeField] private GameObject kick, snare, hatClosed, highTom, midTom, crash;
    [SerializeField] private GameObject kickImg, snareImg, hatClosedImg, highTomImg, midTomImg, crashImg;
    [SerializeField] private AnimationCurve easeInOutCurve, overshootCurve;

    private Dictionary<GameObject, string> _drumNames;
    private Dictionary<GameObject, GameObject> _drumImages;    

    protected override void OnAwake()
    {        
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {kick, DrumCallback },
            {snare, DrumCallback },
            {hatClosed, DrumCallback },
            {highTom, DrumCallback },            
            {midTom, DrumCallback },
            {crash, DrumCallback }
        };
        _drumNames = new Dictionary<GameObject, string>
        {
            {kick, "Kick" },
            {snare, "Snare"},
            {hatClosed, "HiHatClosed" },
            {highTom, "HighTom" },            
            {midTom, "MidTom" },            
            {crash, "Crash" }
        };
        _drumImages = new Dictionary<GameObject, GameObject>
        {
            {kick, kickImg },
            {snare, snareImg},
            {hatClosed, hatClosedImg},
            {highTom, highTomImg },
            {midTom, midTomImg },
            {crash, crashImg }
        };
        foreach(var img in _drumImages)
        {
            img.Value.transform.localScale = new Vector3(0, 0);
        }
    }

    public void Show()
    {
        foreach(var img in _drumImages)
        {
            StartCoroutine(FadeInObjectScale(img.Value, overshootCurve, true, 0.5f));
        }
    }

    private void DrumCallback(GameObject g)
    {     
        RuntimeManager.PlayOneShot($"event:/Drums/{_drumNames[g]}");
        StartCoroutine(DrumHit(_drumImages[g]));
    }

    private IEnumerator DrumHit(GameObject target)
    {
        float timeCounter = 0f;
        while(timeCounter <= 0.05f)
        {
            var diff = 0.1f * easeInOutCurve.Evaluate(timeCounter / 0.05f);
            target.transform.localScale = new Vector3(1 - diff, 1 - diff);
            timeCounter += Time.deltaTime;
            yield return null;
        }        
        timeCounter = 0;
        while (timeCounter <= 0.05f)
        {
            var diff = 0.1f * easeInOutCurve.Evaluate(timeCounter / 0.05f);
            target.transform.localScale = new Vector3(0.9f + diff, 0.9f + diff);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        target.transform.localScale = new Vector3(1, 1);
    }
}
