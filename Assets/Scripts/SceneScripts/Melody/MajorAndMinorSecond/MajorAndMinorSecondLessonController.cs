using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MajorAndMinorSecondLessonController : BaseManager
{
    [SerializeField] private GameObject mainContainer, noteSquareContainer;
    [SerializeField] private GameObject nextButton, tryButton;
    [SerializeField] private GameObject noteSquarePrefab;
    [SerializeField] private List<GameObject> exampleArrows, challengeArrows;
    [SerializeField] private List<Text> exampleTexts, challengeTexts;
    [SerializeField] private Text introText, notesText, helpText;
    [SerializeField] private AnimationCurve easeInOutCurve;

    private int _levelStage;
    private List<GameObject> _exampleSquares, _challengeSquares;
    private List<string> _playedNotes, _correctOrder;
    private List<Vector2> _arrowTargets;
    private Dictionary<GameObject, bool> _arrowMovingLookup;

    protected override void OnAwake()
    {
        NoteSquareMovableController.NotePlayed += NotePlayedCallback;
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
            {exampleTexts[0], true },            
            {exampleTexts[1], true },
            {challengeTexts[0], true },
            {challengeTexts[1], true },
            {challengeTexts[2], true },
            {challengeTexts[3], true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true },
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
        _arrowTargets = new List<Vector2>
        {
            {new Vector2(-150, 100) },
            {new Vector2(-50, 100) },
            {new Vector2(50, 100) },
            {new Vector2(150, 100) }
        };
        _arrowMovingLookup = new Dictionary<GameObject, bool>();
        foreach(var arrow in exampleArrows.Concat(challengeArrows))
        {
            _arrowMovingLookup.Add(arrow, false);
        }
        _playedNotes = new List<string>();
        _correctOrder = new List<string> { "C", "D", "E", "F", "B", "C", "G", "A" };
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
            Persistent.UpdateUserGlossary(new[] { "Major Second", "Minor Second" });
            Persistent.melodyLessons.lessons["Melody Writing"] = true;
            Persistent.sceneToLoad = "MelodyWriting";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void TryButtonCallback(GameObject g)
    {
        if (_levelStage < 2) return;
        _playedNotes = new List<string>();
        float waitTime = 0f;
        foreach(var (a, i) in challengeArrows.WithIndex())
        {
            StartCoroutine(MoveArrow(a, _arrowTargets[i], 1f, waitTime: waitTime));
            waitTime += 1.5f;
        }
    }

    private void NotePlayedCallback(string note)
    {
        if (_levelStage != 2) return;
        _playedNotes.Add(note);
        if(_playedNotes.Count == 8)
        {
            if (_playedNotes.SequenceEqual(_correctOrder))
            {
                helpText.text = "Awesome! You can hear it again or move into the next part.";
                StartCoroutine(FadeText(helpText, true, 0.5f));                
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            }
            else
            {
                helpText.text = "That wasn't quite right, use the list of notes below and remember a Major 2nd is 2 semitones and a Minor 2nd is 1 semitone.";
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
                foreach(var obj in _exampleSquares.Concat(exampleArrows))
                {
                    StartCoroutine(MoveObject(obj, new Vector2(-500, obj.transform.localPosition.y), 1f, destroy: true));
                }
                exampleTexts.ForEach(t => StartCoroutine(FadeText(t, false, 0.5f, destroy: true)));
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
                challengeTexts.ForEach(t => StartCoroutine(FadeText(t, true, 0.5f)));
                challengeArrows.ForEach(a => StartCoroutine(FadeInArrow(a, 0.5f)));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f, wait: 0.5f));
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
        foreach(var t in exampleTexts)
        {
            StartCoroutine(FadeText(t, true, 0.5f));
        }
        foreach(var arrow in exampleArrows)
        {
            StartCoroutine(MoveArrow(arrow, new Vector2(arrow.transform.localPosition.x, 100), 1f, waitTime: 1f + exampleArrows.IndexOf(arrow) * 1.5f));
        }
    }

    private void SpawnChallengeSquares()
    {        
        var rootNotes = new List<Tuple<string, Vector2>>
        {
            new Tuple<string, Vector2>("C2", new Vector2(-150, -100)),
            new Tuple<string, Vector2>("E1", new Vector2(-50, -100)),
            new Tuple<string, Vector2>("B1", new Vector2(50, -100)),
            new Tuple<string, Vector2>("G1", new Vector2(150, -100))
        };
        var options = new List<Tuple<string, Vector2>>
        {
            new Tuple<string, Vector2>("A1", new Vector2(50, 200)),
            new Tuple<string, Vector2>("F1", new Vector2(-150, 200)),
            new Tuple<string, Vector2>("D2", new Vector2(-50, 200)),
            new Tuple<string, Vector2>("C2", new Vector2(150, 200))
        };
        _challengeSquares = new List<GameObject>();
        float waitTime = 0f;
        for(int i = 0; i < rootNotes.Count; i++)
        {
            var rootSquare = Instantiate(noteSquarePrefab, noteSquareContainer.transform);
            rootSquare.transform.localPosition = rootNotes[i].Item2;
            var movableSquare = Instantiate(noteSquarePrefab, noteSquareContainer.transform);
            movableSquare.transform.localPosition = options[i].Item2;
            _challengeSquares.Add(rootSquare);
            _challengeSquares.Add(movableSquare);
            var rootController = rootSquare.GetComponent<NoteSquareMovableController>();
            rootController.StartingYpos = rootSquare.transform.localPosition.y;
            rootController.note = rootNotes[i].Item1;
            rootController.squareColour = Persistent.noteColours[rootNotes[i].Item1.Substring(0, rootNotes[i].Item1.Length - 1)];
            rootController.waitTime = waitTime;
            rootController.Show();
            var movableController = movableSquare.GetComponent<NoteSquareMovableController>();
            movableController.StartingYpos = movableSquare.transform.localPosition.y;
            movableController.note = options[i].Item1;
            movableController.squareColour = Persistent.noteColours[options[i].Item1.Substring(0, options[i].Item1.Length - 1)];
            movableController.waitTime = waitTime;
            movableController.draggable = true;
            movableController.movableYpos = true;
            movableController.xRange = 200;
            movableController.Show();
            waitTime += 0.1f;
        }
    }

    protected override IEnumerator MoveObject(GameObject obj, Vector2 target, float time, float wait = 0, bool reset = false, bool destroy = false)
    {
        float resolution = time / 0.016f;
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
        if (destroy)
        {
            Destroy(obj);
        }
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
        StartCoroutine(MoveArrowLog(arrow, new Vector2(arrow.transform.localPosition.x, -200), 1f, true, false, waitTime: 0.2f));
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
