using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public enum DrumPattern
{
    KickQuarters,
    KickEights,
    SnareEights,
    SnareSixteenths,
    HatsEights,
    HatsSixteenths,
    TomsEights,
    TomsSixteenths
}

public class DrumKitController : BaseManager
{    
    [SerializeField] private GameObject kick, snare, hatClosed, highTom, midTom, crash;
    [SerializeField] private GameObject kickImg, snareImg, hatClosedImg, highTomImg, midTomImg, crashImg;
    [SerializeField] private AnimationCurve easeInOutCurve, overshootCurve;
    [SerializeField] private List<Text> names;

    private Dictionary<GameObject, string> _drumNames;
    private Dictionary<GameObject, GameObject> _drumImages;
    Dictionary<double, List<GameObject>> backbeatLookup, simpleBackbeatLookup, compoundBackbeatLookup, syncopatedLookup, funkLookup, 
        kickQuarterNotes, kickEighthNotes, hatsEighthNotes, hatsSixteenthNotes, snareEighthNotes, snareSixteenthNotes, tomsEighthNotes, tomsSixteenthNotes;

    private bool _clickable;

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
        canTextLerp = new Dictionary<Text, bool>();
        foreach(var t in names)
        {
            canTextLerp.Add(t, true);
        }
        backbeatLookup = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{kickImg, hatClosedImg} },  //1
            {0.333, new List<GameObject>{hatClosedImg} },
            {0.666, new List<GameObject>{snareImg, hatClosed} },
            {1, new List<GameObject>{hatClosedImg} },

            {1.333, new List<GameObject>{kickImg, hatClosedImg} },
            {1.666, new List<GameObject>{kickImg, hatClosedImg} },
            {2, new List<GameObject>{snareImg, hatClosed} },
            {2.333, new List<GameObject>{hatClosedImg} },

            {2.666, new List<GameObject>{kickImg, hatClosedImg} },  //2
            {3, new List<GameObject>{hatClosedImg} },
            {3.333, new List<GameObject>{snareImg, hatClosed} },
            {3.666, new List<GameObject>{hatClosedImg} },

            {4, new List<GameObject>{kickImg, hatClosedImg} },
            {4.333, new List<GameObject>{kickImg, hatClosedImg} },
            {4.666, new List<GameObject>{snareImg, hatClosed} },
            {5, new List<GameObject>{hatClosedImg} },

            {5.333, new List<GameObject>{kickImg, hatClosedImg} },  //3
            {5.666, new List<GameObject>{hatClosedImg} },
            {6, new List<GameObject>{snareImg, hatClosed} },
            {6.333, new List<GameObject>{hatClosedImg} },

            {6.666, new List<GameObject>{kickImg, hatClosedImg} },
            {7, new List<GameObject>{kickImg, hatClosedImg} },
            {7.333, new List<GameObject>{snareImg, hatClosed} },
            {7.666, new List<GameObject>{hatClosedImg} },

            {8, new List<GameObject>{kickImg, hatClosedImg} },  //4
            {8.333, new List<GameObject>{hatClosedImg} },
            {8.666, new List<GameObject>{snareImg, hatClosed} },
            {9, new List<GameObject>{hatClosedImg} },

            {9.333, new List<GameObject>{kickImg, hatClosedImg, snareImg} },
            {9.666, new List<GameObject>{kickImg, hatClosedImg} },
            {10, new List<GameObject>{snareImg} },
            {10.166, new List<GameObject>{snareImg} },

            {10.333, new List<GameObject>{snareImg} },
            {10.5, new List<GameObject>{snareImg} },
            {10.666, new List<GameObject>{crashImg} }
        };
        simpleBackbeatLookup = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{kickImg, hatClosedImg} },  //1
            {0.333, new List<GameObject>{hatClosedImg} },
            {0.666, new List<GameObject>{snareImg, hatClosed} },
            {1, new List<GameObject>{hatClosedImg} },

            {1.333, new List<GameObject>{kickImg, hatClosedImg} },
            {1.666, new List<GameObject>{hatClosedImg} },
            {2, new List<GameObject>{snareImg, hatClosed} },
            {2.333, new List<GameObject>{hatClosedImg} },

            {2.666, new List<GameObject>{kickImg, hatClosedImg} },  //2
            {3, new List<GameObject>{hatClosedImg} },
            {3.333, new List<GameObject>{snareImg, hatClosed} },
            {3.666, new List<GameObject>{hatClosedImg} },

            {4, new List<GameObject>{kickImg, hatClosedImg} },
            {4.333, new List<GameObject>{hatClosedImg} },
            {4.666, new List<GameObject>{snareImg, hatClosed} },
            {5, new List<GameObject>{hatClosedImg} }
        };
        compoundBackbeatLookup = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{kickImg, hatClosedImg} },
            {0.333f, new List<GameObject>{hatClosedImg} },
            {0.666f, new List<GameObject>{hatClosedImg} },
            {1, new List<GameObject>{snareImg, hatClosedImg} },
            {1.333f, new List<GameObject>{hatClosedImg} },
            {1.666f, new List<GameObject>{hatClosedImg} },

            {2, new List<GameObject>{kickImg, hatClosedImg} },
            {2.333f, new List<GameObject>{hatClosedImg} },
            {2.666f, new List<GameObject>{hatClosedImg} },
            {3, new List<GameObject>{snareImg, hatClosedImg} },
            {3.333f, new List<GameObject>{hatClosedImg} },
            {3.666f, new List<GameObject>{hatClosedImg} },
        };
        syncopatedLookup = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{hatClosedImg, kickImg} },   //1
            {0.166, new List<GameObject>{hatClosedImg} },
            {0.333, new List<GameObject>{hatClosedImg} },
            {0.5, new List<GameObject>{hatClosedImg, kickImg} },

            {0.666, new List<GameObject>{hatClosedImg, snareImg} },
            {0.833, new List<GameObject>{hatClosedImg} },
            {1, new List<GameObject>{hatClosedImg, kickImg } },
            {1.166, new List<GameObject>{hatClosedImg} },

            {1.333, new List<GameObject>{hatClosedImg, kickImg } },
            {1.5, new List<GameObject>{hatClosedImg} },
            {1.666, new List<GameObject>{hatClosedImg, snareImg } },
            {1.833, new List<GameObject>{hatClosedImg, kickImg } },

            {2, new List<GameObject>{hatClosedImg, snareImg} },
            {2.166, new List<GameObject>{hatClosedImg, kickImg } },
            {2.333, new List<GameObject>{hatClosedImg} },
            {2.5, new List<GameObject>{hatClosedImg} },

            {2.666, new List<GameObject>{hatClosedImg, kickImg } },   //2
            {2.833, new List<GameObject>{hatClosedImg} },
            {3, new List<GameObject>{hatClosedImg} },
            {3.166, new List<GameObject>{hatClosedImg, kickImg } },

            {3.333, new List<GameObject>{hatClosedImg, snareImg } },
            {3.5, new List<GameObject>{hatClosedImg} },
            {3.666, new List<GameObject>{hatClosedImg, kickImg } },
            {3.833, new List<GameObject>{hatClosedImg} },

            {4, new List<GameObject>{hatClosedImg, kickImg } },  
            {4.166, new List<GameObject>{hatClosedImg} },
            {4.333, new List<GameObject>{hatClosedImg, snareImg } },
            {4.5, new List<GameObject>{hatClosedImg, kickImg } },

            {4.666, new List<GameObject>{hatClosedImg, snareImg } },
            {4.833, new List<GameObject>{hatClosedImg, kickImg } },
            {5, new List<GameObject>{hatClosedImg } },
            {5.166, new List<GameObject>{hatClosedImg} },

            {5.333, new List<GameObject>{hatClosedImg, kickImg } },   //3
            {5.5, new List<GameObject>{hatClosedImg} },
            {5.666, new List<GameObject>{hatClosedImg} },
            {5.833, new List<GameObject>{hatClosedImg, kickImg } },

            {6, new List<GameObject>{hatClosedImg, snareImg } },
            {6.166, new List<GameObject>{hatClosedImg} },
            {6.333, new List<GameObject>{hatClosedImg, kickImg } },
            {6.5, new List<GameObject>{hatClosedImg} },

            {6.666, new List<GameObject>{hatClosedImg, kickImg } },
            {6.833, new List<GameObject>{hatClosedImg} },
            {7, new List<GameObject>{hatClosedImg, snareImg } },
            {7.166, new List<GameObject>{hatClosedImg, kickImg } },

            {7.333, new List<GameObject>{hatClosedImg, snareImg } },
            {7.5, new List<GameObject>{hatClosedImg, kickImg } },
            {7.666, new List<GameObject>{hatClosedImg } },
            {7.833, new List<GameObject>{hatClosedImg} },

            {8, new List<GameObject>{hatClosedImg, kickImg } },   //4
            {8.166, new List<GameObject>{hatClosedImg} },
            {8.333, new List<GameObject>{hatClosedImg} },
            {8.5, new List<GameObject>{hatClosedImg, kickImg } },

            {8.666, new List<GameObject>{hatClosedImg, snareImg } },
            {8.833, new List<GameObject>{hatClosedImg } },
            {9, new List<GameObject>{hatClosedImg, kickImg } },
            {9.166, new List<GameObject>{hatClosedImg} },

            {9.333, new List<GameObject>{hatClosedImg, kickImg } },
            {9.5, new List<GameObject>{hatClosedImg} },
            {9.666, new List<GameObject>{hatClosedImg, snareImg } },
            {9.833, new List<GameObject>{hatClosedImg, kickImg } },

            {10, new List<GameObject>{hatClosedImg, snareImg } },
            {10.166, new List<GameObject>{hatClosedImg, kickImg, snareImg } },
            {10.333, new List<GameObject>{hatClosedImg, snareImg } },
            {10.5, new List<GameObject>{hatClosedImg, snareImg} },

            {10.66, new List<GameObject>{crashImg} },
        };
        funkLookup = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{ kickImg } },
            {0.25, new List<GameObject>{hatClosedImg } },
            {0.5, new List<GameObject>{ kickImg, snareImg } },
            {0.75, new List<GameObject>{ hatClosedImg } },

            {1, new List<GameObject>{ kickImg } },
            {1.25, new List<GameObject>{ hatClosedImg } },
            {1.5, new List<GameObject>{ kickImg, snareImg } },
            {1.75, new List<GameObject>{ hatClosedImg } },

            {2, new List<GameObject>{ kickImg } },
            {2.25, new List<GameObject>{ hatClosedImg } },
            {2.5, new List<GameObject>{ kickImg, snareImg } },
            {2.75, new List<GameObject>{ hatClosedImg } },

            {3, new List<GameObject>{ kickImg } },
            {3.25, new List<GameObject>{ hatClosedImg } },
            {3.5, new List<GameObject>{ kickImg, snareImg } },
            {3.625, new List<GameObject>{ snareImg } },
            {3.75, new List<GameObject>{ highTomImg } },
            {3.875, new List<GameObject>{ midTomImg } },

            {4, new List<GameObject>{ kickImg } },
            {4.25, new List<GameObject>{ hatClosedImg } },
            {4.5, new List<GameObject>{ kickImg, snareImg } },
            {4.75, new List<GameObject>{ hatClosedImg } },

            {5, new List<GameObject>{ kickImg } },
            {5.25, new List<GameObject>{ hatClosedImg } },
            {5.5, new List<GameObject>{ kickImg, snareImg } },
            {5.75, new List<GameObject>{ hatClosedImg } },

            {6, new List<GameObject>{ kickImg } },
            {6.25, new List<GameObject>{ hatClosedImg } },
            {6.5, new List<GameObject>{ kickImg, snareImg } },
            {6.75, new List<GameObject>{ hatClosedImg } },

            {7, new List<GameObject>{ kickImg, snareImg } },
            {7.125, new List<GameObject>{ snareImg } },
            {7.25, new List<GameObject>{ snareImg } },
            {7.375, new List<GameObject>{ snareImg } },
            {7.5, new List<GameObject>{ kickImg, highTomImg } },
            {7.625, new List<GameObject>{ highTomImg } },
            {7.75, new List<GameObject>{ midTomImg } },
            {7.875, new List<GameObject>{ midTomImg } },

            {8, new List<GameObject>{ crashImg } },

        };
        kickQuarterNotes = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{kickImg} },
            {0.666f, new List<GameObject>{kickImg} },
            {1.333f, new List<GameObject>{kickImg} },
            {2, new List<GameObject>{kickImg} },
            {2.666f, new List<GameObject>{kickImg} },
            {3.333f, new List<GameObject>{kickImg} },
            {4, new List<GameObject>{kickImg} },
            {4.666f, new List<GameObject>{kickImg} }
        };
        kickEighthNotes = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{kickImg} },
            {0.333f, new List<GameObject>{kickImg} },
            {0.666f, new List<GameObject>{kickImg} },
            {1, new List<GameObject>{kickImg} },
            {1.333f, new List<GameObject>{kickImg} },
            {1.666f, new List<GameObject>{kickImg} },
            {2, new List<GameObject>{kickImg} },
            {2.333f, new List<GameObject>{kickImg} },
            {2.666f, new List<GameObject>{kickImg} },
            {3, new List<GameObject>{kickImg} },
            {3.333f, new List<GameObject>{kickImg} },
            {3.666f, new List<GameObject>{kickImg} },
            {4, new List<GameObject>{kickImg} },
            {4.333f, new List<GameObject>{kickImg} },
            {4.666f, new List<GameObject>{kickImg} },
            {5, new List<GameObject>{kickImg} }
        };
        hatsEighthNotes = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{hatClosedImg} },
            {0.333f, new List<GameObject>{hatClosedImg} },
            {0.666f, new List<GameObject>{hatClosedImg} },
            {1, new List<GameObject>{hatClosedImg} },
            {1.333f, new List<GameObject>{hatClosedImg} },
            {1.666f, new List<GameObject>{hatClosedImg} },
            {2, new List<GameObject>{hatClosedImg} },
            {2.333f, new List<GameObject>{hatClosedImg} },

            {2.666f, new List<GameObject>{hatClosedImg} },
            {3, new List<GameObject>{hatClosedImg} },
            {3.333f, new List<GameObject>{hatClosedImg} },
            {3.666f, new List<GameObject>{hatClosedImg} },
            {4, new List<GameObject>{hatClosedImg} },
            {4.333f, new List<GameObject>{hatClosedImg} },
            {4.666f, new List<GameObject>{hatClosedImg} },
            {5, new List<GameObject>{hatClosedImg} }
        };
        hatsSixteenthNotes = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{hatClosedImg} },
            {0.166f, new List<GameObject>{hatClosedImg} },
            {0.333f, new List<GameObject>{hatClosedImg} },
            {0.5f, new List<GameObject>{hatClosedImg} },
            {0.666f, new List<GameObject>{hatClosedImg} },
            {0.833f, new List<GameObject>{hatClosedImg} },
            {1, new List<GameObject>{hatClosedImg} },
            {1.166, new List<GameObject>{hatClosedImg} },
            {1.333f, new List<GameObject>{hatClosedImg} },
            {1.5f, new List<GameObject>{hatClosedImg} },
            {1.666f, new List<GameObject>{hatClosedImg} },
            {1.833f, new List<GameObject>{hatClosedImg} },
            {2, new List<GameObject>{hatClosedImg} },
            {2.166f, new List<GameObject>{hatClosedImg} },
            {2.333f, new List<GameObject>{hatClosedImg} },
            {2.5f, new List<GameObject>{hatClosedImg} },

            {2.666f, new List<GameObject>{hatClosedImg} },
            {2.833f, new List<GameObject>{hatClosedImg} },
            {3, new List<GameObject>{hatClosedImg} },
            {3.166f, new List<GameObject>{hatClosedImg} },
            {3.333f, new List<GameObject>{hatClosedImg} },
            {3.5f, new List<GameObject>{hatClosedImg} },
            {3.666f, new List<GameObject>{hatClosedImg} },
            {3.833f, new List<GameObject>{hatClosedImg} },
            {4, new List<GameObject>{hatClosedImg} },
            {4.166f, new List<GameObject>{hatClosedImg} },
            {4.333f, new List<GameObject>{hatClosedImg} },
            {4.5f, new List<GameObject>{hatClosedImg} },
            {4.666f, new List<GameObject>{hatClosedImg} },
            {4.833f, new List<GameObject>{hatClosedImg} },
            {5, new List<GameObject>{hatClosedImg} },
            {5.166f, new List<GameObject>{hatClosedImg} }
        };
        snareEighthNotes = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{snareImg} },
            {0.333f, new List<GameObject>{snareImg} },
            {0.666f, new List<GameObject>{snareImg} },
            {1, new List<GameObject>{snareImg} },
            {1.333f, new List<GameObject>{snareImg} },
            {1.666f, new List<GameObject>{snareImg} },
            {2, new List<GameObject>{snareImg} },
            {2.333f, new List<GameObject>{snareImg} },

            {2.666f, new List<GameObject>{snareImg} },
            {3, new List<GameObject>{snareImg} },
            {3.333f, new List<GameObject>{snareImg} },
            {3.666f, new List<GameObject>{snareImg} },
            {4, new List<GameObject>{snareImg} },
            {4.333f, new List<GameObject>{snareImg} },
            {4.666f, new List<GameObject>{snareImg} },
            {5, new List<GameObject>{snareImg} }
        };
        snareSixteenthNotes = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{snareImg} },
            {0.166f, new List<GameObject>{snareImg} },
            {0.333f, new List<GameObject>{snareImg} },
            {0.5f, new List<GameObject>{snareImg} },
            {0.666f, new List<GameObject>{snareImg} },
            {0.833f, new List<GameObject>{snareImg} },
            {1, new List<GameObject>{snareImg} },
            {1.166, new List<GameObject>{snareImg} },
            {1.333f, new List<GameObject>{snareImg} },
            {1.5f, new List<GameObject>{snareImg} },
            {1.666f, new List<GameObject>{snareImg} },
            {1.833f, new List<GameObject>{snareImg} },
            {2, new List<GameObject>{snareImg} },
            {2.166f, new List<GameObject>{snareImg} },
            {2.333f, new List<GameObject>{snareImg} },
            {2.5f, new List<GameObject>{snareImg} },

            {2.666f, new List<GameObject>{snareImg} },
            {2.833f, new List<GameObject>{snareImg} },
            {3, new List<GameObject>{snareImg} },
            {3.166f, new List<GameObject>{snareImg} },
            {3.333f, new List<GameObject>{snareImg} },
            {3.5f, new List<GameObject>{snareImg} },
            {3.666f, new List<GameObject>{snareImg} },
            {3.833f, new List<GameObject>{snareImg} },
            {4, new List<GameObject>{snareImg} },
            {4.166f, new List<GameObject>{snareImg} },
            {4.333f, new List<GameObject>{snareImg} },
            {4.5f, new List<GameObject>{snareImg} },
            {4.666f, new List<GameObject>{snareImg} },
            {4.833f, new List<GameObject>{snareImg} },
            {5, new List<GameObject>{snareImg} },
            {5.166f, new List<GameObject>{snareImg} }
        };
        tomsEighthNotes = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{highTomImg} },
            {0.333f, new List<GameObject>{midTomImg} },
            {0.666f, new List<GameObject>{highTomImg} },
            {1, new List<GameObject>{midTomImg} },
            {1.333f, new List<GameObject>{highTomImg} },
            {1.666f, new List<GameObject>{midTomImg} },
            {2, new List<GameObject>{highTomImg} },
            {2.333f, new List<GameObject>{midTomImg} },

            {2.666f, new List<GameObject>{highTomImg} },
            {3, new List<GameObject>{midTomImg} },
            {3.333f, new List<GameObject>{highTomImg} },
            {3.666f, new List<GameObject>{midTomImg} },
            {4, new List<GameObject>{highTomImg} },
            {4.333f, new List<GameObject>{midTomImg} },
            {4.666f, new List<GameObject>{highTomImg} },
            {5, new List<GameObject>{midTomImg} }
        };
        tomsSixteenthNotes = new Dictionary<double, List<GameObject>>
        {
            {0, new List<GameObject>{highTomImg} },
            {0.166f, new List<GameObject>{midTomImg} },
            {0.333f, new List<GameObject>{highTomImg} },
            {0.5f, new List<GameObject>{midTomImg} },
            {0.666f, new List<GameObject>{highTomImg} },
            {0.833f, new List<GameObject>{midTomImg} },
            {1, new List<GameObject>{highTomImg} },
            {1.166, new List<GameObject>{midTomImg} },
            {1.333f, new List<GameObject>{highTomImg} },
            {1.5f, new List<GameObject>{midTomImg} },
            {1.666f, new List<GameObject>{highTomImg} },
            {1.833f, new List<GameObject>{midTomImg} },
            {2, new List<GameObject>{highTomImg} },
            {2.166f, new List<GameObject>{midTomImg} },
            {2.333f, new List<GameObject>{highTomImg} },
            {2.5f, new List<GameObject>{midTomImg} },

            {2.666f, new List<GameObject>{highTomImg} },
            {2.833f, new List<GameObject>{midTomImg} },
            {3, new List<GameObject>{highTomImg} },
            {3.166f, new List<GameObject>{midTomImg} },
            {3.333f, new List<GameObject>{highTomImg} },
            {3.5f, new List<GameObject>{midTomImg} },
            {3.666f, new List<GameObject>{highTomImg} },
            {3.833f, new List<GameObject>{midTomImg} },
            {4, new List<GameObject>{highTomImg} },
            {4.166f, new List<GameObject>{midTomImg} },
            {4.333f, new List<GameObject>{highTomImg} },
            {4.5f, new List<GameObject>{midTomImg} },
            {4.666f, new List<GameObject>{highTomImg} },
            {4.833f, new List<GameObject>{midTomImg} },
            {5, new List<GameObject>{highTomImg} },
            {5.166f, new List<GameObject>{midTomImg} }
        };
    }

    public void Show(bool clickable = true)
    {
        _clickable = clickable;
        foreach(var img in _drumImages)
        {
            StartCoroutine(FadeInObjectScale(img.Value, overshootCurve, true, 0.5f));
        }
    }

    private bool _namesOn;

    public void ShowNames()
    {
        _namesOn = !_namesOn;
        foreach(var t in names)
        {
            StartCoroutine(FadeText(t, _namesOn, 0.5f));
        }
    }

    public void StopAnimating(bool stopAudio = false)
    {
        if (stopAudio)
        {
            var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
            bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        StopAllCoroutines();
    }

    public void PlayPattern(int idx)
    {
        switch (idx)
        {
            case 0: // backbeat 90                
                StartCoroutine(AnimatePattern(backbeatLookup));                                
                break;
            case 1: // syncopated 90
                StartCoroutine(AnimatePattern(syncopatedLookup));
                break;
            case 2: // funk 120
                StartCoroutine(AnimatePattern(funkLookup));
                break;
            case 3:
                StartCoroutine(AnimatePattern(simpleBackbeatLookup));
                break;
            case 4:
                StartCoroutine(AnimatePattern(compoundBackbeatLookup));
                break;
            case 5:
                StartCoroutine(AnimatePattern(kickQuarterNotes));
                break;
            case 6:
                StartCoroutine(AnimatePattern(hatsEighthNotes));
                break;
            case 7:
                StartCoroutine(AnimatePattern(hatsSixteenthNotes));
                break;
        }
    }

    public void PlayPattern(DrumPattern pattern)
    {
        switch (pattern)
        {
            case DrumPattern.KickQuarters:
                RuntimeManager.PlayOneShot("event:/Drums/KickLoop");
                StartCoroutine(AnimatePattern(kickQuarterNotes));
                break;
            case DrumPattern.KickEights:
                StartCoroutine(AnimatePattern(kickEighthNotes));
                RuntimeManager.PlayOneShot("event:/Drums/KickEights");
                break;
            case DrumPattern.SnareEights:
                StartCoroutine(AnimatePattern(snareEighthNotes));
                RuntimeManager.PlayOneShot("event:/Drums/SnareEighths");
                break;
            case DrumPattern.SnareSixteenths:
                StartCoroutine(AnimatePattern(snareSixteenthNotes));
                RuntimeManager.PlayOneShot("event:/Drums/SnareSixteenths");
                break;
            case DrumPattern.TomsEights:
                StartCoroutine(AnimatePattern(tomsEighthNotes));
                RuntimeManager.PlayOneShot("event:/Drums/TomsEighths");
                break;
            case DrumPattern.TomsSixteenths:
                StartCoroutine(AnimatePattern(tomsSixteenthNotes));
                RuntimeManager.PlayOneShot("event:/Drums/TomsSixteenths");
                break;
            case DrumPattern.HatsEights:
                StartCoroutine(AnimatePattern(hatsEighthNotes));
                RuntimeManager.PlayOneShot("event:/Drums/HatsLoop");
                break;
            case DrumPattern.HatsSixteenths:
                StartCoroutine(AnimatePattern(hatsSixteenthNotes));
                RuntimeManager.PlayOneShot("event:/Drums/HatsLoopSixteenths");
                break;
        }
    }

    private void DrumCallback(GameObject g)
    {
        if (!_clickable) return;
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

    private IEnumerator AnimatePattern(Dictionary<double, List<GameObject>> lookup)
    {
        yield return new WaitForSecondsRealtime(0.1f);  // fmod latency - this could be inconsistent on phones :/
        var times = lookup.Keys.ToList();
        for(int i = 0; i < times.Count; i++)
        {
            foreach(var g in lookup[times[i]])
            {
                StartCoroutine(DrumHit(g));
            }
            if(i < times.Count - 1)
            {
                yield return new WaitForSecondsRealtime((float)(times[i + 1] - times[i]));
            }            
        }
    }
}
