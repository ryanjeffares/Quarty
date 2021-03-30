using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class PianoKeyDraggableController : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDragHandler, IDropHandler
{
    public static event Action<string> NotePlayed;
    public static event Action<PianoKeyDraggableController> Dropped;
    
    [SerializeField] private Text text;

    private Color _colour;
    private bool _clickable, _usePersistentColour, _collidable, _anyCoroutinesRunning;
    private Vector3 _targetPosition;

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

    public void Show(float waitTime, Vector3 target, bool clickable = true, bool usePersistentColour = false, bool collidable = false)
    {
        GetComponent<Image>().color = Color.clear;
        text.color = Color.clear;
        _clickable = clickable;
        _usePersistentColour = usePersistentColour;
        _collidable = collidable;
        _targetPosition = target;
        StartCoroutine(FadeIn(waitTime));
    }

    private IEnumerator FadeIn(float waitTime)
    {
        _anyCoroutinesRunning = true;
        yield return new WaitForSeconds(waitTime);
        float timeCounter = 0f;
        while (timeCounter < 0.5f)
        {
            GetComponent<Image>().color = Color.Lerp(Color.clear, _colour, timeCounter / 0.5f);
            text.color = Color.Lerp(Color.clear, Color.grey, timeCounter / 0.5f);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        GetComponent<Image>().color = _colour;
        _anyCoroutinesRunning = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PauseManager.paused) return;
        transform.position = eventData.position;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if((Mathf.Abs(_targetPosition.x - eventData.position.x) <= 10) || (Mathf.Abs(_targetPosition.y - eventData.position.y) <= 10))
        {
            transform.position = _targetPosition;
            Dropped?.Invoke(this);
        }
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
        if (!_collidable) return;
        RuntimeManager.PlayOneShot("event:/PianoNotes/" + note);
        NotePlayed?.Invoke(note);
        StartCoroutine(FadeColour(true));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!_collidable) return;
        StartCoroutine(FadeColour(false));
    }

    public void ManualPlayNote()
    {
        StartCoroutine(ManualPlayNoteAsync());
    }

    private IEnumerator ManualPlayNoteAsync()
    {
        _anyCoroutinesRunning = true;
        StartCoroutine(FadeColour(true));
        RuntimeManager.PlayOneShot($"event:/PianoNotes/{note}");
        NotePlayed?.Invoke(note);
        yield return new WaitForSeconds(1f);
        StartCoroutine(FadeColour(false));
        _anyCoroutinesRunning = false;
    }

    private IEnumerator FadeColour(bool noteOn)
    {
        _anyCoroutinesRunning = true;
        var target = noteOn ? new Color(_colour.r, _colour.g, _colour.b, 0.7f) : _colour;
        var start = GetComponent<Image>().color;
        float timeCounter = 0f;
        while (timeCounter <= 0.1f)
        {
            GetComponent<Image>().color = Color.Lerp(start, target, timeCounter / 0.2f);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        GetComponent<Image>().color = target;
        _anyCoroutinesRunning = false;
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
