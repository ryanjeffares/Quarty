﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MinorIntervalsLessonController : BaseManager
{
    [SerializeField] private GameObject mainContainer, noteSquareContainer;
    [SerializeField] private GameObject nextButton, tryButton;
    [SerializeField] private GameObject noteSquarePrefab;
    [SerializeField] private GameObject arrow;
    [SerializeField] private Text introText, notesText, helpText;
    [SerializeField] private AnimationCurve easeInOutCurve;

    private List<GameObject> _demoNoteSquares;
    private List<string> _playedNotes, _correctOrder;
    private int _levelStage;
    private bool _arrowMoving, _ready, _complete;

    protected override void OnAwake()
    {
        NoteSquareMovableController.NotePlayed += NotePlayedCallback;
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {tryButton, TryButtonCallback }
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {notesText, true },
            {helpText, true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true },
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
        _demoNoteSquares = new List<GameObject>();
        _playedNotes = new List<string>();
        _correctOrder = new List<string> { "E", "G", "C", "D", "E" };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeText(notesText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    protected override void DestroyManager()
    {
        NoteSquareMovableController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if(_levelStage < 3)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.UpdateUserGlossary(new[] { "Minor Scale", "Minor Third", "Minor Sixth", "Minor Seventh" });
            Persistent.sceneToLoad = "MinorIntervalsPuzzle";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void TryButtonCallback(GameObject g)
    {
        if (_arrowMoving || !_ready) return;
        _playedNotes.Clear();
        StartCoroutine(MoveArrow(new Vector2(-200, -200), 1.8f));
    }

    private void NotePlayedCallback(string note)
    {
        if (_complete || _levelStage < 2) return;
        _playedNotes.Add(note);
        if(_playedNotes.Count == 5)
        {
            if (_playedNotes.SequenceEqual(_correctOrder))
            {
                helpText.text = "Amazing! You can hear it again or move into the puzzle.";
                StartCoroutine(FadeText(helpText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                _complete = true;
            }
            else
            {
                helpText.text = "That wasn't quite right, use the notes below to figure out which are the correct ones.";
                StartCoroutine(FadeText(helpText, true, 0.5f, fadeOut: true, duration: 3f));
            }
        }
    }

    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                float timeCounter = 0f;
                while(timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
                introText.text = "We write these with a lower case m, so they would be the m3 (Minor 3rd), m6 (Minor 6th), and m7 (Minor 7th).\n \nThis is what they sound like for A Minor!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(SpawnSquaresInitial());
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 4f));
                break;
            case 2:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));                
                foreach(var n in _demoNoteSquares)
                {
                    StartCoroutine(MoveObject(n, new Vector2(-500, n.transform.localPosition.y), 0.5f, destroy:true));                    
                }
                _demoNoteSquares.Clear();
                timeCounter = 0f;
                while (timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
                introText.text = "The distance from the root of a m3 is 3 Semitones.\nThe distance of a m6 is 8 Semitones.\nThe distance of a m7 is 10 Semitones.\n \nUsing the list of notes below, can you fill in these intervals for the E Minor Scale? One of the options is wrong.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f, wait: 1f));
                StartCoroutine(SpawnChallengeSquares());
                break;
        }
    }

    private IEnumerator SpawnSquaresInitial()
    {
        float timeCounter = 0f;
        while (timeCounter <= 0.5f)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            timeCounter += Time.deltaTime;
            yield return null;
        }
        string[] notes = new string[] { "A1", "C2", "F2", "G2", "A2" };
        float waitTime = 0f;
        float y = 300;
        for(int i = 0; i < 5; i++)
        {
            _demoNoteSquares.Add(Instantiate(noteSquarePrefab, noteSquareContainer.transform));
            _demoNoteSquares[i].transform.localPosition = new Vector3(0, y);
            var square = _demoNoteSquares[i].GetComponent<NoteSquareMovableController>();
            square.note = notes[i];
            square.squareColour = Persistent.noteColours[notes[i].Substring(0, notes[i].Length - 1)];
            square.waitTime = waitTime;
            square.Show();            
            y -= 100;
            waitTime += 0.1f;
        }        
        float alpha = 0f;
        while (alpha <= 1f)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            var col = arrow.GetComponent<Image>().color;
            arrow.GetComponent<Image>().color = new Color(col.r, col.g, col.b, alpha);
            alpha += 0.01f;
            yield return new WaitForSeconds(0.005f);
        }
        StartCoroutine(MoveArrow(new Vector2(-200, -200), 1.5f));
    }

    private IEnumerator SpawnChallengeSquares()
    {
        float timeCounter = 0f;
        while (timeCounter <= 0.5f)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            timeCounter += Time.deltaTime;
            yield return null;
        }
        string[] notes = new string[] { "E1", "G1", "B1", "C2", "D2", "E2" };
        float waitTime = 0f;
        float y = 300;
        for (int i = 0; i < 6; i++)
        {
            _demoNoteSquares.Add(Instantiate(noteSquarePrefab, noteSquareContainer.transform));
            _demoNoteSquares[i].transform.localPosition = new Vector3(notes[i].Contains("E") ? 0 : 100, y);
            var square = _demoNoteSquares[i].GetComponent<NoteSquareMovableController>();
            square.note = notes[i];
            square.squareColour = Persistent.noteColours[notes[i].Substring(0, notes[i].Length - 1)];
            square.waitTime = waitTime;
            square.draggable = !notes[i].Contains("E");
            square.canMoveLeft = false;
            square.StartingYpos = y;
            square.Show();
            y -= 100;
            waitTime += 0.1f;
        }
        yield return new WaitForSeconds(0.5f);
        _ready = true;
    }

    protected override IEnumerator MoveObject(GameObject obj, Vector2 target, float time, float wait = 0, bool reset = false, bool destroy = false)
    {        
        if (wait > 0f)
        {     
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += Time.deltaTime;
                yield return null;
            }
        }
        float timeCounter = 0f;        
        var startPos = obj.transform.localPosition;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            obj.transform.localPosition = Vector2.Lerp(startPos, target, timeCounter / time);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        if (destroy)
        {
            Destroy(obj);
        }
    }

    private IEnumerator MoveArrow(Vector2 target, float time, bool disableTrigger = false)
    {
        yield return new WaitUntil(() => !_arrowMoving);
        _arrowMoving = true;
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }        
        float timeCounter = 0f;        
        var startPos = arrow.transform.localPosition;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            arrow.transform.localPosition = Vector2.Lerp(startPos, target, timeCounter / time);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        _arrowMoving = false;
        StartCoroutine(MoveArrowLog(new Vector2(arrow.transform.localPosition.x, 400), 1f, true, true, 0.2f));
    }

    private IEnumerator MoveArrowLog(Vector2 target, float time, bool disableTrigger, bool reset, float waitTime = 0f)
    {
        if (waitTime >= 0)
        {
            float waitCounter = 0f;
            while (waitCounter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += Time.deltaTime;
                yield return null;
            }
        }
        yield return new WaitUntil(() => !_arrowMoving);
        _arrowMoving = true;
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }        
        float targetX = target.x;
        float targetY = target.y;
        var startPos = arrow.transform.localPosition;
        float yDiff = targetY - startPos.y;
        float xDiff = targetX - startPos.x;
        float timeCounter = 0f;        
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            var pos = arrow.transform.localPosition;
            pos.y = startPos.y + (easeInOutCurve.Evaluate(timeCounter / time) * yDiff);
            pos.x = startPos.x + (easeInOutCurve.Evaluate(timeCounter / time) * xDiff);
            arrow.transform.localPosition = pos;
            timeCounter += Time.deltaTime;
            yield return null;
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        if (reset)
        {
            _playedNotes.Clear();
        }
        _arrowMoving = false;
    }
}
