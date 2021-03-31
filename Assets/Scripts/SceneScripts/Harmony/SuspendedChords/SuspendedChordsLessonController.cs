using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SuspendedChordsLessonController : BaseManager
{
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject pianoPrefab, pianoContainer;
    [SerializeField] private Text introText, notesText, chordText;

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
            {notesText, true },
            {chordText, true },
            {nextButton.GetComponentInChildren<Text>(), true }
        };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeText(notesText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if (_levelStage < 5)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.sceneToLoad = "SuspendedChordsPuzzle";
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
                introText.text = "A Suspended 2nd Chord (written as sus2) has a Major 2nd (1 Tone from the Root) in place of the Third. For C, this would be C, D, and G.\n \nHere's what that sounds like!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _piano = Instantiate(pianoPrefab, pianoContainer.transform);
                _piano.GetComponent<PianoController>().Show(2, showFlats: false);
                yield return new WaitForSeconds(1f);
                _piano.GetComponent<PianoController>().HighlightKeys(new[] { "C2", "D2", "G2" });
                yield return new WaitForSeconds(2f);
                _piano.GetComponent<PianoController>().PlayNotesManual(new[] { "C2", "D2", "G2" });
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
                introText.text = "A Suspended 4th Chord (written as sus4) has a Perfect 4th (5 Semitones from the Root) in place of the Third. For C, this would be C, F, and G.\n \nHere's what that sounds like!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                _piano.GetComponent<PianoController>().RemoveKeyHighlights(new[] { "C2", "D2", "G2" });
                _piano.GetComponent<PianoController>().HighlightKeys(new[] { "C2", "F2", "G2" });
                yield return new WaitForSeconds(2f);
                _piano.GetComponent<PianoController>().PlayNotesManual(new[] { "C2", "F2", "G2" });
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
                introText.text = "These chords can be used to create suspense. Playing a Suspended Chord before a regular Major Chord with the same root creates a nice resolve.\n \nHere's what that sounds like for C!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                chordText.text = "C sus4";
                StartCoroutine(FadeText(chordText, true, 0.5f));
                yield return new WaitForSeconds(4f);                
                _piano.GetComponent<PianoController>().PlayNotesManual(new[] { "C2", "F2", "G2" });
                yield return new WaitForSeconds(1f);
                chordText.text = "C sus2";
                _piano.GetComponent<PianoController>().RemoveKeyHighlights(new[] { "C2", "F2", "G2" });
                _piano.GetComponent<PianoController>().HighlightKeys(new[] { "C2", "D2", "G2" });
                _piano.GetComponent<PianoController>().PlayNotesManual(new[] { "C2", "D2", "G2" });
                yield return new WaitForSeconds(1f);
                chordText.text = "C Major";
                _piano.GetComponent<PianoController>().RemoveKeyHighlights(new[] { "C2", "D2", "G2" });
                _piano.GetComponent<PianoController>().HighlightKeys(new[] { "C2", "E2", "G2" });
                _piano.GetComponent<PianoController>().PlayNotesManual(new[] { "C2", "E2", "G2" });
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 1f));
                break;
            case 4:
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(FadeText(introText, false, 0.5f));
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
                introText.text = "You can play with the notes here and hit next when you're ready for the puzzle!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
                break;
        }
    }
}
