using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NoteValuesLessonController : BaseManager
{
    [SerializeField] private Text introText;
    [SerializeField] private GameObject nextButton, playButton;
    [SerializeField] private List<GameObject> patternButtons;
    [SerializeField] private GameObject drumkitPrefab;
    [SerializeField] private GameObject drumContainer;

    private int _levelStage;
    private GameObject _drumkit;
    private bool _readyToAnimate = true;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {playButton, PlayButtonCallback }            
        };        
        canTextLerp = new Dictionary<Text, bool> 
        {
            {introText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {playButton.GetComponentInChildren<Text>(), true }
        };
        foreach (var b in patternButtons)
        {
            fullCallbackLookup.Add(b, PatternButtonCallback);
            canTextLerp.Add(b.GetComponentInChildren<Text>(), true);
        }
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if(_levelStage < 5)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
            bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
            Persistent.UpdateUserGlossary(new[] { "Note Value", "Quarter Note", "Eighth Note", "Sixteenth Note" });
            Persistent.sceneToLoad = "NoteValuesPuzzle";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void PatternButtonCallback(GameObject g)
    {
        if (_drumkit is null || _levelStage < 4) return;
        var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _drumkit.GetComponent<DrumKitController>().StopAnimating();
        switch (patternButtons.IndexOf(g))
        {
            case 0: // q
                FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/KickLoop");
                _drumkit.GetComponent<DrumKitController>().PlayPattern(5);
                break;
            case 1: // e
                FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/HatsLoop");
                _drumkit.GetComponent<DrumKitController>().PlayPattern(6);
                break;
            case 2: // s
                FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/HatsLoopSixteenths");
                _drumkit.GetComponent<DrumKitController>().PlayPattern(7);
                break;
            case 3: // q + e
                FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/KickAndHatsLoopEights");
                _drumkit.GetComponent<DrumKitController>().PlayPattern(5);
                _drumkit.GetComponent<DrumKitController>().PlayPattern(6);
                break;
            case 4: // q + s
                FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/KickAndHatsLoopSixteenths");
                _drumkit.GetComponent<DrumKitController>().PlayPattern(5);
                _drumkit.GetComponent<DrumKitController>().PlayPattern(7);
                break;
        }
    }

    private void PlayButtonCallback(GameObject g)
    {
        if (_drumkit is null || !_readyToAnimate) return;
        var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _drumkit.GetComponent<DrumKitController>().StopAnimating();
        switch (_levelStage)
        {
            case 1:
                {
                    _readyToAnimate = false;
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/KickLoop");
                    _drumkit.GetComponent<DrumKitController>().PlayPattern(5);
                    StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 5f));
                    break;
                }
            case 2:
                {
                    _readyToAnimate = false;
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/HatsLoop");
                    _drumkit.GetComponent<DrumKitController>().PlayPattern(6);
                    StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 5f));
                    break;
                }
            case 3:
                {
                    _readyToAnimate = false;
                    FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/HatsLoopSixteenths");
                    _drumkit.GetComponent<DrumKitController>().PlayPattern(7);
                    StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 5f));
                    break;
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
                introText.text = "We will talk about the Quarter Note, the Eighth Note, and the Sixteenth Note.\n \nA Quarter Note would be a Quarter of a bar of 4/4, so there would be 4 Quarter Notes in a bar. Hit Play to hear Quarter Notes on the kick drum!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(playButton, true, 0.5f, wait: 1f));
                _drumkit = Instantiate(drumkitPrefab, drumContainer.transform);
                _drumkit.transform.localScale = new Vector3(0.8f, 0.8f);
                _drumkit.transform.localPosition = new Vector3(0, 0);
                _drumkit.GetComponent<DrumKitController>().Show(clickable: false);
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
                introText.text = "An Eighth Note would be an Eighth of a bar of 4/4, so there would be 8 Eighth Notes in a bar. Hit Play to hear Eighth Notes on the hi hats!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _readyToAnimate = true;
                StartCoroutine(FadeButtonText(playButton, true, 0.5f, wait: 1f));
                break;
            case 3:
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
                introText.text = "A Sixteenth Note would be a Sixteenth of a bar of 4/4, so there would be 16 Sixteenth Notes in a bar. Hit Play to hear Sixteenth Notes on the hi hats!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _readyToAnimate = true;
                StartCoroutine(FadeButtonText(playButton, true, 0.5f, wait: 1f));
                break;
            case 4:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(FadeButtonText(playButton, false, 0.5f));
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
                Destroy(playButton);
                introText.text = "You can hear them again (or at the same time), and hit Next when you're ready for the puzzle!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                foreach(var b in patternButtons)
                {
                    StartCoroutine(FadeButtonText(b, true, 0.5f));
                }
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                break;
        }
    }
}
