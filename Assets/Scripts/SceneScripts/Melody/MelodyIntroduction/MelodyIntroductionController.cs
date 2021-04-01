using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MelodyIntroductionController : BaseManager
{
    [SerializeField] private Text introText;
    [SerializeField] private Text notesText;
    [SerializeField] private GameObject nextButton, xylophonePrefab;
    
    private GameObject _xylophone;
    private int _levelStage;

    protected override void OnAwake()
    {
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback}
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback}
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true},
            {notesText, true},
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true}
        };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeText(notesText, true, 0.5f, 1.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 4f));
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
            Persistent.UpdateUserGlossary(new[] { "Notes", "Melody" });
            Persistent.sceneToLoad = "Notes";
            Persistent.goingHome = false;
            Persistent.melodyLessons.lessons["Notes"] = true;
            Persistent.UpdateLessonAvailability("Melody");
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeText(notesText, false, 0.5f, destroy:true));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                float counter = 0f;
                while (counter <= 1.5f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }

                    counter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text =
                    "More on that later, for now have a play with the notes. Pay attention to which notes sound good together, and which don't.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _xylophone = Instantiate(xylophonePrefab, transform.GetChild(0));
                _xylophone.transform.localPosition = new Vector3(0, -200, 0);
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 5f));
                break;
            case 2:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                counter = 0f;
                while (counter <= 1.5f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }

                    counter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "When we introduce new terms in each lesson, they will be added to the Glossary which you can see any time through the Pause Menu or from the Main Menu.\n \nWhenever you're ready, let's move into the first lesson!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait:1f));
                break;
        }
    }
}
