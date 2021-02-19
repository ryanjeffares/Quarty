using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class NoteSquareMovableController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Text text;
    private Vector2 _size;
    private RectTransform _rt;
    private Color _textColour;
    private bool _playable;
    private float _startingYpos, _startingYWorldPos;   
    public float startingYpos
    {
        set
        {
            _startingYpos = value;
        }
    }

    public Color squareColour;
    public string note = "";
    public float waitTime;        
    public bool draggable;
    public AnimationCurve curve;

    public static event Action<string> NotePlayed;

    private void Awake()
    {        
        transform.localScale = new Vector3(0, 0);        
        _startingYpos = transform.localPosition.y;
        _startingYWorldPos = transform.position.y;
        _textColour = text.color;
        text.color = Color.clear;
        _rt = GetComponent<RectTransform>();
        _size = _rt.sizeDelta;        
    }

    public void Show()
    {
        if (note.Contains("1") || note.Contains("2") || note.Contains("3"))
        {
            text.text = note.Substring(0, note.Length - 1);
        }
        else
        {
            text.text = note;
        }                
        StartCoroutine(FadeIn(0.5f));
    }

    private IEnumerator FadeIn(float time)
    {
        if (waitTime > 0)
        {
            float counter = 0f;
            while (counter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                counter += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        float resolution = time / 0.016f;
        var startScale = transform.localScale;
        float interval = time / resolution;
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            text.color = Color.Lerp(text.color, _textColour, timeCounter / time);
            GetComponent<Image>().color = Color.Lerp(GetComponent<Image>().color, squareColour, timeCounter / time);
            var scale = transform.localScale;
            scale.x = startScale.x + curve.Evaluate(timeCounter / time);
            scale.y = startScale.y + curve.Evaluate(timeCounter / time);
            transform.localScale = scale;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        _playable = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_playable) return;        
        NotePlayed?.Invoke(text.text);        
        RuntimeManager.PlayOneShot("event:/SineNotes/" + note);
        StartCoroutine(Resize(true));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StartCoroutine(Resize(false));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggable && !PauseManager.paused)
        {                           
            // is this hacky? i dunno but fuck knows i couldnt get it to work otherwise
            // move only the x pos as you drag but clamp between -80/80 of the start position
            // its all done on the same frame so its FINE
            transform.position = new Vector3(eventData.position.x, _startingYWorldPos);
            float newX = transform.localPosition.x;
            if (newX > 80) newX = 80;
            if (newX < -80) newX = -80;
            transform.localPosition = new Vector3(newX, _startingYpos);
        }            
    }

    public void OnPointerDown(PointerEventData eventData)
    {        
        if (draggable && !PauseManager.paused)
        {                        
            var size = GetComponent<RectTransform>().sizeDelta;
            size *= 0.95f;
            GetComponent<RectTransform>().sizeDelta = size;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GetComponent<RectTransform>().sizeDelta = _size;
    }

    private IEnumerator Resize(bool enlarge)
    {
        float target = enlarge ? _size.x * 1.1f : _size.x;
        if (enlarge)
        {
            while (_rt.rect.width <= target)
            {
                var sizeDelta = _rt.sizeDelta;
                sizeDelta.x = sizeDelta.x > target ? target : sizeDelta.x + Time.deltaTime / 0.01f;
                sizeDelta.y = sizeDelta.y > target ? target : sizeDelta.y + Time.deltaTime / 0.01f;
                _rt.sizeDelta = sizeDelta;
                yield return null;
            }
        }
        else
        {
            while (_rt.rect.width >= target)
            {
                var sizeDelta = _rt.sizeDelta;
                sizeDelta.x = sizeDelta.x < target ? target : sizeDelta.x - Time.deltaTime / 0.01f;
                sizeDelta.y = sizeDelta.y < target ? target : sizeDelta.y - Time.deltaTime / 0.01f;
                _rt.sizeDelta = sizeDelta;
                yield return null;
            }
        }
    }

    public IEnumerator Destroy()
    {
        float time = 0.5f;
        float timeCounter = 0f;
        float resolution = time / 0.016f;
        float interval = time / resolution;
        var start = GetComponent<Image>().color;
        while (timeCounter <= time)
        {
            GetComponent<Image>().color = Color.Lerp(start, Color.clear, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }

        Destroy(gameObject);
    }
}
