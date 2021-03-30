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
    private bool _clickable;
    private string _note;
    public string note
    {
        get => _note;
        set
        {
            _note = value;
            _colour = _note.Contains("#") ? Color.black : Color.white;
            text.text = _note.Substring(0, _note.Length - 1);
        }
    }    

    public void Show(float waitTime, bool clickable = true)
    {
        GetComponent<Image>().color = Color.clear;
        text.color = Color.clear;
        _clickable = clickable;
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
        while (gameObject.activeSelf)
        {
            for (int i = 0; i < 360; i += 2)
            {
                float value = (float)Persistent.sineWaveValues[i];
                text.transform.localScale = new Vector3(1 + (value * 0.3f), 1 + (value * 0.3f));
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
