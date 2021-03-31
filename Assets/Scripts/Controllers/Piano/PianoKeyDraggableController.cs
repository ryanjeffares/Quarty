using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class PianoKeyDraggableController : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDragHandler
{
    public static event Action<string> NotePlayed;
    public static event Action<PianoKeyDraggableController, bool> Dropped;
    
    [SerializeField] private Text text;
    [SerializeField] private AnimationCurve curve;

    private Color _colour;
    private bool _clickable, _usePersistentColour, _collidable, _onTarget;
    private Vector3 _targetPosition;

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

    private void Awake()
    {
        KeyOutlineCollider.KeyInsideBounds += (state) => _onTarget = state;
    }

    private void OnDestroy()
    {
        KeyOutlineCollider.KeyInsideBounds -= (state) => _onTarget = state;
    }

    public void Show(float waitTime, Vector3 target, bool clickable = true, bool usePersistentColour = false, bool collidable = false)
    {
        GetComponent<Image>().color = Color.clear;
        text.color = Color.clear;
        _clickable = clickable;
        _usePersistentColour = usePersistentColour;
        _collidable = collidable;
        _targetPosition = target;
        Debug.Log(_targetPosition);
        StartCoroutine(FadeIn(waitTime));
    }

    private IEnumerator FadeIn(float waitTime)
    {    
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PauseManager.paused) return;
        transform.position = eventData.position;
    }

    public void OnPointerDown(PointerEventData eventDate)
    {                
        StopAllCoroutines();
        StartCoroutine(FadeColourAndScale(true));
        if (_clickable)
        {
            RuntimeManager.PlayOneShot($"event:/PianoNotes/{Note}");
            NotePlayed?.Invoke(Note);
        }        
    }

    public void OnPointerUp(PointerEventData eventData)
    {        
        if (_onTarget)
        {
            transform.localPosition = _targetPosition;
            Dropped?.Invoke(this, true);
        }
        else
        {
            Dropped?.Invoke(this, false);
        }        
        StopAllCoroutines();
        StartCoroutine(FadeColourAndScale(false));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_collidable) return;
        RuntimeManager.PlayOneShot("event:/PianoNotes/" + Note);
        NotePlayed?.Invoke(Note);
        StartCoroutine(FadeColourAndScale(true));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!_collidable) return;
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
        yield return new WaitForSeconds(1f);
        StartCoroutine(FadeColourAndScale(false));
    }

    private IEnumerator FadeColourAndScale(bool noteOn)
    {
        var target = noteOn ? new Color(_colour.r, _colour.g, _colour.b, 0.7f) : _colour;
        var start = GetComponent<Image>().color;
        float timeCounter = 0f;
        while (timeCounter <= 0.1f)
        {
            GetComponent<Image>().color = Color.Lerp(start, target, timeCounter / 0.1f);
            var scaleDiff = 0.1f * curve.Evaluate(timeCounter / 0.1f);
            transform.localScale = noteOn ? new Vector3(1 - scaleDiff, 1 - scaleDiff) : new Vector3(0.9f + scaleDiff, 0.9f + scaleDiff);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        GetComponent<Image>().color = target;
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
