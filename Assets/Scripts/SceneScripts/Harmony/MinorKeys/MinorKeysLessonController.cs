using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MinorKeysLessonController : BaseManager
{
    [SerializeField] private GameObject pianoContainer;
    [SerializeField] private Text introText;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject pianoPrefab;

    private int _levelStage;
    private string[] _naturals = new string[]
        {
            "A0", "B0", "C1", "D1", "E1", "F1", "G1",
            "A1", "B1", "C2", "D2", "E2", "F2", "G2",
            "A2", "B2", "C3", "D3", "E3", "F3", "G3",
            "A3"
        };

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
        if (_levelStage < 3)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.UpdateUserGlossary("Minor Key");
            Persistent.harmonyLessons.lessons["Chord Progressions"] = true;
            Persistent.UpdateLessonAvailability("Harmony");
            Persistent.sceneToLoad = "ChordProgressions";
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
                while (timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
                introText.text = "Any Minor Key has the same notes and chords as a Major Key based on the Third of the Minor Key.\n \nFor example, the Minor Third of A Minor is C, so A Minor is the same as C Major. Try playing with it again!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                var piano = Instantiate(pianoPrefab, pianoContainer.transform);
                piano.GetComponent<PianoController>().Show(2, showFlats: false, autoPlayNotes: true, useColours: true, useCustomNotes: true, customNaturals: _naturals);
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
                introText.text = "Press Next when you're ready for the next lesson!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                break;
        }
    }
}
