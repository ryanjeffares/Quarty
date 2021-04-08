using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NoteValuesPuzzleController : BaseManager
{
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private Text introText, niceText;
    [SerializeField] private GameObject nextButton, retryButton;
    [SerializeField] private List<GameObject> patternButtons;
    [SerializeField] private GameObject drumContainer, drumPrefab;
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private AnimationCurve overshootCurve, overshootOutCurve;

    private Dictionary<DrumPattern, int> _correctAnswerLookup;
    private List<GameObject> _stars = new List<GameObject>();
    private int _correctAnswer;
    private readonly System.Random _random = new System.Random();    
    private DrumKitController _drumkitController;
    private GameObject _drumkit;
    private GameObject Drumkit
    {
        get => _drumkit;
        set
        {
            _drumkit = value;
            _drumkitController = _drumkit.GetComponent<DrumKitController>();
        }
    }

    private int _levelStage;
    private DrumPattern _lastPattern;
    private int _patternsPlayed, _correctPatterns;
    private bool _firstRun = true;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {retryButton, RetryButtonCallback},
            {patternButtons[0], PatternButtonCallback },
            {patternButtons[1], PatternButtonCallback },
            {patternButtons[2], PatternButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {niceText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {retryButton.GetComponentInChildren<Text>(), true },
            {patternButtons[0].GetComponentInChildren<Text>(), true },
            {patternButtons[1].GetComponentInChildren<Text>(), true },
            {patternButtons[2].GetComponentInChildren<Text>(), true }
        };
        _correctAnswerLookup = new Dictionary<DrumPattern, int>
        {
            {DrumPattern.KickEights, 1 },
            {DrumPattern.KickQuarters, 0 },
            {DrumPattern.SnareEights, 1 },
            {DrumPattern.SnareSixteenths, 2 },
            {DrumPattern.HatsEights, 1 },
            {DrumPattern.HatsSixteenths, 2 },
            {DrumPattern.TomsEights, 1 },
            {DrumPattern.TomsSixteenths, 2 },
        };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
        Drumkit = Instantiate(drumPrefab, drumContainer.transform);
        Drumkit.transform.localPosition = new Vector3(0, 0);
        _drumkitController.Show(clickable: false);
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        StartCoroutine(AdvanceLevelStage());
    }

    private void RetryButtonCallback(GameObject g)
    {
        foreach(var (s, i) in _stars.WithIndex())
        {
            StartCoroutine(FadeInObjectScale(s, overshootOutCurve, false, 0.3f, wait: i * 0.2f));
        }
        _firstRun = true;
        _patternsPlayed = 0;
        _correctPatterns = 0;
        StartCoroutine(FadeText(introText, false, 0.5f));
        StartCoroutine(FadeButtonText(retryButton, false, 0.5f));
        PlayPattern();
    }

    private void PatternButtonCallback(GameObject g)
    {
        if(patternButtons.IndexOf(g) == _correctAnswer)
        {
            _correctPatterns++;
            niceText.text = "Correct!";
            niceText.color = new Color(0.32f, 0.57f, 0.47f);
            StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.2f, true));
            StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.2f, false, wait: 1f));
        }
        else
        {
            niceText.text = "Not quite!";
            niceText.color = new Color(0.76f, 0.43f, 0.41f);
            StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.2f, true));
            StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.2f, false, wait: 1f));
        }
        if (_patternsPlayed < 6)
        {
            PlayPattern();
        }
        else 
        { 
            PuzzleFinished(); 
        }
    }

    protected override IEnumerator AdvanceLevelStage()
    {        
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                PlayPattern();
                break;
        }
        yield return null;
    }

    private void PuzzleFinished()
    {
        foreach(var b in patternButtons)
        {
            StartCoroutine(FadeButtonText(b, false, 0.5f));
        }
        if (_correctPatterns > 0)
        {            
            int stars;
            if (_correctPatterns < 3)
            {
                stars = 1;
            }
            else if (_correctPatterns < 6)
            {
                stars = 2;
            }
            else
            {
                stars = 3;
            }
            string readout = stars > 1 ? "stars" : "star";
            introText.text = $"Awesome {Persistent.userName}! You correctly matched {_correctPatterns} note values and got {stars} {readout}. You can try again, or move into the next lesson.";
            StartCoroutine(FadeText(introText, true, 0.5f));
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
            StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            List<Vector2> starPositions = new List<Vector2>();
            switch (stars)
            {
                case 1:
                    starPositions.Add(new Vector2(0, 220));
                    break;
                case 2:
                    starPositions.Add(new Vector2(-50, 220));
                    starPositions.Add(new Vector2(50, 220));
                    break;
                case 3:
                    starPositions.Add(new Vector2(-70, 220));
                    starPositions.Add(new Vector2(0, 220));
                    starPositions.Add(new Vector2(70, 220));
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
            if (stars > Persistent.rhythmLessons.scores["Note Values"])
            {
                Persistent.rhythmLessons.lessons["Putting It All Together"] = true;
                Persistent.rhythmLessons.scores["Note Values"] = stars;
                Persistent.UpdateLessonAvailability("Rhythm");
            }
        }
        else
        {
            introText.text = "Looks like you didn't match any note values correctly. Press retry to try again.";
            StartCoroutine(FadeText(introText, true, 0.5f));
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
        }
    }

    private void PlayPattern()
    {
        _patternsPlayed++;
        if (_firstRun)
        {
            _firstRun = false;
        }
        else
        {
            foreach (var b in patternButtons)
            {
                StartCoroutine(FadeButtonText(b, false, 0.5f));
            }
        }
        var pattern = (DrumPattern)_random.Next(8);
        while (pattern == _lastPattern)
        {
            pattern = (DrumPattern)_random.Next(8);
        }
        _lastPattern = pattern;
        _correctAnswer = _correctAnswerLookup[pattern];
        _drumkitController.StopAnimating(stopAudio: true);
        _drumkitController.PlayPattern(pattern);        
        foreach(var b in patternButtons)
        {
            StartCoroutine(FadeButtonText(b, true, 0.5f, wait: 4f));
        }
    }
}
