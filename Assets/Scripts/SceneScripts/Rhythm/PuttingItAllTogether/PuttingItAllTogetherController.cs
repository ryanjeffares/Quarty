using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PuttingItAllTogetherController : BaseManager
{
    [SerializeField] private Text introText, chordProgText, patternText;
    [SerializeField] private GameObject mainContainer;    
    [SerializeField] private GameObject crotchy;
    [SerializeField] private GameObject nextButton, playButton, finishedButton;
    [SerializeField] private List<GameObject> progressionButtons;
    [SerializeField] private List<GameObject> progressionImages;
    [SerializeField] private List<GameObject> patternButtons;
    [SerializeField] private List<GameObject> patternImages;
    [SerializeField] private GameObject drumPrefab, pianoPrefab;
    [SerializeField] private AnimationCurve curve;

    private int _levelStage;

    private PianoController _pianoController;
    private GameObject _piano;
    private GameObject Piano
    {
        get => _piano;
        set
        {
            _piano = value;
            _pianoController = _piano.GetComponent<PianoController>();
        }
    }

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

    private List<string> _parameterNames = new List<string>
    {
        "1451Volume", "1564Volume", "1251Volume"
    };
    private List<string> _patternNames = new List<string>
    {
        "event:/AllTogether/Backbeat90bpm", "event:/AllTogether/Syncopated90bpm", "event:/AllTogether/Funk120bpm"
    };
    private string _selectedPattern = "event:/AllTogether/Backbeat90bpm";

    private bool _playing;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {playButton, PlayButtonCallback },
            {finishedButton, (g) => 
                {
                    if (_levelStage < 2) return;
                    FMODUnity.RuntimeManager.GetBus("bus:/Objects").stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    Persistent.goingHome = true; 
                    Persistent.sceneToLoad = "MainMenu"; 
                    SceneManager.LoadScene("LoadingScreen"); 
                } 
            },
            {progressionButtons[0], ProgressionButtonCallback },
            {progressionButtons[1], ProgressionButtonCallback },
            {progressionButtons[2], ProgressionButtonCallback },
            {patternButtons[0], PatternButtonCallback },
            {patternButtons[1], PatternButtonCallback },
            {patternButtons[2], PatternButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {chordProgText, true },
            {patternText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {playButton.GetComponentInChildren<Text>(), true },
            {finishedButton.GetComponentInChildren<Text>(), true },
            {progressionButtons[0].GetComponentInChildren<Text>(), true },
            {progressionButtons[1].GetComponentInChildren<Text>(), true },
            {progressionButtons[2].GetComponentInChildren<Text>(), true },
            {patternButtons[0].GetComponentInChildren<Text>(), true },
            {patternButtons[1].GetComponentInChildren<Text>(), true },
            {patternButtons[2].GetComponentInChildren<Text>(), true }
        };
        introText.text = $"Congratulations, {Persistent.userName}! You have completed all the courses of Quarty with flying colours. We hope you enjoyed yourself, and thank you for playing. Remember to take the survey linked through the Settings Page if you haven't already.\n" +
            $"\nBest of luck on your future musical journey - we hope this little game has helped to prepare you for it.";
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        StartCoroutine(AdvanceLevelStage());
    }

    private void PlayButtonCallback(GameObject g)
    {
        FMODUnity.RuntimeManager.PlayOneShot(_selectedPattern);
        _drumkitController.PlayPattern(_patternNames.IndexOf(_selectedPattern));
        var col = playButton.GetComponentInChildren<Text>().color;
        playButton.GetComponentInChildren<Text>().color = new Color(col.r, col.g, col.b, 0.3f);
        _playing = true;
        StartCoroutine(DisablePlayButton(col));
    }
    
    private IEnumerator DisablePlayButton(Color col)
    {
        yield return new WaitForSeconds(_selectedPattern == _patternNames[1] ? 8 : 12);
        _playing = false;
        playButton.GetComponentInChildren<Text>().color = col;
    }

    private void ProgressionButtonCallback(GameObject g)
    {
        if (_playing) return;
        foreach(var (img, index) in progressionImages.WithIndex())
        {
            img.SetActive(index == progressionButtons.IndexOf(g));
        }
        foreach(var (param, index) in _parameterNames.WithIndex())
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName(param, index == progressionButtons.IndexOf(g) ? 1 : 0);
        }
    }

    private void PatternButtonCallback(GameObject g)
    {
        if (_playing) return;
        _selectedPattern = _patternNames[patternButtons.IndexOf(g)];
        foreach(var (img, index) in patternImages.WithIndex())
        {
            img.SetActive(index == patternButtons.IndexOf(g));
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
                introText.text = "Take this level as an oppurtunity to put it all together, and make some music! Select a drum pattern, a chord progression, press play and play a melody along with it - hit Next to check it out.\n" +
                    "\nAs well as studying music and playing an instrument, there's lots of free software where you can experiment with making your own music and expressing yourself. Have fun!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
                break;
            case 2:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(FadeButtonText(playButton, true, 0.5f));
                StartCoroutine(FadeButtonText(finishedButton, true, 0.5f));
                StartCoroutine(FadeInObjectScale(crotchy, curve, false, 0.5f));
                StartCoroutine(FadeText(chordProgText, true, 0.5f));
                StartCoroutine(FadeText(patternText, true, 0.5f));
                foreach(var b in progressionButtons.Concat(patternButtons))
                {
                    StartCoroutine(FadeButtonText(b, true, 0.5f));
                }
                StartCoroutine(FadeImage(progressionImages[0].GetComponent<Image>(), Color.clear, Color.white, 0.5f));
                StartCoroutine(FadeImage(patternImages[0].GetComponent<Image>(), Color.clear, Color.white, 0.5f));
                Piano = Instantiate(pianoPrefab, mainContainer.transform);
                Piano.transform.localPosition = new Vector3(0, 280);
                _pianoController.Show(3);
                Drumkit = Instantiate(drumPrefab, mainContainer.transform);
                Drumkit.transform.localPosition = new Vector3(0, -360);
                Drumkit.transform.localScale = new Vector3(0.8f, 0.8f);
                _drumkitController.Show();
                break;
        }
    }
}
