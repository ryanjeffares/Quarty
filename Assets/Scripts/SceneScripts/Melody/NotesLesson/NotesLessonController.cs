using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NotesLessonController : BaseManager
{
    [SerializeField] private GameObject nextButton, tryButton;
    [SerializeField] private Text introText, helpText;
    [SerializeField] private GameObject noteCirclesPrefab, emptyNoteCirclesPrefab, movableCirclePrefab;
    [SerializeField] private GameObject arrow;    

    [SerializeField] private AnimationCurve easeInOutCurve;
    [SerializeField] private AnimationCurve overshootCurve;

    private GameObject _noteCircles, _emptyNoteCircles;
    private List<string> _playedNotes, _correctNotes;
    private List<GameObject> _movableCircles;
    private bool _arrowMoving, _complete;
    private int _levelStage;

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
            {helpText, true},           
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true},
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true}
        };
        _correctNotes = new List<string>
        {
            "C", "D", "E", "F", "G", "A", "B", "C"
        };
        _playedNotes = new List<string>();
        _movableCircles = new List<GameObject>();
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 1f));
    }

    protected override void DestroyManager()
    {
        NoteCircleMovableController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        if (_levelStage < 3)
        {
            ++_levelStage;
            StartCoroutine(AdvanceLevelStage());   
        }
        else
        {
            Persistent.UpdateUserGlossary("Scale");
            Persistent.sceneToLoad = "NotesPuzzle";
            Persistent.goingHome = false;            
            SceneManager.LoadScene("LoadingScreen");
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
                        helpText.text = "Nice! Now try it on your own.";
                        StartCoroutine(FadeText(helpText, true, 0.5f, fadeOut:true, duration:2f));
                        StartCoroutine(StageThree());
                        _levelStage++;
                        break;
                    case 3:
                        _complete = true;
                        helpText.text = "Amazing! You can hear it again or move into the puzzle.";
                        StartCoroutine(FadeText(helpText, true, 0.5f));
                        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                        break;
                }
            }
            else
            {
                if(!_complete)
                {
                    helpText.text = "That didn't sound quite right! Have another try.";
                    StartCoroutine(FadeText(helpText, true, 0.5f, fadeOut: true, duration: 2f));
                    StartCoroutine(MoveMovableCircles(0.5f));
                }
            }
            _playedNotes.Clear();
        }
    }

    private void TryButtonCallback(GameObject g)
    {
        if(_levelStage == 2)
        {
            StartCoroutine(MoveObject(arrow, new Vector2(260, -250), 2f));
        }
        else if (_levelStage == 3)
        {
            StartCoroutine(MoveObject(arrow, new Vector2(260, -200), 2f));
        }
    }

    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1: 
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                float counter = 0f;
                while (counter <= 2f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    counter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "See how the numbers increase with the notes' letters and their pitch.\n \nWhen we get up to G, the next note is A but at a higher pitch.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                counter = 0f;
                while (counter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    counter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                StartCoroutine(SpawnSampleCirclesInitial());
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 6f));
                break;
            case 2:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeText(nextButton.transform.GetChild(0).GetComponent<Text>(), false, 0.5f)); ;
                StartCoroutine(MoveCircles(new Vector2(0, -100), 1.5f));
                StartCoroutine(MoveObjectLog(arrow, new Vector2(-260, -250), 1.5f));
                StartCoroutine(RotateArrow360(1.5f));
                counter = 0f;
                while (counter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    counter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "Drag the notes into the correct place and press \"Go\" to see if you got it right!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _emptyNoteCircles = Instantiate(emptyNoteCirclesPrefab, transform.GetChild(0));
                _emptyNoteCircles.transform.localPosition = new Vector3(0, -250);
                SpawnMovableCircles();
                counter = 0f;
                while (counter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    counter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f));
                break;
        }
    }

    private IEnumerator StageThree()
    {
        float counter = 0f;
        while (counter <= 1.2f)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            counter += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        StartCoroutine(MoveMovableCircles(1f));
        StartCoroutine(MoveObjectLog(tryButton, new Vector2(-260, -250), 1f));
        StartCoroutine(MoveObjectLog(_emptyNoteCircles, new Vector2(0, -200), 1f));
        StartCoroutine(MoveObjectLog(arrow, new Vector2(-260, -200), 1f));
        StartCoroutine(MoveCircles(new Vector2(-500, -100), 1f, true));
    }
    
    private IEnumerator MoveMovableCircles(float time)
    {
        int[] localXs = {-125, -75, -25, 25, 75, 125};
        List<int> indexes = new List<int>{0, 1, 2, 3, 4, 5}; 
        indexes.Shuffle();

        Dictionary<GameObject, Vector3> startPositions = new Dictionary<GameObject, Vector3>();
        Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();
        Dictionary<GameObject, Tuple<float, float>> diffs = new Dictionary<GameObject, Tuple<float, float>>();

        foreach(var (circle, idx) in _movableCircles.Where(c => !c.GetComponent<NoteCircleMovableController>().note.Contains("C")).WithIndex())
        {
            startPositions.Add(circle, circle.transform.localPosition);
            targetPositions.Add(circle, new Vector3(localXs[indexes[idx]], -60));
            diffs.Add(circle, new Tuple<float, float>
                (targetPositions[circle].x - startPositions[circle].x, targetPositions[circle].y - startPositions[circle].y));
        }
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }            
            foreach (var circle in _movableCircles.Where(c => !c.GetComponent<NoteCircleMovableController>().note.Contains("C")))
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

    private IEnumerator SpawnSampleCirclesInitial()
    {
        float counter = 0f;
        while (counter <= 1f)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }

            counter += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        _noteCircles = Instantiate(noteCirclesPrefab, transform.GetChild(0));
        _noteCircles.transform.localPosition = new Vector3(0, -200);
        _noteCircles.GetComponent<NoteCirclesScaleController>().Show(false);   
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
        yield return new WaitForSeconds(1f);
        StartCoroutine(MoveObject(arrow, new Vector2(260, -200), 2f));
    }

    private void SpawnMovableCircles()
    {
        string[] notes = {"C2", "D2", "E2", "F2", "G2", "A2", "B2", "C3"};
        int[] localXs = {-125, -75, -25, 25, 75, 125};
        List<int> indexes = new List<int>{0, 1, 2, 3, 4, 5};
        indexes.Shuffle();
        float wait = 0f;
        for (int i = 0; i < 8; i++)
        {
            _movableCircles.Add(Instantiate(movableCirclePrefab, _emptyNoteCircles.transform));
            _movableCircles[i].GetComponent<NoteCircleMovableController>().note = notes[i];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().waitTime = wait;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().draggable = true;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().circleColour = Persistent.noteColours[notes[i].Substring(0, 1)];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().curve = overshootCurve;            
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

    private IEnumerator MoveCircles(Vector2 target, float time, bool destroy = false)
    {
        arrow.GetComponent<BoxCollider2D>().enabled = false;
        float targetX = target.x;
        float targetY = target.y;
        var startPos = _noteCircles.transform.localPosition;
        float yDiff = targetY - startPos.y;
        float xDiff = targetX - startPos.x;
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            var pos = _noteCircles.transform.localPosition;
            pos.y = startPos.y + (easeInOutCurve.Evaluate(timeCounter / time) * yDiff);
            pos.x = startPos.x + (easeInOutCurve.Evaluate(timeCounter / time) * xDiff);
            _noteCircles.transform.localPosition = pos;
            timeCounter += Time.deltaTime;
            yield return null;
        }
        arrow.GetComponent<BoxCollider2D>().enabled = true;
        if (destroy)
        {
            Destroy(_noteCircles);
        }
    }

    protected override IEnumerator MoveObject(GameObject obj, Vector2 target, float time, float wait = 0f, bool disableTrigger = false, bool destroy = false)
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

        if (obj == arrow && buttonCallbackLookup.ContainsKey(tryButton))
        {
            buttonCallbackLookup.Remove(tryButton);
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
                if(_levelStage > 1)
                {
                    StartCoroutine(MoveObjectLog(arrow, new Vector2(-260, arrow.transform.localPosition.y), 1f,
                        disableTrigger:true, reset:true));
                }
            }
        }
    }

    protected override IEnumerator MoveObjectLog(GameObject obj, Vector2 target, float time, float wait = 0f, 
        bool disableTrigger = false, 
        bool reset = false, 
        bool destroy = false)
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
        if (reset)
        {
            _playedNotes.Clear();
        }
        if (obj == arrow)
        {
            _arrowMoving = false;
            if (!buttonCallbackLookup.ContainsKey(tryButton))
            {
                buttonCallbackLookup.Add(tryButton, TryButtonCallback);
            }
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
    }
}
