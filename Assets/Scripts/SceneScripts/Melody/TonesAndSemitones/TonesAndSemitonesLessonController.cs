﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TonesAndSemitonesLessonController : BaseManager
{
    [Header("Prefabs")] 
    [SerializeField] private GameObject scalePrefab;
    [SerializeField] private GameObject emptyScalePrefab;
    [SerializeField] private GameObject movableCirclePrefab;
    [Header("Texts")] 
    [SerializeField] private Text introText;
    [SerializeField] private Text allNotesText;
    [SerializeField] private Text helpText;
    [Header("GameObjects")] 
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject tryButton;
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject arrow;
    [Header("Curves")] 
    [SerializeField] private AnimationCurve easeInOutCurve;
    [SerializeField] private AnimationCurve overshootCurve;
    [SerializeField] private AnimationCurve overshootOutCurve;

    private int _levelStage;
    private bool _arrowMoving;
    private GameObject _noteScale, _emptyNoteScale;
    private List<GameObject> _movableCircles;
    private List<string> _playedNotes, _correctNotes;
    
    protected override void OnAwake()
    {
        NoteCircleMovableController.NotePlayed += NotePlayedCallback;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();                
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback},
            {tryButton, TryButtonCallback}
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true},
            {allNotesText, true},
            {helpText, true},
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true},
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true}
        };
        _movableCircles = new List<GameObject>();
        _playedNotes = new List<string>();
        _correctNotes = new List<string>
        {
            "A", "B", "C#", "D", "E", "F#", "G#", "A"
        };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 1f));
    }

    protected override void DestroyManager()
    {
        NoteCircleMovableController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        if(_levelStage < 4)
        {
            ++_levelStage;
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.UpdateUserGlossary(new[] { "Interval", "Tone", "Semitone" });
            Persistent.sceneToLoad = "TonesAndSemitonesPuzzle";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void TryButtonCallback(GameObject g)
    {
        if(_levelStage > 2 && !_arrowMoving)
        {
            _playedNotes = new List<string>();
            StartCoroutine(MoveObject(arrow, new Vector2(260, 200), 2f));
        }                    
    }

    private void NotePlayedCallback(string note)
    {
        _playedNotes.Add(note);        
        if (_playedNotes.Count == 8)
        {
            if (_playedNotes.SequenceEqual(_correctNotes))
            {
                if(_levelStage == 3) 
                {                    
                    helpText.text = "Awesome! You can hear it again or move into the puzzle.";
                    StartCoroutine(FadeText(helpText, true, 0.5f));
                    StartCoroutine(FadeButtonText(nextButton, true, 0.5f));                        
                    ++_levelStage;                        
                }
            }
            else
            {
                if (_levelStage == 3)
                {                    
                    helpText.text = "That wasn't quite right - look at the list of notes below and remember a Tone is 2 notes and a Semitone is 1 note.";
                    StartCoroutine(FadeText(helpText, true, 0.5f, fadeOut: true, duration: 3f));
                    StartCoroutine(MoveMovableCircles(0.5f));                 
                }
            }
            _playedNotes.Clear();
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
                while (timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "A Tone is an interval of two notes, so C to D for example.\n \nC -> C# -> D\n \nA Semitone is an interval of only one note, so like C to C#.\n \nC -> C#";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
                break;
            case 2:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                timeCounter = 0f;
                while (timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "Look how the notes follow the tone and semitone pattern.\n \nRemember, all the notes in music are A, A#, B, C, C#, D, D#, E, F, F#, G, and G# - and they repeat!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                timeCounter = 0f;
                while (timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                StartCoroutine(SpawnSampleScale());
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 4f));
                break;
            case 3:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(MoveObjectLog(arrow, new Vector2(-260, 200), 1.5f, disableTrigger:true));
                StartCoroutine(MoveObjectLog(_noteScale, new Vector2(-1000, 50), 1.5f, destroy:true));
                StartCoroutine(RotateArrow360(1.5f));
                timeCounter = 0f;
                while (timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "Now you try it! We'll keep the list of all notes on the screen to help you.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeText(allNotesText, true, 0.5f));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f, 1f));
                SpawnEmptyScale();
                break;
        }
    }

    private IEnumerator SpawnSampleScale()
    {
        _noteScale = Instantiate(scalePrefab, mainContainer.transform);
        _noteScale.transform.localPosition = new Vector3(0, 50);
        _noteScale.GetComponent<NoteCirclesScaleController>().Show(true, new List<string> { "A1", "B1", "C#2", "D2", "E2", "F#2", "G#2", "A2" });
        float alpha = 0f;
        while (alpha <= 1f)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            var col = arrow.GetComponent<Image>().color;
            arrow.GetComponent<Image>().color = new Color(col.r, col.g, col.b, alpha);
            alpha += (1 / (0.5f / 0.016f));
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return new WaitForSeconds(1f);
        StartCoroutine(MoveObject(arrow, new Vector2(260, 50), 2f));
    }

    private void SpawnMovableCircles()
    {
        string[] notes = { "A1", "B1", "C#2", "D2", "E2", "F#2", "G#2", "A2"};
        int[] localXs = {-125, -75, -25, 25, 75, 125};
        List<int> indexes = new List<int>{0, 1, 2, 3, 4, 5};
        indexes.Shuffle();
        float wait = 0f;
        for (int i = 0; i < 8; i++)
        {
            _movableCircles.Add(Instantiate(movableCirclePrefab, _emptyNoteScale.transform));
            _movableCircles[i].GetComponent<NoteCircleMovableController>().note = notes[i];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().waitTime = wait;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().draggable = true;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().circleColour = Persistent.noteColours[notes[i].Substring(0, notes[i].Length - 1)];                    
            if (i == 0) // if its a C
            {
                _movableCircles[i].transform.localPosition = new Vector3(-175, 0);
            }
            else if (i == 7)
            {
                _movableCircles[i].transform.localPosition = new Vector3(175, 0);
            }
            else
            {
                _movableCircles[i].transform.localPosition = new Vector3(localXs[indexes[i - 1]], -60);
            }
            _movableCircles[i].GetComponent<NoteCircleMovableController>().Show();
            wait += 0.1f;
        }
    }

    private IEnumerator MoveMovableCircles(float time)
    {
        int[] localXs = { -125, -75, -25, 25, 75, 125 };
        List<int> indexes = new List<int>{0, 1, 2, 3, 4, 5};
        indexes.Shuffle();
        Dictionary<GameObject, Vector3> startPositions = new Dictionary<GameObject, Vector3>();
        Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();
        Dictionary<GameObject, Tuple<float, float>> diffs = new Dictionary<GameObject, Tuple<float, float>>();
        int i = 0;
        foreach(var circle in _movableCircles.Where(c => !c.GetComponent<NoteCircleMovableController>().note.Contains("A")))
        {
            startPositions.Add(circle, circle.transform.localPosition);
            targetPositions.Add(circle, new Vector3(localXs[indexes[i]], - 60));
            diffs.Add(circle, new Tuple<float, float>
                (targetPositions[circle].x - startPositions[circle].x, targetPositions[circle].y - startPositions[circle].y));
            i++;
        }
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            foreach (var circle in _movableCircles.Where(c => !c.GetComponent<NoteCircleMovableController>().note.Contains("A")))
            {
                var pos = circle.transform.localPosition;
                pos.x = startPositions[circle].x + easeInOutCurve.Evaluate(timeCounter / time) * diffs[circle].Item1;
                pos.y = startPositions[circle].y + easeInOutCurve.Evaluate(timeCounter / time) * diffs[circle].Item2;
                circle.transform.localPosition = pos;
            }
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }

    private void SpawnEmptyScale()
    {        
        _emptyNoteScale = Instantiate(emptyScalePrefab, mainContainer.transform);
        _emptyNoteScale.transform.localPosition = new Vector3(0, 200);
        SpawnMovableCircles();
    }        

    protected override IEnumerator MoveObject(GameObject obj, Vector2 target, float time, float wait = 0f,
        bool disableTrigger = false, bool destroy = false)
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

        if (obj == arrow && _arrowMoving)
        {
            yield return new WaitUntil(() => !_arrowMoving);
            _arrowMoving = true;
        }

        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
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
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        if (destroy)
        {
            Destroy(obj);
        }
        if (obj == arrow)
        {
            _arrowMoving = false;
            if (target.x > 0)
            {
                if(_levelStage > 2)
                {
                    StartCoroutine(MoveObjectLog(arrow, new Vector2(-260, arrow.transform.localPosition.y), 1f, 0f,
                        true, true));
                }
            }
        }
    }

    protected override IEnumerator MoveObjectLog(GameObject obj, Vector2 target, float time,
        float wait = 0f,
        bool disableTrigger = false, bool reset = true, bool destroy = false)
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
        if (obj == arrow && _arrowMoving)
        {
            yield return new WaitUntil(() => !_arrowMoving);
            _arrowMoving = true;
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }
        float targetX = target.x;
        float targetY = target.y;
        var startPos = obj.transform.localPosition;
        float yDiff = targetY - startPos.y;
        float xDiff = targetX - startPos.x;
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            var pos = obj.transform.localPosition;
            pos.y = startPos.y + (easeInOutCurve.Evaluate(timeCounter / time) * yDiff);
            pos.x = startPos.x + (easeInOutCurve.Evaluate(timeCounter / time) * xDiff);
            obj.transform.localPosition = pos;
            timeCounter += Time.deltaTime;
            yield return null;
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        if (obj == arrow)
        {
            _arrowMoving = false;
        }

        if (destroy)
        {
            Destroy(obj);
        }
    }
    
    private IEnumerator RotateArrow360(float time, float wait = 0f)
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
        var rotation = arrow.transform.eulerAngles;
        while (timeCounter <= time)
        {            
            arrow.transform.eulerAngles = new Vector3(0, 0, rotation.z + (360 * (timeCounter / time)));
            timeCounter += Time.deltaTime;
            yield return null;
        }
        arrow.transform.eulerAngles = new Vector3(0, 0, 90);
    }
}