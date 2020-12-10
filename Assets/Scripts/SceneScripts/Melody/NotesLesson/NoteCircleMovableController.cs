﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoteCircleMovableController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private Text text;
    private Vector2 _size;
    private RectTransform _rt;
    private Color _textColour;

    public Color circleColour;
    public string note;
    public float waitTime;
    public float localY = 0;
    public List<int> availableX;
    public bool octaveUp;

    public static Action<string> NotePlayed;

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
        text.text = note;
        StartCoroutine(FadeIn(0.5f, 200f));
    }

    private IEnumerator FadeIn(float time, float resolution)
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
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        NotePlayed?.Invoke(text.text);
        GetComponent<AudioSource>().Play();
        StartCoroutine(Resize(true));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        StartCoroutine(Resize(false));
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        var size = GetComponent<RectTransform>().sizeDelta;
        size *= 0.95f;
        GetComponent<RectTransform>().sizeDelta = size;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GetComponent<RectTransform>().sizeDelta = _size;
        if (Math.Abs(localY - transform.localPosition.y) < 20)
        {
            foreach (int x in availableX.Where(x => Math.Abs(x - transform.localPosition.x) < 20))
            {
                transform.localPosition = new Vector3(x, localY);
                break;
            }
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
}