using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinorScaleLessonController : BaseManager
{
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private Text introText, notesText, scaleText, helpText;
    [SerializeField] private GameObject nextButton, tryButton;
    [SerializeField] private GameObject arrow;
    [SerializeField] private AnimationCurve easeInOutCurve;
    [SerializeField] private GameObject squaresScalePrefab;

    private int _levelStage = 0;
    private GameObject _squareScale, _secondSquareScale;
    private bool _arrowMoving, _ready, _complete;
    private List<string> _playedNotes, _correctOrder;    

    protected override void OnAwake()
    {
        NoteSquareMovableController.NotePlayed += NotePlayedCallback;
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {tryButton, TryButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {notesText, true },
            {scaleText, true },
            {helpText, true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true },
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
        _playedNotes = new List<string>();
        _correctOrder = new List<string> { "A", "B", "C", "D", "E", "F", "G", "A" };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        introText.text = "Another one of the most commonly used scales is the Minor Scale.\n \nIts pattern is T, S, T, T, S, T, T.";
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeText(notesText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 3f));
    }

    protected override void DestroyManager()
    {
        NoteSquareMovableController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        if(_levelStage < 3)
        {
            ++_levelStage;
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.melodyLessons.lessons["Minor Intervals"] = true;
            Persistent.sceneToLoad = "MinorIntervals";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void TryButtonCallback(GameObject g)
    {
        if (_arrowMoving || !_ready) return;
        _playedNotes.Clear();
        StartCoroutine(MoveArrow(new Vector2(200, -430), 2f));
    }

    private void NotePlayedCallback(string note)
    {
        if (_complete) return;
        if (_playedNotes == null) _playedNotes = new List<string>();
        _playedNotes.Add(note);
        if(_playedNotes.Count == 8)
        {
            if (_playedNotes.SequenceEqual(_correctOrder))
            {
                switch (_levelStage)
                {
                    case 2:
                        helpText.text = "Nice job! Now lets see if you can do it by yourself with a different scale.";
                        StartCoroutine(FadeText(helpText, true, 0.5f, fadeOut: true, duration: 3f));
                        ++_levelStage;
                        _ready = false;
                        StartCoroutine(StageThree());
                        break;
                    case 3:
                        helpText.text = "Incredible! You can hear it again, or move into the next lesson.";
                        StartCoroutine(FadeText(helpText, true, 0.5f));
                        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                        _complete = true;
                        break;
                }
            }
            else
            {
                switch (_levelStage)
                {
                    case 2:
                        helpText.text = "That didn't sound quite right, make sure to slide out the notes that are not present in the A Minor Scale below.";
                        StartCoroutine(FadeText(helpText, true, 0.5f, fadeOut: true, duration: 3f));
                        break;
                    case 3:
                        helpText.text = "That didn't sound quite right, make sure to slide out the notes that are not present in the E Minor Scale below.";
                        StartCoroutine(FadeText(helpText, true, 0.5f, fadeOut: true, duration: 3f));
                        break;
                }
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
                while (timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
                introText.text = "You can start that pattern on any note and it will give you the Minor Scale of that note.\n \nStarting on A, for example, gives us the A Minor Scale.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeText(scaleText, true, 0.5f, 1f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 2f));
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
                introText.text = "Can you slide out of the way the notes that aren't in the A Minor Scale?";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _squareScale = Instantiate(squaresScalePrefab, mainContainer.transform);
                _squareScale.transform.localPosition = new Vector3(200, 200);
                _squareScale.GetComponent<NoteSquaresScaleController>().Show(true, new List<string> { "A1", "B1", "C2", "C#2", "D2", "E2", "F2", "G2", "G#2", "A2" }, squaresDraggableRight: false);
                _squareScale.GetComponent<NoteSquaresScaleController>().MakeDraggable(true);
                StartCoroutine(FadeInArrow(0.5f));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f));
                yield return new WaitForSeconds(0.5f);
                _ready = true;
                break;
        }
        yield return null;
    }

    private IEnumerator StageThree()
    {
        StartCoroutine(FadeText(introText, false, 0.5f));
        StartCoroutine(FadeText(scaleText, false, 0.5f));
        StartCoroutine(MoveObject(_squareScale, new Vector2(500, _squareScale.transform.localPosition.y), 1f, wait: 0.5f, destroy: true));
        yield return new WaitForSeconds(1);
        introText.text = "Try it with the E Minor scale. You have the notes at the bottom of the screen to help you.\n \nRemember the pattern: T, S, T, T, S, T, T.";
        StartCoroutine(FadeText(introText, true, 0.5f));
        _secondSquareScale = Instantiate(squaresScalePrefab, mainContainer.transform);
        _secondSquareScale.transform.localPosition = new Vector3(200, 200);        
        _secondSquareScale.GetComponent<NoteSquaresScaleController>().Show(true, new List<string> { "E1", "F#1", "G1", "G#1", "A1", "B1", "C2", "C#2", "D2", "E2" }, squaresDraggableRight: false);
        _secondSquareScale.GetComponent<NoteSquaresScaleController>().MakeDraggable(true);
        _correctOrder = new List<string> { "E", "F#", "G", "A", "B", "C", "D", "E" };
        yield return new WaitForSeconds(1);
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
        StartCoroutine(MoveArrowLog(new Vector2(arrow.transform.localPosition.x, 430), 1f, true, true, 0.2f));
    }

    private IEnumerator MoveArrowLog(Vector2 target, float time, bool disableTrigger, bool reset, float waitTime = 0f)
    {        
        if(waitTime >= 0)
        {
            float waitCounter = 0f;
            while(waitCounter <= waitTime)
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

    private IEnumerator FadeInArrow(float time)
    {
        float alpha;
        float timeCounter = 0f;
        while(timeCounter <= time)
        {
            alpha = Mathf.Lerp(0f, 1f, timeCounter / time);
            arrow.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }
}
