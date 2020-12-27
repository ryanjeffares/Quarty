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
    [Header("GameObjects")] 
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject tryButton;
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject arrow;
    [Header("Curves")] 
    [SerializeField] private AnimationCurve easeInOutCurve;
    [SerializeField] private AnimationCurve overshootCurve;
    [SerializeField] private AnimationCurve overshootOutCurve;
    [Header("Clips")] 
    [SerializeField] private List<AudioClip> clips;

    private int _levelStage;
    private bool _arrowMoving;
    private GameObject _noteScale, _emptyNoteScale;
    private List<GameObject> _movableCircles;
    private List<string> _playedNotes, _correctNotes;
    
    protected override void OnAwake()
    {
        NoteCircleMovableController.NotePlayed += NotePlayedCallback;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback},
            {tryButton, TryButtonCallback}
        };
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback},
            {tryButton, TryButtonCallback}
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true},
            {allNotesText, true},
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true},
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true}
        };
        _movableCircles = new List<GameObject>();
        _playedNotes = new List<string>();
        _correctNotes = new List<string>
        {
            "A", "B", "C#", "D", "E", "F#", "G#", "A"
        };
        StartCoroutine(FadeText(introText, true, 0.5f, 200f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 200f, 1f));
    }

    protected override void DestroyManager()
    {
        NoteCircleMovableController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        StartCoroutine(AdvanceLevelStage());
    }

    private void TryButtonCallback(GameObject g)
    {
        switch (_levelStage)
        {
            case 2:
                StartCoroutine(MoveObject(arrow, new Vector2(215, 200), 1.2f, 200f));
                break;
        }
    }

    private void NotePlayedCallback(string note)
    {
        _playedNotes.Add(note);
        if (_playedNotes.Count == 8)
        {
            if (_playedNotes.SequenceEqual(_correctNotes))
            {
                switch (_levelStage)
                {
                    case 2:
                        break;
                }
            }
            else
            {
                
            }
            _playedNotes.Clear();
        }
    }

    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f, 200f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f, 200f));
                float timeCounter = 0f;
                while (timeCounter <= 1f)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                    timeCounter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "Look how the notes follow the tone and semitone pattern.\n \nRemember, all the notes in music are A, A#, B, C, C#, D, D#, E, F, F#, G, and G# - and they repeat!";
                StartCoroutine(FadeText(introText, true, 0.5f, 200f));
                timeCounter = 0f;
                while (timeCounter <= 1f)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                    timeCounter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                StartCoroutine(SpawnSampleScale());
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 200f, 4f));
                break;
            case 2:
                StartCoroutine(FadeText(introText, false, 0.5f, 200f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f, 200f));
                StartCoroutine(MoveObjectLog(arrow, new Vector2(-215, 200), 0.5f, 100f, disableTrigger:true));
                StartCoroutine(MoveObjectLog(_noteScale, new Vector2(-1000, 50), 0.5f, 100f, destroy:true));
                StartCoroutine(RotateArrow360(0.5f, 100f));
                timeCounter = 0f;
                while (timeCounter <= 1f)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                    timeCounter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "Now you try it! We'll keep the list of all notes on the screen to help you.";
                StartCoroutine(FadeText(introText, true, 0.5f, 200f));
                StartCoroutine(FadeText(allNotesText, true, 0.5f, 200f));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f, 200f, 1f));
                StartCoroutine(SpawnEmptyScale());
                break;
        }
    }

    private IEnumerator SpawnSampleScale()
    {
        _noteScale = Instantiate(scalePrefab, mainContainer.transform);
        _noteScale.transform.localPosition = new Vector3(0, 50);
        float alpha = 0f;
        while (alpha <= 1f)
        {
            yield return new WaitUntil(() => !PauseManager.paused);
            var col = arrow.GetComponent<Image>().color;
            arrow.GetComponent<Image>().color = new Color(col.r, col.g, col.b, alpha);
            alpha += 0.01f;
            yield return new WaitForSeconds(0.005f);
        }
        yield return new WaitForSeconds(1f);
        StartCoroutine(MoveObject(arrow, new Vector2(215, 50), 1.2f, 200f));
    }

    private void SpawnMovableCircles()
    {
        string[] notes = { "A", "B", "C#", "D", "E", "F#", "G#", "A"};
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
            _movableCircles[i].GetComponent<NoteCircleMovableController>().circleColour = Persistent.noteColours[notes[i]];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().curve = overshootCurve;
            _movableCircles[i].GetComponent<AudioSource>().clip = clips[i];
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
    
    private IEnumerator SpawnEmptyScale()
    {
        float counter = 0f;
        while (counter <= 1)
        {
            yield return new WaitUntil(() => !PauseManager.paused);
            counter += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        _emptyNoteScale = Instantiate(emptyScalePrefab, mainContainer.transform);
        _emptyNoteScale.transform.localPosition = new Vector3(0, 200);
        SpawnMovableCircles();
    }
    
    protected override IEnumerator MoveObject(GameObject obj, Vector2 target, float time, float resolution, float wait = 0f,
        bool disableTrigger = false, bool destroy = false)
    {
        if (wait > 0f)
        {
            float waitInterval = wait / resolution;
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            }   
        }

        if (obj == arrow && _arrowMoving)
        {
            yield return new WaitUntil(() => !_arrowMoving);
            _arrowMoving = true;
        }

        if (obj == arrow && buttonCallbackLookup.ContainsKey(tryButton))
        {
            buttonCallbackLookup.Remove(tryButton);
        }

        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }
        float timeCounter = 0f;
        float interval = time / resolution;
        var startPos = obj.transform.localPosition;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            obj.transform.localPosition = Vector2.Lerp(startPos, target, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
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
                if(_levelStage > 1)
                {
                    StartCoroutine(MoveObjectLog(arrow, new Vector2(-215, arrow.transform.localPosition.y), 1f, 200f,
                        0f,
                        true, true));
                }
            }
        }
    }

    protected override IEnumerator MoveObjectLog(GameObject obj, Vector2 target, float time, float resolution,
        float wait = 0f,
        bool disableTrigger = false, bool reset = true, bool destroy = false)
    {
        if (wait > 0f)
        {
            float waitInterval = wait / resolution;
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
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
        float interval = time / resolution;
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
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        if (reset)
        {
            //_playedNotes.Clear();
        }
        if (obj == arrow)
        {
            _arrowMoving = false;
            if (!buttonCallbackLookup.ContainsKey(tryButton))
            {
                buttonCallbackLookup.Add(tryButton, TryButtonCallback);
            }
        }

        if (destroy)
        {
            Destroy(obj);
        }
    }
    
    private IEnumerator RotateArrow360(float time, float resolution, float wait = 0f)
    {
        if (wait > 0f)
        {
            float waitInterval = wait / resolution;
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            }   
        }
        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            var rotation = arrow.transform.eulerAngles;
            arrow.transform.eulerAngles = new Vector3(0, 0, rotation.z + 360 / resolution);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        arrow.transform.eulerAngles = new Vector3(0, 0, 90);
    }
}