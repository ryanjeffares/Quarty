using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class PianoKeyController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static event Action<string> NotePlayed;
    public bool playNoteOnTrigger = true;

    [SerializeField] private Text text, numberText;
    [SerializeField] private AnimationCurve curve;

    private PianoController _parent;
    private Color _colour;
    private bool _clickable, _usePersistentColour, _fadedIn;    
    private string _note;
    public string Note
    {
        get => _note;
        set
        {
            _note = value;
            if (_usePersistentColour)
            {
                _colour = Persistent.noteColours[_note.Substring(0, _note.Length - 1)];
            }
            else
            {
                _colour = _note.Contains("#") ? Color.black : Color.white;
            }            
            text.text = _note.Substring(0, _note.Length - 1);
        }
    }
    
    [HideInInspector]
    public bool animate;
    [HideInInspector]
    public bool autoChord;

    public void Show(float waitTime, bool clickable = true, bool usePersistentColour = false, 
        bool autoPlayChord = false, PianoController parent = null, bool showNumbers = false, int number = 0)
    {
        GetComponent<Image>().color = Color.clear;
        _parent = parent;
        text.color = Color.clear;
        _clickable = clickable;
        _usePersistentColour = usePersistentColour;
        autoChord = autoPlayChord;
        if (showNumbers)
        {
            numberText.text = number.ToString();
        }
        StartCoroutine(FadeIn(waitTime));
    }

    private IEnumerator FadeIn(float waitTime)
    {        
        yield return new WaitForSeconds(waitTime);
        float timeCounter = 0f;
        while(timeCounter < 0.5f)
        {
            GetComponent<Image>().color = Color.Lerp(Color.clear, _colour, timeCounter / 0.5f);
            text.color = Color.Lerp(Color.clear, Color.grey, timeCounter / 0.5f);
            numberText.color = Color.Lerp(Color.clear, Color.grey, timeCounter / 0.5f);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        GetComponent<Image>().color = _colour;
        _fadedIn = true;
    }

    public void OnPointerDown(PointerEventData eventDate)
    {
        if (!_clickable) return;        
        StartCoroutine(FadeColourAndScale(true));
        RuntimeManager.PlayOneShot($"event:/PianoNotes/{Note}");
        NotePlayed?.Invoke(Note);
        if (autoChord)
        {
            if(_parent == null)
            {
                Debug.Log("You tried to autoplay chord but PianoKeyController's parent is null!");
                return;
            }
            _parent.PlayChord(gameObject);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_clickable) return;        
        StartCoroutine(FadeColourAndScale(false));
        if (autoChord)
        {
            if (_parent == null)
            {
                Debug.Log("You tried to autoplay chord but PianoKeyController's parent is null!");
                return;
            }
            _parent.ChordOff(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playNoteOnTrigger)
        {
            RuntimeManager.PlayOneShot("event:/PianoNotes/" + Note);
            NotePlayed?.Invoke(Note);
        }        
        StartCoroutine(FadeColourAndScale(true));
        if (autoChord)
        {
            if (_parent == null)
            {
                Debug.Log("You tried to autoplay chord but PianoKeyController's parent is null!");
                return;
            }
            _parent.PlayChord(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StartCoroutine(FadeColourAndScale(false));
        if (autoChord)
        {
            if (_parent == null)
            {
                Debug.Log("You tried to autoplay chord but PianoKeyController's parent is null!");
                return;
            }
            _parent.ChordOff(gameObject);
        }
    }

    public void ManualPlayNote(bool waitForNoteOff = false)
    {
        StartCoroutine(ManualPlayNoteAsync(waitForNoteOff: waitForNoteOff));
    }

    public void ManualNoteOff()
    {
        StartCoroutine(FadeColourAndScale(false));
    }

    private IEnumerator ManualPlayNoteAsync(bool waitForNoteOff = false)
    {        
        StartCoroutine(FadeColourAndScale(true));
        RuntimeManager.PlayOneShot($"event:/PianoNotes/{Note}");
        NotePlayed?.Invoke(Note);
        if (!waitForNoteOff)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(FadeColourAndScale(false));
        }             
    }

    private IEnumerator FadeColourAndScale(bool noteOn)
    {        
        var target = noteOn ? new Color(_colour.r, _colour.g, _colour.b, 0.7f) : _colour;
        var start = GetComponent<Image>().color;
        float timeCounter = 0f;        
        while(timeCounter <= 0.1f)
        {            
            GetComponent<Image>().color = Color.Lerp(start, target, timeCounter / 0.1f);
            var scaleDiff = 0.1f * curve.Evaluate(timeCounter / 0.1f);
            transform.localScale = noteOn ? new Vector3(1 - scaleDiff, 1 - scaleDiff) : new Vector3(0.9f + scaleDiff, 0.9f + scaleDiff);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        GetComponent<Image>().color = target;     
    }

    public void BeginHighlight()
    {
        animate = true;
        StartCoroutine(HighlightKey());
    }

    private IEnumerator HighlightKey()
    {        
        // this runs until animate is set to false from the PianoController, and sets the colour
        _colour = Persistent.noteColours[_note.Substring(0, _note.Length - 1)];
        if (_fadedIn)
        {
            GetComponent<Image>().color = _colour;
        }        
        while (animate)
        {
            for (int i = 0; i < 360; i += 2)
            {
                if (!animate) break;
                float value = (float)Persistent.sineWaveValues[i];
                text.transform.localScale = new Vector3(1 + (value * 0.3f), 1 + (value * 0.3f));
                yield return null;
            }
        }
        // set the text back to normal scale and reset colour
        text.transform.localScale = new Vector3(1, 1);
        _colour = _note.Contains("#") ? Color.black : Color.white;
        GetComponent<Image>().color = _colour;
    }

    public IEnumerator Dispose()
    {
        float timeCounter = 0f;
        while (timeCounter <= 0.5f)
        {
            GetComponent<Image>().color = Color.Lerp(_colour, Color.clear, timeCounter / 0.5f);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
