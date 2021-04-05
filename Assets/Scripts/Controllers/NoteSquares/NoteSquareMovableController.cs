using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FMODUnity;

public class NoteSquareMovableController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public static event Action<string> NotePlayed;

    [SerializeField] private Text text;

    private Vector2 _size;
    private RectTransform _rt;
    private Color _textColour;
    private bool _playable;
    private float _startingYpos, _startingYWorldPos;   
    public float StartingYpos
    {
        set
        {
            _startingYpos = value;
        }
    }

    [HideInInspector] public Color squareColour;
    [HideInInspector] public string note = "";
    [HideInInspector] public float waitTime;
    [HideInInspector] public float xRange = 80;
    [HideInInspector] public float yRange = 200;
    [HideInInspector] public bool draggable, movableYpos;
    [HideInInspector] public float[] targetXs;
    [HideInInspector] public float targetY;
    [HideInInspector] public bool shouldSnap;    

    [HideInInspector] public bool canMoveRight = true, canMoveLeft = true;

    public AnimationCurve curve;    
    [SerializeField] private AnimationCurve smoothCurve;    

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
        var startScale = transform.localScale;        
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
            timeCounter += Time.deltaTime;
            yield return null;
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
            // is this hacky with world position/local position? i dunno but fuck knows i couldnt get it to work otherwise
            // move only the x pos as you drag but clamp between -80/80 of the start position
            // its all done on the same frame so its FINE
            transform.position = new Vector3(eventData.position.x, movableYpos ? eventData.position.y : _startingYWorldPos);
            float newX = transform.localPosition.x;
            float newY = transform.localPosition.y;
            if (canMoveRight)
            {
                if (newX > xRange) newX = xRange;                
            }
            else
            {
                if (newX > 0) newX = 0;
            }
            if (canMoveLeft)
            {
                if (newX < -xRange) newX = -xRange;
            }
            else
            {
                if (newX < 0) newX = 0;
            }
            if (newY > yRange) newY = yRange;
            if (newY < -yRange) newY = -yRange;
            transform.localPosition = new Vector3(newX, movableYpos ? newY : _startingYpos);
        }            
    }

    public void OnPointerDown(PointerEventData eventData)
    {        
        if (draggable && !PauseManager.paused)
        {
            StartCoroutine(OnPress(true));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(draggable && !PauseManager.paused)
        {
            StartCoroutine(OnPress(false));
            if (shouldSnap)
            {
                // for the record, i hate this
                float newY = -1, newX = -1;
                if(Mathf.Abs(targetY - transform.localPosition.y) <= 40)
                {
                    newY = targetY;
                }
                foreach(var x in targetXs)
                {
                    if(Mathf.Abs(x - transform.localPosition.x) <= 40)
                    {
                        newX = x;
                        break;
                    }
                }
                if(newX != -1 && newY != -1)
                {
                    transform.localPosition = new Vector3(newX, newY);
                }
            }
        }        
    }

    private IEnumerator OnPress(bool down)
    {
        float timeCounter = 0f;
        while (timeCounter <= 0.1f)
        {
            var scaleDiff = 0.1f * curve.Evaluate(timeCounter / 0.1f);
            transform.localScale = down ? new Vector3(1 - scaleDiff, 1 - scaleDiff) : new Vector3(0.9f + scaleDiff, 0.9f + scaleDiff);
            timeCounter += Time.deltaTime;
            yield return null;
        }
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
        var start = GetComponent<Image>().color;
        while (timeCounter <= time)
        {
            GetComponent<Image>().color = Color.Lerp(start, Color.clear, timeCounter / time);
            timeCounter += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
