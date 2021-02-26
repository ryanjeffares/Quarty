using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TonesAndSemitonesPuzzleController : BaseManager
{
    [Header("Prefabs")]
    [SerializeField] private GameObject emptyScalePrefab;
    [SerializeField] private GameObject movableCirclePrefab;
    [SerializeField] private GameObject starPrefab;
    [Header("Texts")]
    [SerializeField] private Text introText;
    [SerializeField] private Text helpText;
    [SerializeField] private Text niceText;
    [SerializeField] private Text allNotesText;
    [SerializeField] private Text scoreCounter;
    [SerializeField] private Text timeText;
    [Header("Game Objects")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject tryButton;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject arrow;
    [SerializeField] private Slider timeSlider;
    [Header("Curves")]
    [SerializeField] private AnimationCurve easeInOutCurve;
    [SerializeField] private AnimationCurve overshootCurve;
    [SerializeField] private AnimationCurve overshootOutCurve;
    [Header("Clips")]
    [SerializeField] private List<string> clips;

    public int timer;
    private int _levelStage, _scalesDone;
    private int[] allowedStartNoteIndexes;
    private bool _playing, _arrowMoving, _success;
    private string previousRoot = "";
    private GameObject _emptyScale;
    private List<GameObject> _stars, _movableCircles;
    private List<string> _fullOctave, _correctOrder, _playedNotes;
    private System.Random _random = new System.Random();

    protected override void OnAwake()
    {
        NoteCircleMovableController.NotePlayed += NotePlayedCallback;
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {helpText, true },
            {niceText, true },
            {allNotesText, true },
            {scoreCounter, true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true },
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true },
            {retryButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
        buttonCallbackLookup = new Dictionary<GameObject, System.Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {tryButton, TryButtonCallback }
        };
        fullCallbackLookup = new Dictionary<GameObject, System.Action<GameObject>>
        {
            {nextButton, NextButtonCallback},
            {tryButton, TryButtonCallback},
            {retryButton, RetryButtonCallback}
        };
        _fullOctave = new List<string>
        {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B", "C"
        };
        allowedStartNoteIndexes = new int[]{0, 2, 4, 7, 9, 11};
        _movableCircles = new List<GameObject>();
        _stars = new List<GameObject>();
        _playedNotes = new List<string>();
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeText(allNotesText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));        
        _emptyScale = Instantiate(emptyScalePrefab, mainContainer.transform);
        _emptyScale.transform.localPosition = new Vector3(0, 100);
        StartCoroutine(SpawnMovableCircles());
        StartCoroutine(FadeSlider(0.5f));
    }

    protected override void DestroyManager()
    {
        NoteCircleMovableController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        if (!_success)
        {
            ++_levelStage;
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.sceneToLoad = "MajorScale";
            Persistent.goingHome = false;            
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void TryButtonCallback(GameObject g)
    {
        if (_arrowMoving || !_playing) return;
        StartCoroutine(MoveArrow(new Vector2(260, 100), 2f));
    }

    private void RetryButtonCallback(GameObject g)
    {
        _scalesDone = 0;
        scoreCounter.text = "Scales Done: 0";
        timeSlider.value = timer;
        foreach (var circle in _movableCircles)
        {
            Destroy(circle);
        }

        int index = 0;
        foreach (var star in _stars)
        {
            StartCoroutine(FadeStar(star, overshootOutCurve, false, 0.3f, wait:0.2f * index));
            index++;
        }
        _movableCircles.Clear();
        StartCoroutine(FadeText(introText, false, 0.5f));
        StartCoroutine(FadeButtonText(retryButton, false, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
        StartCoroutine(DecreaseTimer());
        StartCoroutine(SpawnMovableCircles());
    }

    private void NotePlayedCallback(string note)
    {
        if (!_playing) return;
        _playedNotes.Add(note);
        if(_playedNotes.Count == 8)
        {
            if (_playedNotes.SequenceEqual(_correctOrder))
            {
                _scalesDone++;
                scoreCounter.text = "Scales Done: " + _scalesDone;
                if (_playing)
                {
                    niceText.text = "Nice!";
                    niceText.color = new Color(0.32f, 0.57f, 0.47f);
                    StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.3f, true));
                    StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait:1f));
                }
            }
            else
            {
                if (_playing)
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
            StartCoroutine(SpawnMovableCircles());
        }
    }

    private void TimerEnd()
    {
        if (_scalesDone > 0)
        {
            _stars = new List<GameObject>();
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
            StartCoroutine(FadeText(introText, true, 0.5f));            
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
            StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            List<Vector2> starPositions = new List<Vector2>();
            switch (stars)
            {
                case 1:
                    starPositions.Add(new Vector2(0, 360));
                    break;
                case 2:
                    starPositions.Add(new Vector2(-50, 360));
                    starPositions.Add(new Vector2(50, 360));
                    break;
                case 3:
                    starPositions.Add(new Vector2(-70, 360));
                    starPositions.Add(new Vector2(0, 360));
                    starPositions.Add(new Vector2(70, 360));
                    break;
            }
            _stars = new List<GameObject>();
            for (int i = 0; i < stars; i++)
            {
                _stars.Add(Instantiate(starPrefab, mainContainer.transform));
                _stars[i].transform.localPosition = starPositions[i];
                _stars[i].GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                StartCoroutine(FadeStar(_stars[i], overshootCurve, true, 0.3f, wait:0.2f * i));
            }
            if (stars > Persistent.melodyLessons.scores["Tones And Semitones"])
            {
                Persistent.melodyLessons.lessons["Major Scale"] = true;
                Persistent.UpdateLessonAvailability("Melody");
                Persistent.melodyLessons.scores["Tones And Semitones"] = stars;
            }
        }
        else
        {
            introText.text = "Looks like you didn't fill in any scales correctly. Press retry to try again.";
            StartCoroutine(FadeText(introText, true, 0.5f));            
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
        }
    }

    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f));
                StartCoroutine(FadeText(scoreCounter, true, 0.5f));
                StartCoroutine(DecreaseTimer());
                foreach(var circle in _movableCircles)
                {
                    circle.GetComponent<NoteCircleMovableController>().draggable = true;
                }
                break;
        }
        yield return null;
    }

    private IEnumerator SpawnMovableCircles(float waitTime = 0f)
    {
        if(waitTime > 0)
        {
            float waitInterval = waitTime / 100f;
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
        int startIndex = allowedStartNoteIndexes[_random.Next(allowedStartNoteIndexes.Length)];
        var startNote = _fullOctave[startIndex];
        if (previousRoot != "")
        {
            while (startNote == previousRoot)
            {
                startIndex = allowedStartNoteIndexes[_random.Next(allowedStartNoteIndexes.Length)];
                startNote = _fullOctave[startIndex];
            }
        }
        previousRoot = startNote;
        int[] localXs = { -125, -75, -25, 25, 75, 125 };
        List<int> indexes = new List<int> { 0, 1, 2, 3, 4, 5 };
        indexes.Shuffle();
        _correctOrder = new List<string>();
        float wait = 0f;
        for(int i = 0; i < 8; i++)
        {
            var currentNote = _fullOctave[(startIndex + Persistent.majorScale[i]) % 12];
            _correctOrder.Add(currentNote);
            _movableCircles.Add(Instantiate(movableCirclePrefab, _emptyScale.transform));            
            _movableCircles[i].GetComponent<NoteCircleMovableController>().note = clips[startIndex + Persistent.majorScale[i]];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().waitTime = wait;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().circleColour = Persistent.noteColours[_fullOctave[(startIndex + Persistent.majorScale[i]) % 12]];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().draggable = _playing;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().curve = overshootCurve;
            //_movableCircles[i].GetComponent<AudioSource>().clip = clips[startIndex + Persistent.majorScale[i]];
            switch (i)
            {
                case 0:
                    _movableCircles[i].transform.localPosition = new Vector3(-175, 0);
                    break;
                case 7:
                    _movableCircles[i].transform.localPosition = new Vector3(175, 0);
                    _movableCircles[i].GetComponent<NoteCircleMovableController>().octaveUp = true;
                    break;
                default:
                    _movableCircles[i].transform.localPosition = new Vector3(localXs[indexes[i - 1]], -60);
                    break;
            }
            _movableCircles[i].GetComponent<NoteCircleMovableController>().Show();
            wait += 0.1f;
        }
    }    

    private IEnumerator FadeSlider(float time)
    {
        float resolution = time / 0.016f;
        float timeCounter = 0f;
        float interval = time / resolution;
        var startScale = timeSlider.transform.localScale;
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
            var sc = timeSlider.transform.localScale;
            sc.x = startScale.x + overshootCurve.Evaluate(timeCounter / time);
            sc.y = startScale.y + overshootCurve.Evaluate(timeCounter / time);
            timeSlider.transform.localScale = sc;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator DecreaseTimer()
    {
        _playing = true;
        float timeCounter = 0f;      
        while (timeCounter <= timer)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }            
            float remaining = timer - timeCounter;
            timeSlider.value = remaining;
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
                timeText.text = "Time remaining: " + ((int)(timer - timeCounter));
            }

            timeSlider.fillRect.GetComponent<Image>().color = Color.Lerp(Persistent.rainbowColours[3], Persistent.rainbowColours[0], timeCounter / timer);
            timeCounter += Time.deltaTime;            
            yield return new WaitForSeconds(Time.deltaTime);
        }
        _playing = false;
        foreach (var circle in _movableCircles)
        {
            circle.GetComponent<NoteCircleMovableController>().draggable = false;
        }
        timeText.text = "Time Remaining: 0";
        TimerEnd();
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
        StartCoroutine(MoveArrowLog(new Vector2(-260, arrow.transform.localPosition.y), 1f, true, true));
    }

    private IEnumerator MoveArrowLog(Vector2 target, float time, bool disableTrigger, bool reset)
    {
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
        float resolution = time / 0.016f;
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
        _arrowMoving = false;
    }
}
