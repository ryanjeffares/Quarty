using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HarmonyIntroductionController : BaseManager
{
    [SerializeField] private GameObject nextButton, pianoPrefab, pianoContainer;
    [SerializeField] private Text introText;
    private GameObject _piano;
    private int _levelStage = 0;

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
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
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
            Persistent.melodyLessons.lessons["Major Triads"] = true;
            Persistent.UpdateLessonAvailability("Harmony");
            Persistent.sceneToLoad = "MajorTriadsLesson";
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
                while (timeCounter <= 1)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
                introText.text = "Play around with the notes on this piano. Try playing some notes at the same time, and use the intervals we learned about in the Melody section.\n \nHit next when you're ready to go into the next lesson!";
                _piano = Instantiate(pianoPrefab, pianoContainer.transform);
                _piano.GetComponent<PianoController>().Show(3);
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 3f));
                break;
        }
    }
}
