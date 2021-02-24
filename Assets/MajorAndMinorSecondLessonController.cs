using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;

public class MajorAndMinorSecondLessonController : BaseManager
{
    [SerializeField] private GameObject mainContainer, noteSquareContainer;
    [SerializeField] private GameObject nextButton, tryButton;
    [SerializeField] private GameObject noteSquarePrefab;
    [SerializeField] private List<GameObject> exampleArrows;
    [SerializeField] private Text introText, notesText, helpText;
    [SerializeField] private AnimationCurve easeInOutCurve;

    private int _levelStage;
    private List<GameObject> _exampleSquares, _challengeSquares;
    private List<string> _playedNotes;
    private Dictionary<GameObject, bool> _arrowMovingLookup;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {tryButton, TryButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {notesText, true },
            {helpText, true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true },
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
        _arrowMovingLookup = new Dictionary<GameObject, bool>();
        foreach(var arrow in exampleArrows)
        {
            _arrowMovingLookup.Add(arrow, false);
        }
        _playedNotes = new List<string>();
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeText(notesText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        StartCoroutine(AdvanceLevelStage());
    }

    private void TryButtonCallback(GameObject g)
    {

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
                introText.text = "The Major 2nd is 2 semitones. The Minor 2nd is 1 semitone. Here's what they sound like!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                SpawnSampleSquares();
                foreach (var arrow in exampleArrows)
                {
                    StartCoroutine(FadeInArrow(arrow, 0.5f));
                }
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait:5f));
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
                    yield return null;
                }
                introText.text = "Now try match up the options with the correct root note and type of 2nd.";
                SpawnChallengeSquares();
                StartCoroutine(FadeText(introText, true, 0.5f));
                break;
        }
    }

    private void SpawnSampleSquares()
    {
        var noteCoords = new List<Tuple<string, Vector2>>
        {
            new Tuple<string, Vector2>("C2", new Vector2(-100, -100) ),
            new Tuple<string, Vector2>("D2", new Vector2(-100, 0) ),
            new Tuple<string, Vector2>("C2", new Vector2(100, -100) ),
            new Tuple<string, Vector2>("C#2", new Vector2(100, 0) )
        };
        _exampleSquares = new List<GameObject>();
        float waitTime = 0f;
        foreach(var notePair in noteCoords)
        {
            var square = Instantiate(noteSquarePrefab, noteSquareContainer.transform);
            square.transform.localPosition = notePair.Item2;
            _exampleSquares.Add(square);
            var controller = square.GetComponent<NoteSquareMovableController>();
            controller.note = notePair.Item1;
            controller.waitTime = waitTime;
            controller.squareColour = Persistent.noteColours[notePair.Item1.Substring(0, notePair.Item1.Length - 1)];
            controller.Show();
            waitTime += 0.1f;            
        }
        foreach(var arrow in exampleArrows)
        {
            StartCoroutine(MoveArrow(arrow, new Vector2(arrow.transform.localPosition.x, 100), 1f, waitTime: 1f + exampleArrows.IndexOf(arrow) * 1.5f));
        }
    }

    private void SpawnChallengeSquares()
    {
        var rootNotes = new OrderedDictionary
        {
            {"C2", new Vector2(-200, -100) },
            {"E1", new Vector2(-100, -100) },
            {"B1", new Vector2(100, -100) },
            {"G1", new Vector2(200, -100) }
        };
        var options = new OrderedDictionary
        {
            {"A1", new Vector2(200, 100) },
            {"F1", new Vector2(-100, 100) },
            {"D2", new Vector2(-200, 100) },                        
            {"C1", new Vector2(100, 100) }
        };
        _challengeSquares = new List<GameObject>();
        float waitTime = 0f;

    }

    private IEnumerator MoveArrow(GameObject arrow, Vector2 target, float time, bool disableTrigger = false, float waitTime = 0f)
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
        yield return new WaitUntil(() => !_arrowMovingLookup[arrow]);
        _arrowMovingLookup[arrow] = true;
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }
        float resolution = time / 0.016f;
        float timeCounter = 0f;
        float interval = time / resolution;
        var startPos = arrow.transform.localPosition;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            arrow.transform.localPosition = Vector2.Lerp(startPos, target, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        _arrowMovingLookup[arrow] = false;
        StartCoroutine(MoveArrowLog(arrow, new Vector2(arrow.transform.localPosition.x, -200), 1f, true, true, 0.2f));
    }

    private IEnumerator MoveArrowLog(GameObject arrow, Vector2 target, float time, bool disableTrigger, bool reset, float waitTime = 0f)
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
        yield return new WaitUntil(() => !_arrowMovingLookup[arrow]);
        _arrowMovingLookup[arrow] = true;
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }
        float resolution = time / 0.016f;
        float targetX = target.x;
        float targetY = target.y;
        var startPos = arrow.transform.localPosition;
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
            var pos = arrow.transform.localPosition;
            pos.y = startPos.y + (easeInOutCurve.Evaluate(timeCounter / time) * yDiff);
            pos.x = startPos.x + (easeInOutCurve.Evaluate(timeCounter / time) * xDiff);
            arrow.transform.localPosition = pos;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        if (reset)
        {
            _playedNotes.Clear();
        }
        _arrowMovingLookup[arrow] = false;
    }

    private IEnumerator FadeInArrow(GameObject arrow, float time)
    {
        float alpha;
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            alpha = Mathf.Lerp(0f, 1f, timeCounter / time);
            arrow.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }
}
