using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MajorSeventhsLessonController : BaseManager
{
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject pianoPrefab, pianoContainer;
    [SerializeField] private Text introText;

    private int _levelStage;

    protected override void OnAwake()
    {
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback }
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {nextButton.GetComponentInChildren<Text>(), true }
        };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if (_levelStage < 3)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.sceneToLoad = "MajorSeventhChordsPuzzle";
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
                introText.text = "In a Major Scale, we use the Major 7th and we get a Major 7th Chord. Here's what it sounds like!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                var piano = Instantiate(pianoPrefab, pianoContainer.transform);
                piano.GetComponent<PianoController>().Show(1, showFlats: false);
                piano.GetComponent<PianoController>().HighlightKeys(new string[] { "C2", "E2", "G2", "B2" });
                yield return new WaitForSeconds(2f);
                piano.GetComponent<PianoController>().PlayNotesManual(new string[] { "C2", "E2", "G2", "B2" });
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 0.5f));
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
                introText.text = "The Major 7th Chord has a soft and airy sound, and it is used commonly in jazz music.\n \nHit next when you're ready for the puzzle!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 0.5f));
                break;
        }
    }
}
