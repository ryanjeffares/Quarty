using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NotesPuzzleController : BaseManager
{
    [Header("Prefabs")]
    [SerializeField] private GameObject emptyNoteCirclesPrefab;
    [SerializeField] private GameObject movableCirclePrefab;
    [SerializeField] private GameObject starPrefab;
    [Header("Objects")]
    [SerializeField] private Slider timeRemaining;
    [SerializeField] private GameObject nextButton, tryButton, arrow, retryButton, mainHolder;
    [Header("Texts")] 
    [SerializeField] private Text introText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text niceText;
    [SerializeField] private Text scoreCounter;
    [Header("Curves")]
    [SerializeField] private AnimationCurve overshootCurve;
    [SerializeField] private AnimationCurve overshootOutCurve;
    [SerializeField] private AnimationCurve easeInOutCurve;
    [Header("Lists")]
    [SerializeField] private List<AudioClip> clips;

    public int timer;
    
    private GameObject _emptyNoteCircles;
    private List<GameObject> _movableCircles, _stars;
    private List<string> _correctOrder, _playedNotes;
    private int _levelStage;
    private int _notesPlayed;
    private int _scalesDone;
    private bool _arrowMoving;
    private bool _playing;
    private bool _success;
    private string _previousRoot = "C";
    
    protected override void OnAwake()
    {
        NoteCircleMovableController.NotePlayed += NotePlayedCallback;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback},
            {tryButton, TryButtonCallback},
        };
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback},
            {tryButton, TryButtonCallback},
            {retryButton, RetryButtonCallback}
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true},
            {niceText, true},
            {scoreCounter, true},
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true},
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true},
            {retryButton.transform.GetChild(0).GetComponent<Text>(), true}
        };
        _playedNotes = new List<string>();
        _stars = new List<GameObject>();
        timeRemaining.fillRect.GetComponent<Image>().color = Persistent.rainbowColours[3];
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
        StartCoroutine(FadeSlider(0.5f));
        _emptyNoteCircles = Instantiate(emptyNoteCirclesPrefab, mainHolder.transform); 
        _emptyNoteCircles.transform.localPosition = new Vector3(0, -200);
        StartCoroutine(SpawnMovableCircles());
    }

    protected override void DestroyManager()
    {
        NoteCircleMovableController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        if(!_success)
        {
            ++_levelStage;
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.sceneToLoad = "TonesAndSemitones";
            Persistent.goingHome = false;
            Persistent.melodyLessons.lessons["Tones And Semitones"] = true;
            Persistent.UpdateLessonAvailability("Melody");
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void TryButtonCallback(GameObject g)
    {
        if (_arrowMoving || !_playing) return;
        StartCoroutine(MoveArrow(new Vector2(215, -200), 2f));
    }

    private void RetryButtonCallback(GameObject g)
    {
        _scalesDone = 0;
        scoreCounter.text = "Scales Done: 0";
        timeRemaining.value = 45;
        foreach (var circle in _movableCircles)
        {
            Destroy(circle);
        }

        int index = 0;
        foreach (var star in _stars)
        {
            StartCoroutine(FadeStar(star, overshootOutCurve, false, 0.3f, 0.2f * index));            
            index++;
        }
        _stars.Clear();
        _movableCircles.Clear();
        StartCoroutine(FadeText(introText, false, 0.5f));
        StartCoroutine(FadeButtonText(retryButton, false, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
        StartCoroutine(DecreaseTimer());
        StartCoroutine(SpawnMovableCircles());
    }
    
    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f));   
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f));
                StartCoroutine(FadeText(scoreCounter, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(DecreaseTimer());
                foreach (var circle in _movableCircles)
                {
                    circle.GetComponent<NoteCircleMovableController>().draggable = true;
                }
                break;
            default: break;
        }

        yield return null;
    }

    private IEnumerator SpawnMovableCircles(float waitTime = 0f)
    {
        float resolution = waitTime / 0.016f;
        if (waitTime > 0)
        {
            float waitInterval = waitTime / resolution;
            float waitCounter = 0f;
            while (waitCounter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            } 
        }
        _movableCircles = new List<GameObject>();
        List<string> notes = new List<string>{"C", "D", "E", "F", "G", "A", "B", "C", "D", "E", "F", "G", "A", "B"};
        List<string> notesToShuffle = new List<string>{"C", "D", "E", "F", "G", "A", "B"};
        int[] localXs = {-125, -75, -25, 25, 75, 125};
        List<int> indexes = new List<int>{0, 1, 2, 3, 4, 5};
        indexes.Shuffle();
        while(notesToShuffle[0] == _previousRoot)
        {
            notesToShuffle.Shuffle();
        }
        _previousRoot = notesToShuffle[0];
        float wait = 0f;
        var startNote = notesToShuffle[0]; // random start note...
        int idx = notes.IndexOf(notes.First(s => s ==startNote));
        _correctOrder = new List<string>();
        for (int i = 0; i < 8; i++)
        {
            _movableCircles.Add(Instantiate(movableCirclePrefab, _emptyNoteCircles.transform));
            _correctOrder.Add(notes[idx + i]);
            _movableCircles[i].GetComponent<NoteCircleMovableController>().note = notes[idx + i];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().waitTime = wait;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().circleColour = Persistent.noteColours[notesToShuffle[i % 7]];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().draggable = _playing;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().curve = overshootCurve;
            _movableCircles[i].GetComponent<AudioSource>().clip = clips[i + idx];
            switch (i)
            {
                case 0: _movableCircles[i].transform.localPosition = new Vector3(-175, 0);
                    break;
                case 7: _movableCircles[i].transform.localPosition = new Vector3(175, 0);
                    _movableCircles[i].GetComponent<NoteCircleMovableController>().octaveUp = true;
                    break;
                default: _movableCircles[i].transform.localPosition = new Vector3(localXs[indexes[i - 1]], -60);
                    break;
            }
            _movableCircles[i].GetComponent<NoteCircleMovableController>().Show();
            wait += 0.1f;
        }
    }

    private void TimerEnd()
    {
        if (_scalesDone > 0)
        {
            _success = true;
            int stars;
            if (_scalesDone < 3)
            {
                stars = 1;
            }
            else if (_scalesDone < 6)
            {
                stars = 2;
            }
            else
            {
                stars = 3;
            }
            string readout = stars > 1 ? "stars" : "star";
            introText.text = $"Awesome! You completed {_scalesDone} scales and got {stars} {readout}. You can try again, or move into the next lesson.";
            retryButton.transform.localPosition = new Vector3(0, -100);
            StartCoroutine(FadeText(introText, true, 0.5f));
            StartCoroutine(FadeText(scoreCounter, false, 0.5f));
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
            StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            List<Vector2> starPositions = new List<Vector2>();
            switch (stars)
            {
                case 1: starPositions.Add(new Vector2(0, 30));
                    break;
                case 2: starPositions.Add(new Vector2(-50, 30));
                    starPositions.Add(new Vector2(50, 30));
                    break;
                case 3: starPositions.Add(new Vector2(-70, 30));
                    starPositions.Add(new Vector2(0, 30));
                    starPositions.Add(new Vector2(70, 30));
                    break;
            }

            for (int i = 0; i < stars; i++)
            {
                _stars.Add(Instantiate(starPrefab, mainHolder.transform));
                _stars[i].transform.localPosition = starPositions[i];
                _stars[i].GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                StartCoroutine(FadeStar(_stars[i], overshootCurve, true, 0.3f, wait:(0.2f * i)));
            }
        }
        else
        {
            introText.text = "Looks like you didn't fill in any scales correctly. Press retry to try again.";
            StartCoroutine(FadeText(introText, true, 0.5f));
            retryButton.transform.localPosition = new Vector3(0, -100);
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
        }
    }
    
    private void NotePlayedCallback(string note)
    {
        if (!_playing) return;
        _playedNotes.Add(note);
        _notesPlayed++;
        if (_notesPlayed == 8)
        {
            if (_playedNotes.SequenceEqual(_correctOrder))
            {
                _scalesDone++;
                scoreCounter.text = "Scales Done: " + _scalesDone;
                if(_playing)
                {
                    niceText.text = "Nice!";
                    niceText.color = new Color(0.32f, 0.57f, 0.47f);
                    StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.3f, true));
                    StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait:1f));
                }
            }
            else
            {
                if(_playing)
                {
                    niceText.text = "Oops!";
                    niceText.color = new Color(0.76f, 0.43f, 0.41f);
                    StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.3f, true));
                    StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait:1f));
                }
            }
            foreach (var circle in _movableCircles)
            {
                StartCoroutine(circle.GetComponent<NoteCircleMovableController>().Destroy());
            }
            _movableCircles.Clear();
            StartCoroutine(SpawnMovableCircles(0.5f));
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
        _arrowMoving = false;
        StartCoroutine(MoveArrowLog(new Vector2(-215, arrow.transform.localPosition.y), 1f, true, true));
    }

    private IEnumerator MoveArrowLog(Vector2 target, float time, bool disableTrigger, bool reset)
    {
        yield return new WaitUntil(() => !_arrowMoving);
        _arrowMoving = true;
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
            _notesPlayed = 0;
        }
        _arrowMoving = false;
    }
    
    private IEnumerator DecreaseTimer()
    {
        _playing = true;
        float timeCounter = 0f;
        float resolution = 1000f;
        float interval = timer / resolution;
        timeRemaining.fillRect.GetComponent<Image>().color = Persistent.rainbowColours[3];
        while (timeCounter <= timer)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            float remaining = timer - timeCounter;
            timeRemaining.value = remaining;
            if (remaining < 5f)
            {
                if (remaining < 2f)
                {
                    var t = remaining.ToString().Substring(0, 4);
                    timeText.text = "Time remaining: " + t;
                }
                else
                {
                    var t = remaining.ToString().Substring(0, 3);
                    timeText.text = "Time remaining: " + t;
                }
            }
            else
            {
                timeText.text = "Time remaining: " + ((int) (timer - timeCounter));   
            }

            timeRemaining.fillRect.GetComponent<Image>().color = Color.Lerp(Persistent.rainbowColours[3], Persistent.rainbowColours[0], timeCounter / timer);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        _playing = false;
        foreach (var circle in _movableCircles)
        {
            circle.GetComponent<NoteCircleMovableController>().draggable = false;
        }
        timeText.text = "Time Remaining: 0";
        TimerEnd();
    }
    
    private IEnumerator FadeSlider(float time)
    {
        float resolution = time / 0.016f;
        float timeCounter = 0f;
        float interval = time / resolution;
        var startScale = timeRemaining.transform.localScale;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            if (_levelStage == 0)
            {
                arrow.GetComponent<Image>().color = Color.Lerp(Color.clear, Color.black, timeCounter / time);
            }
            var sc = timeRemaining.transform.localScale;
            sc.x = startScale.x + overshootCurve.Evaluate(timeCounter / time);
            sc.y = startScale.y + overshootCurve.Evaluate(timeCounter / time);
            timeRemaining.transform.localScale = sc;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }
}
