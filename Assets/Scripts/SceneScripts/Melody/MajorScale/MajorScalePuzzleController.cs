using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MajorScalePuzzleController : BaseManager
{
    [Header("Prefabs")]
    [SerializeField] private GameObject starPrefab;    
    [SerializeField] private GameObject squareScalePrefab;
    [Header("Texts")]
    [SerializeField] private Text introText;
    [SerializeField] private Text helpText;
    [SerializeField] private Text niceText;
    [SerializeField] private Text allNotesText;
    [SerializeField] private Text scoreCounter;
    [SerializeField] private Text timeText;
    [SerializeField] private Text hintText;
    [Header("Game Objects")]
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject tryButton;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject squareContainer;
    [SerializeField] private GameObject arrow;
    [SerializeField] private Slider timeSlider;
    [Header("Curves")]
    [SerializeField] private AnimationCurve easeInOutCurve;
    [SerializeField] private AnimationCurve overshootCurve;
    [SerializeField] private AnimationCurve overshootOutCurve;
    
    public int timer;

    private GameObject _scale;
    private GameObject Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _controller = _scale.GetComponent<NoteSquaresScaleController>();
        }
    }
    private NoteSquaresScaleController _controller;
    private readonly System.Random _random = new System.Random();
    private List<List<string>> _noteCombinations;
    private List<string> _correctOrder, _playedNotes;
    private List<GameObject> _stars;
    private Dictionary<string, string[]> _wrongNotes;
    private int _levelStage, _scalesDone;
    private bool _playing, _success, _arrowMoving;

    protected override void OnAwake()
    {
        NoteSquareMovableController.NotePlayed += NotePlayedCallback;
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {tryButton, TryButtonCallback },
            {retryButton, RetryButtonCallback },
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {helpText, true },
            {niceText, true },
            {allNotesText, true },
            {scoreCounter, true },
            {hintText, true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true },
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true },
            {retryButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
        _noteCombinations = new List<List<string>>
        {
            new List<string>{"C2", "C#2", "D2", "E2", "F2", "F#2", "G2", "A2", "B2", "C3" },
            new List<string>{"D1", "F1", "E1", "F#1", "G1", "A1", "B1", "C2", "C#2", "D2" },
            new List<string>{"E1", "F#1", "G1", "G#1", "A1", "A#1", "B1", "C#2", "D#2", "E2" },            
            new List<string>{"G1", "A1", "B1", "C2", "C#2", "D2", "E2", "F2", "F#2", "G2" },
            new List<string>{"A1", "B1", "C2", "C#2", "D2", "E2", "F2", "F#2", "G#2", "A2" },            
            new List<string>{"B1", "C2", "C#2", "D#2", "E2", "F#2", "G#2", "A2", "A#2", "B2" }
        };
        _wrongNotes = new Dictionary<string, string[]>
        {
            {"C", new[]{"F#2", "C#2" } },
            {"D", new[]{"C2", "F1" } },
            {"E", new[]{"G1", "A#1" } },            
            {"G", new[]{"C#2", "F2" } },
            {"A", new[]{"F2", "C2" } },
            {"B", new[]{"C2", "A2" } }
        };
        _stars = new List<GameObject>();
        _playedNotes = new List<string>();
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeText(allNotesText, true, 0.5f));        
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
        StartCoroutine(FadeSlider(0.5f));
        StartCoroutine(SpawnMovableSquares());
    }

    protected override void DestroyManager()
    {
        NoteSquareMovableController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if (_success)
        {
            Persistent.sceneToLoad = "MinorScale";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
        else
        {
            StartCoroutine(AdvanceLevelStage());
        }        
    }    

    private void TryButtonCallback(GameObject g)
    {
        if (!_playing || _arrowMoving) return;
        StartCoroutine(MoveArrow(new Vector2(200, -480), 2f));
    }

    private void RetryButtonCallback(GameObject g)
    {
        _scalesDone = 0;
        scoreCounter.text = "Scales Done: 0";
        timeSlider.value = timer;
        _controller.DestroySquares();
        foreach(var (s, index) in _stars.WithIndex())
        {
            StartCoroutine(FadeInObjectScale(s, overshootOutCurve, false, 0.3f, wait: 0.2f * index));
        }        
        StartCoroutine(FadeText(introText, false, 0.5f));
        StartCoroutine(FadeButtonText(retryButton, false, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
        StartCoroutine(DecreaseTimer());
        StartCoroutine(SpawnMovableSquares());
    }

    private void NotePlayedCallback(string note)
    {
        if (!_playing) return;
        _playedNotes.Add(note);
        if (_playedNotes.Count == 8)
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
                    StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait: 1f));
                }
            }
            else
            {
                if (_playing)
                {
                    niceText.text = "Oops!";
                    niceText.color = new Color(0.76f, 0.43f, 0.41f);
                    StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.3f, true));
                    StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait: 1f));
                }
            }
            _controller.DestroySquares();
            StartCoroutine(SpawnMovableSquares());
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
            introText.text = $"Awesome {Persistent.userName}! You completed {_scalesDone} scales and got {stars} {readout}. You can try again, or move into the next lesson.";
            StartCoroutine(FadeText(introText, true, 0.5f));
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
            StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            List<Vector2> starPositions = new List<Vector2>();
            switch (stars)
            {
                case 1:
                    starPositions.Add(new Vector2(0, -177));
                    break;
                case 2:
                    starPositions.Add(new Vector2(-50, -177));
                    starPositions.Add(new Vector2(50, -177));
                    break;
                case 3:
                    starPositions.Add(new Vector2(-70, -177));
                    starPositions.Add(new Vector2(0, -177));
                    starPositions.Add(new Vector2(70, -177));
                    break;
            }
            _stars = new List<GameObject>();
            for (int i = 0; i < stars; i++)
            {
                _stars.Add(Instantiate(starPrefab, mainContainer.transform));
                _stars[i].transform.localPosition = starPositions[i];
                _stars[i].GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                StartCoroutine(FadeInObjectScale(_stars[i], overshootCurve, true, 0.3f, wait: 0.2f * i));
            }
            if (stars > Persistent.melodyLessons.scores["Major Scale"])
            {
                Persistent.melodyLessons.lessons["Minor Scale"] = true;
                Persistent.melodyLessons.scores["Major Scale"] = stars;
                Persistent.UpdateLessonAvailability("Melody");                
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
                StartCoroutine(FadeText(hintText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f));
                StartCoroutine(FadeText(scoreCounter, true, 0.5f));
                StartCoroutine(DecreaseTimer());
                break;
        }
        yield return null;
    }

    private IEnumerator SpawnMovableSquares(float waitTime = 0f)
    {
        if (waitTime > 0)
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
        _correctOrder = new List<string>();               
        var nextCombo = _noteCombinations[_random.Next(_noteCombinations.Count)];
        var root = nextCombo[0].Substring(0, 1);                                
        Scale = Instantiate(squareScalePrefab, squareContainer.transform);        
        _controller.Show(squaresDraggableRight: false, useCustomNotes: true, customNotes: nextCombo);
        _controller.MakeDraggable(true);
        nextCombo.ForEach(n =>
        {
            if (!_wrongNotes[root].Contains(n))
            {
                _correctOrder.Add(n.Substring(0, n.Length - 1));
            }
        });

        //for (int i = 0; i < nextCombo.Count; i++)
        //{
        //    if(nextCombo[i] != _wrongNotes[root])
        //    {
        //        _correctOrder.Add(nextCombo[i].Substring(0, nextCombo[i].Length - 1));                
        //    }
        //    _movableSquares.Add(Instantiate(squarePrefab, squareContainer.transform));
        //    var controller = _movableSquares[i].GetComponent<NoteSquareMovableController>();
        //    controller.note = nextCombo[i];
        //    controller.waitTime = wait;
        //    controller.squareColour = Persistent.noteColours[nextCombo[i].Substring(0, nextCombo[i].Length - 1)];
        //    controller.draggable = !nextCombo[i].Contains(root);
        //    controller.canMoveRight = false;
        //    _movableSquares[i].transform.localPosition = new Vector3(nextCombo[i].Contains(root) ? 0 : -100, y);
        //    controller.StartingYpos = y;
        //    controller.Show();
        //    y -= 100;
        //    wait += 0.1f;
        //}
    }

    private IEnumerator DecreaseTimer()
    {
        _playing = true;
        float timeCounter = 0f;
        while(timeCounter <= timer)
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
            yield return null;
        }
        _playing = false;
        _controller.MakeDraggable(false);
        timeText.text = "Time Remaining: 0";
        TimerEnd();
    }

    private IEnumerator FadeSlider(float time)
    {        
        float timeCounter = 0f;
        timeSlider.transform.localScale = Vector3.zero;
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
            sc.x = 0.8f * + overshootCurve.Evaluate(timeCounter / time);
            sc.y = 0.8f * + overshootCurve.Evaluate(timeCounter / time);
            timeSlider.transform.localScale = sc;
            timeCounter += Time.deltaTime;
            yield return null;
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
            timeCounter += Time.deltaTime; ;
            yield return null;
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        _arrowMoving = false;
        StartCoroutine(MoveArrowLog(new Vector2(arrow.transform.localPosition.x, 400), 1f, true, true));
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
            timeCounter += Time.deltaTime; ;
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
