using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RhythmIntroductionController : BaseManager
{
    [SerializeField] private Text introText;
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject nextButton, showNames;
    [SerializeField] private List<GameObject> patternButtons;
    [SerializeField] private GameObject drumKitPrefab;

    private int _levelStage;
    private GameObject _drumkit;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {patternButtons[0], PatternButtonCallback },
            {patternButtons[1], PatternButtonCallback },
            {patternButtons[2], PatternButtonCallback },
            {showNames, ShowNamesCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {patternButtons[0].GetComponentInChildren<Text>(), true },
            {patternButtons[1].GetComponentInChildren<Text>(), true },
            {patternButtons[2].GetComponentInChildren<Text>(), true },
            {showNames.GetComponentInChildren<Text>(), true }
        };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if(_levelStage < 2)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.UpdateUserGlossary(new[] { "Rhythm", "Kick", "Snare", "Hi Hat", "Toms", "Crash" });
            Persistent.rhythmLessons.lessons["Time Signatures"] = true;
            Persistent.UpdateLessonAvailability("Rhythm");
            Persistent.sceneToLoad = "TimeSignatures";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    protected override void DestroyManager()
    {
        var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    private void PatternButtonCallback(GameObject g)
    {
        if (_drumkit is null) return;
        var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        switch (patternButtons.IndexOf(g))
        {
            case 0:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/Backbeat90bpm");
                break;
            case 1:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/Syncopated90bpm");
                break;
            case 2:
                FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/Funk120bpm");
                break;
        }                
        _drumkit.GetComponent<DrumKitController>().StopAnimating();
        _drumkit.GetComponent<DrumKitController>().PlayPattern(patternButtons.IndexOf(g));                    
    }

    private bool _namesOn;

    private void ShowNamesCallback(GameObject g)
    {
        if (_drumkit is null) return;
        _namesOn = !_namesOn;
        g.GetComponentInChildren<Text>().text = _namesOn ? "Hide Names" : "Show Names";
        _drumkit.GetComponent<DrumKitController>().ShowNames();
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
                introText.text = "We will use a familiar drum kit to demonstrate it. Have a play with the drums by tapping on them or hearing some patterns, and hit next when you're ready to move into the first lesson!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 4f));
                StartCoroutine(FadeButtonText(showNames, true, 0.5f, wait: 2f));
                for(int i = 0; i < patternButtons.Count; i++)
                {
                    StartCoroutine(FadeButtonText(patternButtons[i], true, 0.5f, 1 + (0.5f * i)));
                }
                yield return new WaitForSeconds(1f);
                _drumkit = Instantiate(drumKitPrefab, mainContainer.transform);
                _drumkit.GetComponent<DrumKitController>().Show();
                break;
        }
    }
}
