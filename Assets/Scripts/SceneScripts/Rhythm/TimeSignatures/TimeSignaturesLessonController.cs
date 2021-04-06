using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimeSignaturesLessonController : BaseManager
{
    [SerializeField] private GameObject drumContainer;
    [SerializeField] private GameObject nextButton, playButton, fourFourButton, sixEightButton;
    [SerializeField] private Text introText;
    [SerializeField] private GameObject drumkitPrefab;    

    private int _levelStage;
    private GameObject _drumkit;
    private bool _readyToPlayPattern = true;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {playButton, PlayButtonCallback },
            {fourFourButton, PatternButtonCallback },
            {sixEightButton, PatternButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {playButton.GetComponentInChildren<Text>(), true },
            {fourFourButton.GetComponentInChildren<Text>(), true },
            {sixEightButton.GetComponentInChildren<Text>(), true }
        };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if(_levelStage < 4)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
            bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
            Persistent.UpdateUserGlossary("Time Signature");
            Persistent.sceneToLoad = "NoteValues";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void PlayButtonCallback(GameObject g)
    {
        if (_drumkit is null || !_readyToPlayPattern) return;
        if(_levelStage == 1)
        {
            _readyToPlayPattern = false;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/SimpleBackbeat90bpmWithClick");            
            _drumkit.GetComponent<DrumKitController>().PlayPattern(3);
            StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 5f));
        }
        else if(_levelStage == 2)
        {
            _readyToPlayPattern = false;
            FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/CompoundBackbeat90bpmWithClick");
            _drumkit.GetComponent<DrumKitController>().PlayPattern(4);
            StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 4f));
        }
    }

    private void PatternButtonCallback(GameObject g)
    {
        if (_levelStage < 3) return;
        _drumkit.GetComponent<DrumKitController>().StopAnimating();
        var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        if (g == fourFourButton)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/SimpleBackbeat90bpmWithClick");
            _drumkit.GetComponent<DrumKitController>().PlayPattern(3);
        }
        else if(g == sixEightButton)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Drums/CompoundBackbeat90bpmWithClick");
            _drumkit.GetComponent<DrumKitController>().PlayPattern(4);
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
                introText.text = "We will talk about note values in the next lesson. For now, let's look at two Time Signatures.\n \nThe drums will play 2 bars of a 4/4 (said like Four Four) Time Signature with a Click Track. There are 4 strong beats which are Quarter Notes, hence 4/4. Hit Play to hear it!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(playButton, true, 0.5f, wait: 1f));
                _drumkit = Instantiate(drumkitPrefab, drumContainer.transform);
                _drumkit.transform.localPosition = new Vector3(0, 0);
                _drumkit.transform.localScale = new Vector3(0.8f, 0.8f);
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
                introText.text = "The next one is 6/8 (said like Six Eight). There are 6 short Eighth Notes in each bar, which make a very different type of rhythm.\n \nHit Play to hear it!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(playButton, true, 0.5f, wait: 1f));
                _readyToPlayPattern = true;
                break;
            case 3:
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
                introText.text = "You can hear either of the patterns again, and hit Next when you're ready for the next lesson!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 1f));
                StartCoroutine(FadeButtonText(fourFourButton, true, 0.5f, wait: 1f));
                StartCoroutine(FadeButtonText(sixEightButton, true, 0.5f, wait: 1f));
                break;
        }
    }
}
