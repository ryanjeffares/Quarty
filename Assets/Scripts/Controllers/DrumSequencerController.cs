using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumSequencerController : MonoBehaviour
{
    public static event Action PlayerDone;
    [SerializeField] private GameObject container, squares, player;
    [SerializeField] private AnimationCurve overshootCurve, easeCurve;

    private List<GameObject> _kickSquares, _snareSquares, _hatSquares;
    private bool _playing;
    public bool Playing
    {
        get => _playing;
        set
        {
            _playing = value;
            player.GetComponent<BoxCollider2D>().enabled = _playing;
        }
    }

    private List<int> xPositions = new List<int>
    {
        -210, -150, -90, -30, 30, 90, 150, 210
    };

    private void Awake()
    {
        DrumPieceMovableController.DrumPieceDropped += DrumPieceDropped;
        container.transform.localScale = Vector3.zero;
        _kickSquares = new List<GameObject>();
        _snareSquares = new List<GameObject>();
        _hatSquares = new List<GameObject>();
        for(int i = 0; i < squares.transform.childCount; i++)
        {
            if (i < 8)
            {
                _kickSquares.Add(squares.transform.GetChild(i).gameObject);
            }
            else if (i < 16)
            {
                _snareSquares.Add(squares.transform.GetChild(i).gameObject);
            }
            else if (i < 24)
            {
                _hatSquares.Add(squares.transform.GetChild(i).gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        DrumPieceMovableController.DrumPieceDropped -= DrumPieceDropped;
    }

    public void FadeInScale()
    {
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        player.transform.localPosition = new Vector3(-243, 0);
        float timeCounter = 0f;
        while (timeCounter <= 0.5f)
        {
            container.transform.localScale = new Vector3(overshootCurve.Evaluate(timeCounter / 0.5f), overshootCurve.Evaluate(timeCounter / 0.5f));
            timeCounter += Time.deltaTime;
            yield return null;
        }
        container.transform.localScale = Vector3.one;
    }

    private void DrumPieceDropped(GameObject g, DrumType type)
    {
        if (!g.TryGetComponent(out DrumPieceMovableController dp)) return;
        var pos = g.transform.localPosition;
        if (pos.x < -250 || pos.x > 250 || pos.y < -100 || pos.y > 100) 
        {
            dp.Snapped = false;
        }
        else
        {
            int xNew = xPositions.FirstOrDefault(x => Mathf.Abs(x - pos.x) <= 30);
            int yNew = -1;
            switch (type)
            {
                case DrumType.Kick:
                    if (pos.y > 30 && pos.y < 90)
                    {
                        yNew = 60;
                    }
                    break;
                case DrumType.Snare:
                    if (pos.y < 30 && pos.y > -30)
                    {
                        yNew = 0;
                    }
                    break;
                case DrumType.HiHatClosed:
                    if (pos.y > -90 && pos.y < -30)
                    {
                        yNew = -60;
                    }
                    break;
            }
            if (yNew == -1)
            {
                dp.Snapped = false;
            }
            else
            {
                g.transform.localPosition = new Vector3(xNew, yNew);
                dp.Snapped = true;
            }
        }        
    }

    public void PlaySequence(float time)
    {
        StartCoroutine(SequenceAsync(time));        
    }

    private IEnumerator SequenceAsync(float time)
    {
        var target = new Vector3(243, 0);
        var start = player.transform.localPosition.x;
        var xDiff = target.x - start;
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            player.transform.localPosition = new Vector3(start + (xDiff * (timeCounter / time)), 0);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        Playing = false;
        PlayerDone?.Invoke();
        StartCoroutine(MovePlayerBackToStart());
    }

    private IEnumerator MovePlayerBackToStart()
    {
        var target = new Vector3(-243, 0);
        float timeCounter = 0f;
        var start = player.transform.localPosition.x;
        var xDiff = target.x - start;
        while (timeCounter <= 0.5f)
        {
            player.transform.localPosition = new Vector3(start + (xDiff * easeCurve.Evaluate(timeCounter / 0.5f)), 0);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        player.transform.localPosition = target;
    }
}
