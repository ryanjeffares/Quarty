using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum DrumType
{
    Kick, Snare, HiHatClosed, MidTom, Crash
}

public class DrumPieceMovableController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public static event Action<DrumType> MovableDrumPlayed;
    public static event Action<GameObject, DrumType> DrumPieceDropped;

    [SerializeField] private List<Sprite> images;// kickImg, snareImg, hatImg, tomImg, crashImg;
    [SerializeField] private AnimationCurve overShootCurve, easeInOutCurve;
    private string _fmodEvent;
    private DrumType _type;

    private bool _snapped;
    public bool Snapped
    {
        get => _snapped;
        set
        {
            _snapped = value;
            GetComponent<Image>().color = new Color(1, 1, 1, Snapped ? 1 : 0.5f);
        }
    }

    private void Awake()
    {
        transform.localScale = new Vector3(0, 0);        
    }

    public void Show(DrumType type, float waitTime)
    {
        _type = type;
        _fmodEvent = "event:/Drums/" + _type.ToString();
        GetComponent<Image>().sprite = images[(int)_type];
        StartCoroutine(FadeScale(waitTime));
    }

    private IEnumerator FadeScale(float wait)
    {
        yield return new WaitForSeconds(wait);
        float timeCounter = 0f;
        while(timeCounter <= 0.5f)
        {
            var val = overShootCurve.Evaluate(timeCounter / 0.5f);
            transform.localScale = new Vector3(val, val);
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Snapped) return;
        FMODUnity.RuntimeManager.PlayOneShot(_fmodEvent);
        MovableDrumPlayed?.Invoke(_type);
        StartCoroutine(Resize(true));
        StartCoroutine(Resize(false, wait: 0.12f));
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartCoroutine(Resize(true));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DrumPieceDropped?.Invoke(gameObject, _type);
        StartCoroutine(Resize(false));
    }

    private IEnumerator Resize(bool down, float wait = 0)
    {
        float timeCounter = 0f;
        while (timeCounter <= 0.05f)
        {
            var diff = 0.1f * easeInOutCurve.Evaluate(timeCounter / 0.05f);
            transform.localScale = new Vector3(down ? (1 - diff) : (0.9f + diff), 1 - diff);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        transform.localScale = down ? new Vector3(0.9f, 0.9f) : new Vector3(1, 1);
    }
}
