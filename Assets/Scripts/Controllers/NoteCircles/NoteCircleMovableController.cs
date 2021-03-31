using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;

public class NoteCircleMovableController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private Text text;    
    private Vector2 _size;
    private RectTransform _rt;
    private Color _textColour;
    private bool _playable;

    [HideInInspector] public Color circleColour;
    [HideInInspector] public string note;
    [HideInInspector] public float waitTime;
    [HideInInspector] public float localY = 0;
    [HideInInspector] public List<int> availableX;
    [HideInInspector] public bool octaveUp;
    [HideInInspector] public bool draggable;
    [HideInInspector] public bool clickToPlay;

    public AnimationCurve curve;
    
    public static event Action<string> NotePlayed;

    private void Awake()
    {
        _textColour = text.color;
        text.color = Color.clear;
        _rt = GetComponent<RectTransform>();
        _size = _rt.sizeDelta;
        availableX = new List<int>{-175, -125, -75, -25, 25, 75, 125, 175};
    }

    public void Show()
    {
        if(note.Contains("1") || note.Contains("2") || note.Contains("3"))
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
        if(waitTime > 0)
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
            GetComponent<Image>().color = Color.Lerp(GetComponent<Image>().color, circleColour, timeCounter / time);
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
        StartCoroutine(Resize(true, true));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StartCoroutine(Resize(false, true));
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(draggable && !PauseManager.paused)
            transform.position = eventData.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickToPlay)
        {
            RuntimeManager.PlayOneShot("event:/SineNotes/" + note);
        }
        if (draggable && !PauseManager.paused)
        {            
            var size = GetComponent<RectTransform>().sizeDelta;
            size *= 0.95f;
            GetComponent<RectTransform>().sizeDelta = size;   
        }
        StartCoroutine(Resize(true, false));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GetComponent<RectTransform>().sizeDelta = _size;
        if (Math.Abs(localY - transform.localPosition.y) <= 20)
        {
            int snap = availableX.FirstOrDefault(x => Math.Abs(x - transform.localPosition.x) <= 20);            
            transform.localPosition = new Vector3(snap == 0 ? transform.localPosition.x : snap, localY);
        }
        StartCoroutine(Resize(false, false));
    }

    private IEnumerator Resize(bool noteOn, bool enlarge)
    {
        float timeCounter = 0f;
        while (timeCounter <= 0.1f)
        {
            var scaleDiff = 0.1f * curve.Evaluate(timeCounter / 0.1f);
            if (enlarge)
            {
                transform.localScale = noteOn ? new Vector3(1 + scaleDiff, 1 + scaleDiff) : new Vector3(1.1f - scaleDiff, 1.1f - scaleDiff);
            }
            else
            {
                transform.localScale = noteOn ? new Vector3(1 - scaleDiff, 1 - scaleDiff) : new Vector3(0.9f + scaleDiff, 0.9f + scaleDiff);
            }
            timeCounter += Time.deltaTime;
            yield return null;
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
