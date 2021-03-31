using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class PianoKeyController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static event Action<string> NotePlayed;

    [SerializeField] private Text text;
    [SerializeField] private AnimationCurve curve;

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

    public void Show(float waitTime, bool clickable = true, bool usePersistentColour = false)
    {
        GetComponent<Image>().color = Color.clear;
        text.color = Color.clear;
        _clickable = clickable;
        _usePersistentColour = usePersistentColour;        
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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_clickable) return;        
        StartCoroutine(FadeColourAndScale(false));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RuntimeManager.PlayOneShot("event:/PianoNotes/" + Note);
        NotePlayed?.Invoke(Note);
        StartCoroutine(FadeColourAndScale(true));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StartCoroutine(FadeColourAndScale(false));
    }

    public void ManualPlayNote()
    {
        StartCoroutine(ManualPlayNoteAsync());
    }

    private IEnumerator ManualPlayNoteAsync()
    {        
        StartCoroutine(FadeColourAndScale(true));
        RuntimeManager.PlayOneShot($"event:/PianoNotes/{Note}");
        NotePlayed?.Invoke(Note);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeColourAndScale(false));     
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
