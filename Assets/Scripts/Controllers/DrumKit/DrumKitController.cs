using System;
using System.Linq;
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
    [SerializeField] private List<Text> names;

    private Dictionary<GameObject, string> _drumNames;
    private Dictionary<GameObject, GameObject> _drumImages;
    Dictionary<double, List<GameObject>> backbeatLookup, syncopatedLookup, funkLookup;

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
    }

    public void Show()
    {
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

    public void StopAnimating()
    {
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
