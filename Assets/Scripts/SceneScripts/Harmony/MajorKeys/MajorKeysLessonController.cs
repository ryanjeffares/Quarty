using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MajorKeysLessonController : BaseManager
{
    [SerializeField] private GameObject pianoContainer;
    [SerializeField] private Text introText;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject pianoPrefab;

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
            {nextButton.GetComponentInChildren<Text>(), true }
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
            Persistent.UpdateUserGlossary(new[] { "Key", "Major Key" });
            Persistent.harmonyLessons.lessons["Minor Keys"] = true;
            Persistent.UpdateLessonAvailability("Harmony");
            Persistent.sceneToLoad = "MinorKeys";
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
                introText.text = "A Major Key has all the notes from a Major Scale, and chords with those notes as the roots.\n \nThe types of chords, in order of the scale, are Major, Minor, Minor, Major, Major, Minor, Diminished.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
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
                introText.text = "The C Major scale has the notes C, D, E, F, G, A, and B, so the chords would be:\n \nC Major, D Minor, E Minor, F Major, G Major, A Minor, and B Diminished. Hit the root notes and we'll play the chords for you!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                var piano = Instantiate(pianoPrefab, pianoContainer.transform);
                piano.GetComponent<PianoController>().Show(2, showFlats: false, autoPlayNotes: true, useColours: true);
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
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
                introText.text = "Press Next when you're ready for the next lesson!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                break;
        }        
    }
}
