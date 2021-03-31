using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MinorSeventhsLessonController : BaseManager
{
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject pianoPrefab, pianoContainer;
    [SerializeField] private Text introText;

    private int _levelStage;
    private GameObject _piano;

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
        if (_levelStage < 4)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.sceneToLoad = "MinorSeventhChordsPuzzle";
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
                while (timeCounter <= 1f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }
                    timeCounter += Time.deltaTime;
                    yield return null;
                }
                introText.text = "The D Dominant Seventh would be D, F, A, and C. Here's what that sounds like!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _piano = Instantiate(pianoPrefab, pianoContainer.transform);
                _piano.GetComponent<PianoController>().Show(2, showFlats: false);
                yield return new WaitForSeconds(1f);
                _piano.GetComponent<PianoController>().HighlightKeys(new[] { "D2", "F2", "A2", "C3" });
                yield return new WaitForSeconds(2f);
                _piano.GetComponent<PianoController>().PlayNotesManual(new[] { "D2", "F2", "A2", "C3" });
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
                introText.text = "The A Minor Seventh would be A, C, E, and G. Here's what that sounds like!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _piano.GetComponent<PianoController>().RemoveKeyHighlights(new[] { "D2", "F2", "A2", "C3" });
                _piano.GetComponent<PianoController>().HighlightKeys(new[] { "A2", "C3", "E3", "G3" });
                yield return new WaitForSeconds(2f);
                _piano.GetComponent<PianoController>().PlayNotesManual(new[] { "A2", "C3", "E3", "G3" });
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 0.5f));
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
                introText.text = "These chords can be used to create tension and suspense.\n \nHit next when you're ready for the puzzle!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 0.5f));
                break;
        }
    }
}
