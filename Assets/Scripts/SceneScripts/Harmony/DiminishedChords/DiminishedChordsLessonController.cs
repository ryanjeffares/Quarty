using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DiminishedChordsLessonController : BaseManager
{
    [SerializeField] private GameObject mainContainer, pianoContainer;
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
        if(_levelStage < 3)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.UpdateUserGlossary(new[] { "Diminished Chord", "Dissonant", "Consonant" });
            Persistent.harmonyLessons.lessons["Major Keys"] = true;
            Persistent.UpdateLessonAvailability("Harmony");
            Persistent.sceneToLoad = "MajorKeys";
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
                introText.text = "A Diminished Chord is made from the Root, Minor Third, and Tritone (3 Tones).\n \nHere's what a C Dimished Chord sounds like!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 4f));
                var piano = Instantiate(pianoPrefab, pianoContainer.transform);
                piano.GetComponent<PianoController>().Show(1);
                yield return new WaitForSeconds(1f);
                piano.GetComponent<PianoController>().HighlightKeys(new[] { "C2", "D#2", "F#2" });
                yield return new WaitForSeconds(2f);
                piano.GetComponent<PianoController>().PlayNotesManual(new[] { "C2", "D#2", "F#2" });
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
                introText.text = "A Diminished Chord would be described as \"Dissonant\", meaning it is unpleasant and the notes clash with eachother. \"Consonant\" is the opposite - pleasant sounding, like a Major Chord.\n \nHit next when you're ready for the next lesson!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                break;
        }
    }
}
