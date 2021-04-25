using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChordProgressionsLessonController : BaseManager
{
    [SerializeField] private GameObject pianoContainer;
    [SerializeField] private Text introText, helpText;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private Toggle autoPlayChords;
    [SerializeField] private GameObject pianoPrefab;
    [SerializeField] private AnimationCurve curve;

    private int _levelStage;
    private GameObject _piano;

    protected override void OnAwake()
    {
        introText.text = $"Congratulations {Persistent.userName}! You have completed the Harmony section of Quarty. You can see how you did on the stats page and move on into the Rhythm section whenever you want to. Remember to look over the glossary at all the Music Theory terms you've learned about!";
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {helpText, true },
            {nextButton.GetComponentInChildren<Text>(), true }
        };
        autoPlayChords.onValueChanged.AddListener((state) =>
        {
            if (_piano is null) return;
            _piano.GetComponent<PianoController>().ToggleAutoplay(state);
        });
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
            Persistent.rhythmLessons.lessons["Rhythm Introduction"] = true;
            Persistent.UpdateLessonAvailability("Rhythm");
            Persistent.UpdateUserGlossary("Chord Progression");
            Persistent.sceneToLoad = "MainMenu";
            Persistent.goingHome = true;
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
                introText.text = "Before you go on, let's quickly talk about Chord Progressions. A Chord Progression is a sequence of chords.\n \nTry playing some of the Chord Progressions listed below (the numbers are the position of the root note in the scale), and make up your own! Hit Next when you're done!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeText(helpText, true, 0.5f, wait: 2f));
                StartCoroutine(FadeButtonText(nextButton, true, 4f));
                _piano = Instantiate(pianoPrefab, pianoContainer.transform);
                _piano.GetComponent<PianoController>().Show(3, showFlats: false, useColours: true, autoPlayNotes: true, showNumbers: true);
                StartCoroutine(FadeInObjectScale(autoPlayChords.gameObject, curve, true, 0.5f, wait: 2f));
                break;
        }
    }
}
