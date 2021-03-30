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
    private Color _colour;
    private bool _clickable, _usePersistentColour;
    private string _note;
    public string note
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
    }

    public void OnPointerDown(PointerEventData eventDate)
    {
        if (!_clickable) return;
        StopAllCoroutines();
        StartCoroutine(FadeColour(true));
        RuntimeManager.PlayOneShot($"event:/PianoNotes/{note}");
        NotePlayed?.Invoke(note);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_clickable) return;
        StopAllCoroutines();
        StartCoroutine(FadeColour(false));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RuntimeManager.PlayOneShot("event:/PianoNotes/" + note);
        NotePlayed?.Invoke(note);
        StartCoroutine(FadeColour(true));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StartCoroutine(FadeColour(false));
    }

    public void ManualPlayNote()
    {
        StartCoroutine(ManualPlayNoteAsync());
    }

    private IEnumerator ManualPlayNoteAsync()
    {        
        StartCoroutine(FadeColour(true));
        RuntimeManager.PlayOneShot($"event:/PianoNotes/{note}");
        NotePlayed?.Invoke(note);
        yield return new WaitForSeconds(1f);
        StartCoroutine(FadeColour(false));     
    }

    private IEnumerator FadeColour(bool noteOn)
    {        
        var target = noteOn ? new Color(_colour.r, _colour.g, _colour.b, 0.7f) : _colour;
        var start = GetComponent<Image>().color;
        float timeCounter = 0f;        
        while(timeCounter <= 0.1f)
        {            
            GetComponent<Image>().color = Color.Lerp(start, target, timeCounter / 0.2f);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        GetComponent<Image>().color = target;     
    }

    public IEnumerator AnimateText()
    {
        animate = true;
        while (animate)
        {
            for (int i = 0; i < 360; i += 2)
            {
                float value = (float)Persistent.sineWaveValues[i];
                text.transform.localScale = new Vector3(1 + (value * 0.3f), 1 + (value * 0.3f));
                yield return new WaitForFixedUpdate();
            }
        }
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
