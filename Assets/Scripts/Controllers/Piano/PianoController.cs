using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PianoController : MonoBehaviour
{
    [SerializeField] private bool debug;
    [SerializeField] private GameObject keyPrefab, content;
    [SerializeField] private Image scrollbar, scrollbarBg;
    
    private string[] _naturals = new string[]
        {
            "C1", "D1", "E1", "F1", "G1", "A1", "B1",
            "C2", "D2", "E2", "F2", "G2", "A2", "B2",
            "C3", "D3", "E3", "F3", "G3", "A3", "B3",
            "C4"
        };
    private string[] _sharps = new string[]
        {
            "C#1", "D#1", "F#1", "G#1", "A#1",
            "C#2", "D#2", "F#2", "G#2", "A#2",
            "C#3", "D#3", "F#3", "G#3", "A#3"            
        };

    private List<GameObject> _keys;

    private void Awake()
    {
        if (!debug) return;
        Show(2, false);
    }

    public void Show(int numOctaves, bool showFlats = true, bool useColours = false, bool clickable = true, bool autoPlayNotes = false, 
        bool useCustomNotes = false, string[] customNaturals = null, string[] customSharps = null, bool showNumbers = false)
    {
        int naturalStartIndex, sharpStartIndex, naturalEndIndex, sharpEndIndex; // these will be inclusive in the loop conditions
        switch (numOctaves)
        {
            case 1:
                naturalStartIndex = 7;
                sharpStartIndex = 5;
                naturalEndIndex = 14;
                sharpEndIndex = 9;
                break;
            case 2:
                naturalStartIndex = 7;
                sharpStartIndex = 5;
                naturalEndIndex = 21;
                sharpEndIndex = 14;
                break;
            case 3:
                naturalStartIndex = 0;
                sharpStartIndex = 0;
                naturalEndIndex = 21;
                sharpEndIndex = 14;
                break;
            default:
                Debug.LogWarning("Invalid number of octaves given to PianoController.Show(), returning.");
                return;
        }
        _keys = new List<GameObject>();
        var pos = new Vector2(30, -150);
        var nats = useCustomNotes ? customNaturals : _naturals;
        float waitTime = 0f;
        for(int i = naturalStartIndex; i <= naturalEndIndex; i++)
        {            
            var note = Instantiate(keyPrefab, content.transform);
            note.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 200);
            note.transform.localPosition = pos;
            _keys.Add(note);
            var controller = note.GetComponent<PianoKeyController>();
            controller.Show(waitTime, clickable: clickable, usePersistentColour: useColours, autoPlayChord: autoPlayNotes, parent: this, showNumbers: showNumbers, number: (i % 7) + 1);
            controller.Note = nats[i];            
            pos.x += 60;
            if(nats[i].Contains("E") || nats[i].Contains("B"))
            {
                waitTime += 0.1f;
            }
            else
            {
                waitTime += 0.2f;
            }
        }
        if (showFlats)
        {
            var sharps = useCustomNotes ? customSharps : _sharps;
            pos = new Vector2(60, -75);
            waitTime = 0.1f;
            for (int i = sharpStartIndex; i <= sharpEndIndex; i++)
            {
                var note = Instantiate(keyPrefab, content.transform);
                note.transform.localPosition = pos;
                _keys.Add(note);
                var controller = note.GetComponent<PianoKeyController>();                
                controller.Show(waitTime, clickable: clickable, usePersistentColour: useColours, autoPlayChord: autoPlayNotes, parent: this);
                controller.Note = sharps[i];
                if (sharps[i].Contains("C") || sharps[i].Contains("F") || sharps[i].Contains("G"))
                {
                    waitTime += 0.2f;
                    pos.x += 60;
                }
                else
                {
                    waitTime += 0.3f;
                    pos.x += 120;
                }
            }            
        }        
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(
            (250 * numOctaves) + 50,
            300);
        StartCoroutine(FadeImages(scrollbar));
        StartCoroutine(FadeImages(scrollbarBg));
    }

    public void ToggleAutoplay(bool state)
    {
        foreach(var k in _keys)
        {
            k.GetComponent<PianoKeyController>().autoChord = state;
        }
    }

    public void SetPlayable(bool state)
    {
        foreach(var k in _keys)
        {
            k.GetComponent<PianoKeyController>().playNoteOnTrigger = state;
        }
    }

    private IEnumerator FadeImages(Image img)
    {
        var start = img.color;
        float timeCounter = 0f;
        while(timeCounter <= 0.5f)
        {
            img.color = Color.Lerp(start, Color.white, timeCounter / 0.5f);
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }

    public void HighlightKeys(string[] notes)
    {
        foreach(var k in _keys.Where(k => notes.Contains(k.GetComponent<PianoKeyController>().Note)))
        {
            k.GetComponent<PianoKeyController>().BeginHighlight();
        }
    }

    public void RemoveKeyHighlights(string[] notes)
    {
        foreach (var k in _keys.Where(k => notes.Contains(k.GetComponent<PianoKeyController>().Note)))
        {
            k.GetComponent<PianoKeyController>().animate = false;
        }
    }

    public void PlayNotesManual(string[] notes)
    {
        foreach (var k in _keys.Where(k => notes.Contains(k.GetComponent<PianoKeyController>().Note)))
        {
            k.GetComponent<PianoKeyController>().ManualPlayNote();
        }
    }

    public void PlayChord(GameObject root)
    {
        if(_keys.IndexOf(root) + 2 < _keys.Count)
        {
            var third = _keys[_keys.IndexOf(root) + 2];
            third.GetComponent<PianoKeyController>().ManualPlayNote(waitForNoteOff: true);
        }
        if (_keys.IndexOf(root) + 4 < _keys.Count)
        {
            var fifth = _keys[_keys.IndexOf(root) + 4];
            fifth.GetComponent<PianoKeyController>().ManualPlayNote(waitForNoteOff: true);
        }                    
    }

    public void ChordOff(GameObject root)
    {
        if (_keys.IndexOf(root) + 2 < _keys.Count)
        {
            var third = _keys[_keys.IndexOf(root) + 2];
            third.GetComponent<PianoKeyController>().ManualNoteOff();
        }
        if (_keys.IndexOf(root) + 4 < _keys.Count)
        {
            var fifth = _keys[_keys.IndexOf(root) + 4];
            fifth.GetComponent<PianoKeyController>().ManualNoteOff();
        }
    }
}
