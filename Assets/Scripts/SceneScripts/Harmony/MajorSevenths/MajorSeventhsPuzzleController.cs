using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MajorSeventhsPuzzleController : BaseManager
{
    [SerializeField] private GameObject mainContainer, keyContainer;
    [SerializeField] private GameObject nextButton, tryButton, retryButton;
    [SerializeField] private Text introText, notesText, timeText, niceText, scoreCounter;
    [SerializeField] private Slider timeSlider;
    [SerializeField] private AnimationCurve overshootCurve, overshootOutCurve;
    [SerializeField] private Image keyOutline;
    [SerializeField] private GameObject keyPrefab, draggableKeyPrefab, starPrefab;

    [SerializeField] private int allowedTime = 60;
    private int _levelStage, _correctChords;       
    private bool _playing, _ready;
    private List<string[]> _chords;
    private List<string> _correctSevenths;
    private readonly string[] _allNotes = 
    {
        "C2", "C#2", "D2", "D#2", "E2", "F2", "F#2", "G2", "G#2", "A2", "A#2", "B1"
    };
    private readonly System.Random _random = new System.Random();
    private List<GameObject> _keys, _draggableKeys, _stars;
    private PianoKeyDraggableController _draggableKeyToPlay;
    private List<string> _playedNotes, _correctOrder;

    protected override void OnAwake()
    {
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {tryButton, TryButtonCallback },
            {retryButton, RetryButtonCallback }
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {notesText, true },
            {timeText, true },
            {niceText, true },
            {scoreCounter, true },            
            {nextButton.GetComponentInChildren<Text>(), true },
            {tryButton.GetComponentInChildren<Text>(), true },
            {retryButton.GetComponentInChildren<Text>(), true }
        };
        _chords = new List<string[]>
        {
            new[]{"C2", "E2", "G2"},
            new[]{"D2", "F#2", "A2"},
            new[]{"E2", "G#2", "B2"},
            new[]{"F2", "A2", "C3"},
            new[]{"G2", "B2", "D3"},
            new[]{"A2", "C#3", "E3"},
            new[]{"B2", "D#3", "F#3"}
        };
        _correctSevenths = new List<string>
        {
            "B1", "C#2", "D#2", "E2", "F#2", "G#2", "A#2"
        };
        _stars = new List<GameObject>();
        PianoKeyDraggableController.Dropped += KeyDroppedCallback;
        PianoKeyDraggableController.NotePlayed += NotePlayedCallback;
        PianoKeyController.NotePlayed += NotePlayedCallback;
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeText(notesText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
        StartCoroutine(FadeInObjectScale(timeSlider.gameObject, overshootCurve, true, 0.5f));   
        
    }

    protected override void DestroyManager()
    {
        PianoKeyDraggableController.Dropped -= KeyDroppedCallback;
        PianoKeyDraggableController.NotePlayed -= NotePlayedCallback;
        PianoKeyController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if (_levelStage < 2)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.sceneToLoad = "MinorSeventhChords";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void RetryButtonCallback(GameObject g)
    {
        _keys.ForEach(k => StartCoroutine(k.GetComponent<PianoKeyController>().Dispose()));
        _draggableKeys.ForEach(k => StartCoroutine(k.GetComponent<PianoKeyDraggableController>().Dispose()));
        _correctChords = 0;        
        scoreCounter.text = "Chords Complete: 0";
        int index = 0;
        foreach (var star in _stars)
        {
            StartCoroutine(FadeInObjectScale(star, overshootOutCurve, false, 0.3f, 0.2f * index));
            index++;
        }
        _stars.Clear();
        StartCoroutine(FadeText(introText, false, 0.5f));
        StartCoroutine(FadeButtonText(retryButton, false, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
        timeSlider.value = allowedTime;
        StartCoroutine(DecreaseTimer());
        SpawnKeys();
    }

    private void TryButtonCallback(GameObject g)
    {
        if (!_ready || !_playing) return;
        _playedNotes = new List<string>();
        foreach(var k in _keys)
        {
            k.GetComponent<PianoKeyController>().ManualPlayNote();
        }
        _draggableKeyToPlay.ManualPlayNote();
        tryButton.GetComponentInChildren<Text>().color = new Color(0.196f, 0.196f, 0.196f, 0.5f);
        _ready = false;
    }

    private void KeyDroppedCallback(PianoKeyDraggableController kc, bool onTarget)
    {
        _ready = onTarget;
        tryButton.GetComponentInChildren<Text>().color = new Color(0.196f, 0.196f, 0.196f, onTarget ? 1 : 0.5f);
        _draggableKeyToPlay = onTarget ? kc : null;
    }

    private void NotePlayedCallback(string note)
    {
        if (!_playing) return;
        _playedNotes.Add(note);
        if (_playedNotes.Count != 4) return;
        if (_playedNotes.CheckElementsEqualUnordered(_correctOrder))
        {
            ++_correctChords;
            scoreCounter.text = $"Chords Complete: {_correctChords}";
            niceText.text = "Nice!";
            niceText.color = new Color(0.32f, 0.57f, 0.47f);
            StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.3f, true));
            StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait: 1f));
        }
        else
        {
            niceText.text = "Oops!";
            niceText.color = new Color(0.76f, 0.43f, 0.41f);
            StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.3f, true));
            StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait: 1f));
        }
        _keys.ForEach(k => StartCoroutine(k.GetComponent<PianoKeyController>().Dispose()));
        _draggableKeys.ForEach(k => StartCoroutine(k.GetComponent<PianoKeyDraggableController>().Dispose()));
        SpawnKeys(wait: 0.5f);
    }

    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(DecreaseTimer());
                SpawnKeys();
                StartCoroutine(FadeImage(keyOutline, Color.clear, Color.white, 0.5f));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f, targetAlpha: 0.5f));
                StartCoroutine(FadeText(scoreCounter, true, 0.5f));
                break;
        }
        yield return null;
    }
    
    private void SpawnKeys(float wait = 0f)
    {
        _keys = new List<GameObject>();
        _draggableKeys = new List<GameObject>();
        var nextChord = _chords[_random.Next(_chords.Count)];        
        int x = -150;
        foreach(var n in nextChord)
        {
            var key = Instantiate(keyPrefab, keyContainer.transform);
            _keys.Add(key);
            var controller = key.GetComponent<PianoKeyController>();
            key.transform.localPosition = new Vector3(x, 200);
            controller.Show(wait, clickable: false, usePersistentColour: true);
            controller.Note = n;            
            wait += 0.1f;
            x += 100;
        }
        var noteOptions = new List<string> 
        { 
            _correctSevenths[_chords.IndexOf(nextChord)], 
            _allNotes[_random.Next(_allNotes.Length)], 
            _allNotes[_random.Next(_allNotes.Length)],
            _allNotes[_random.Next(_allNotes.Length)] 
        };
        _correctOrder = new List<string>
        {
            nextChord[0], nextChord[1], nextChord[2], noteOptions[0]
        };
        noteOptions.Shuffle();
        wait = 0f;
        x = -150;
        foreach(var n in noteOptions)
        {
            var key = Instantiate(draggableKeyPrefab, keyContainer.transform);
            _draggableKeys.Add(key);
            var controller = key.GetComponent<PianoKeyDraggableController>();
            key.transform.localPosition = new Vector3(x, -100);
            controller.Show(wait, keyOutline.gameObject.transform.localPosition, clickable: false, usePersistentColour: true);
            controller.Note = n;
            wait += 0.1f;
            x += 100;
        }
    }

    private void TimerEnd()
    {        
        if (_correctChords > 0)
        {            
            int stars;
            if (_correctChords < 3)
            {
                stars = 1;
            }
            else if (_correctChords < 6)
            {
                stars = 2;
            }
            else
            {
                stars = 3;
            }
            string readout = stars > 1 ? "stars" : "star";
            introText.text = $"Nice work {Persistent.userName}! You matched {_correctChords} chords and got {stars} {readout}. You can try again, or move into the next lesson.";
            StartCoroutine(FadeText(introText, true, 0.5f));
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
            StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            List<Vector2> starPositions = new List<Vector2>();
            switch (stars)
            {
                case 1:
                    starPositions.Add(new Vector2(0, 50));
                    break;
                case 2:
                    starPositions.Add(new Vector2(-50, 50));
                    starPositions.Add(new Vector2(50, 50));
                    break;
                case 3:
                    starPositions.Add(new Vector2(-70, 50));
                    starPositions.Add(new Vector2(0, 50));
                    starPositions.Add(new Vector2(70, 50));
                    break;
            }
            _stars = new List<GameObject>();
            for (int i = 0; i < stars; i++)
            {
                _stars.Add(Instantiate(starPrefab, mainContainer.transform));
                _stars[i].transform.localPosition = starPositions[i];
                _stars[i].GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                StartCoroutine(FadeInObjectScale(_stars[i], overshootCurve, true, 0.3f, wait: (0.2f * i)));
            }
            if (stars > Persistent.harmonyLessons.scores["Major Seventh Chords"])
            {
                Persistent.harmonyLessons.scores["Major Seventh Chords"] = stars;
                Persistent.harmonyLessons.lessons["Minor Seventh Chords"] = true;
                Persistent.UpdateLessonAvailability("Harmony");
            }
        }
        else
        {
            introText.text = "Looks like you didn't match any chords correctly. Press retry to try again.";
            StartCoroutine(FadeText(introText, true, 0.5f));
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
        }
    }

    private IEnumerator DecreaseTimer()
    {
        _playing = true;
        float timeCounter = 0f;
        timeSlider.fillRect.GetComponent<Image>().color = Persistent.rainbowColours[3];
        while (timeCounter <= allowedTime)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            float remaining = allowedTime - timeCounter;
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
                timeText.text = "Time remaining: " + ((int)(allowedTime - timeCounter));
            }

            timeSlider.fillRect.GetComponent<Image>().color = Color.Lerp(Persistent.rainbowColours[3], Persistent.rainbowColours[0], timeCounter / allowedTime);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        _playing = false;
        timeText.text = "Time Remaining: 0";
        TimerEnd();
    }
}
