using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MajorTriadsLessonController : BaseManager
{
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject pianoPrefab;
    [SerializeField] private GameObject pianoContainer;
    [SerializeField] private Text introText;

    private GameObject _piano;
    private int _levelStage;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
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
            Persistent.UpdateUserGlossary(new[] { "Chord", "Triad", "Major Chord" });
            Persistent.sceneToLoad = "MajorTriadsPuzzle";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
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
                introText.text = "The Major Triad is made of the root, Major 3rd, and Perfect 5th. This is a Major Chord.\n \nFor C, the C Major Chord would be C, E, and G. Try playing it!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _piano = Instantiate(pianoPrefab, pianoContainer.transform);
                _piano.GetComponent<PianoController>().Show(1);
                _piano.GetComponent<PianoController>().HighlightKeys(new string[]{ "C2", "E2", "G2"});
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 3f));
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
                introText.text = "Hit next when you're ready to move into the puzzle.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                break;
        }
    }
}
