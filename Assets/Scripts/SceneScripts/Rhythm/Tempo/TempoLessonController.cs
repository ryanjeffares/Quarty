using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TempoLessonController : BaseManager
{
    [SerializeField] private GameObject nextButton, tryButton;
    [SerializeField] private Text introText;
    [SerializeField] private List<Text> beatTexts;

    private int _levelStage;
    private bool _sequencePlaying;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {tryButton, TryButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {tryButton.GetComponentInChildren<Text>(), true }
        };
        foreach(var t in beatTexts)
        {
            canTextLerp.Add(t, true);
        }
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if(_levelStage < 3)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            var bus = FMODUnity.RuntimeManager.GetBus("bus:/Objects");
            bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
            Persistent.UpdateUserGlossary("Tempo");
            Persistent.sceneToLoad = "TempoPuzzle";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void TryButtonCallback(GameObject g)
    {
        if (_sequencePlaying) return;
        FMODUnity.RuntimeManager.PlayOneShot("event:/Tempo/Click120bpm");
        StartCoroutine(HighlightTexts(120));
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
                introText.text = "We often count to 4 in music - we'll talk about that in a later lesson! If we count to 4 at 120 beats per minute, there will be 2 beats every second. Here's what that sounds like!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                foreach(var (t, index) in beatTexts.WithIndex())
                {
                    StartCoroutine(FadeText(t, true, 0.5f, wait: (index * 0.1f)));
                }
                timeCounter = 0f;
                while (timeCounter <= 1.5f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return null;
                }                
                FMODUnity.RuntimeManager.PlayOneShot("event:/Tempo/Click120bpm");
                StartCoroutine(HighlightTexts(120));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 5f));
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
                introText.text = "You can hear it again and try tapping along - most musicians tap with their feet!\n \nHit next when you're ready for the puzzle!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                StartCoroutine(FadeButtonText(tryButton, true, 0.5f));
                break;
        }
    }

    private IEnumerator HighlightTexts(int bpm)
    {
        _sequencePlaying = true;
        float beatTime = 60f / bpm;
        foreach(var t in beatTexts)
        {
            StartCoroutine(ResizeText(t));
            yield return new WaitForSeconds(beatTime);
        }
        _sequencePlaying = false;
    }

    private IEnumerator ResizeText(Text t)
    {        
        float timeCounter = 0f;
        while(timeCounter <= 0.5f)
        {
            if(timeCounter <= 0.25f)
            {
                t.gameObject.transform.localScale = new Vector3(1 + (timeCounter * 2), 1 + (timeCounter * 2));
            }
            else
            {
                t.gameObject.transform.localScale = new Vector3(2 - (timeCounter * 2), 2 - (timeCounter * 2));
            }
            timeCounter += Time.deltaTime;
            yield return null;
        }        
    }
}
