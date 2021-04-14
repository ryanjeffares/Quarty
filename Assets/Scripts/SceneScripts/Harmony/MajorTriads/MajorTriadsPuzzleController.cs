using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MajorTriadsPuzzleController : BaseManager
{
    [SerializeField] private GameObject nextButton, retryButton;
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject chordPrefab, starPrefab;
    [SerializeField] private GameObject piano;
    [SerializeField] private Text introText, niceText, scoreCounter;
    [SerializeField] private AnimationCurve overshootCurve, overshootOutCurve;
    
    private int _levelStage, _chordsSpawned, _correctChords;
    private List<string[]> chords;
    private List<string> _playedNotes;
    private List<GameObject> _stars;
    private string[] _lastChord;

    protected override void OnAwake()
    {
        FallingChordController.Destroyed += SpawnNewChord;
        PianoKeyController.NotePlayed += NotePlayedCallback;
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {retryButton, RetryButtonCallback }
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {niceText, true },
            {scoreCounter, true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true },
            {retryButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
        chords = new List<string[]>
        {
            new string[]{ "C", "E", "G" },
            new string[]{ "F", "A", "C" },
            new string[]{ "G", "B", "D" },
        };
        _lastChord = chords[0];
        _playedNotes = new List<string>();
        _stars = new List<GameObject>();
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 1f));
    }

    protected override void DestroyManager()
    {
        FallingChordController.Destroyed -= SpawnNewChord;
        PianoKeyController.NotePlayed -= NotePlayedCallback;
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;        
        if (_levelStage < 2)
        {
            StartCoroutine(AdvanceLevelStage());
        }
        else
        {
            Persistent.sceneToLoad = "MinorTriads";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }        
    }

    private void RetryButtonCallback(GameObject g)
    {
        _correctChords = 0;
        _chordsSpawned = 0;
        scoreCounter.text = "Chords Complete: 0";
        int index = 0;
        foreach (var star in _stars)
        {
            StartCoroutine(FadeInObjectScale(star, overshootOutCurve, false, 0.3f, 0.2f * index));
            index++;
        }
        _stars.Clear();        
        StartCoroutine(FadeText(introText, false, 0.5f));
        StartCoroutine(FadeButtonText(retryButton, false, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
        SpawnNewChord();
    }

    private void NotePlayedCallback(string note)
    {
        _playedNotes.Add(note.Substring(0, 1));
        if(_playedNotes.Count == 3)
        {            
            if (Enumerable.SequenceEqual(_playedNotes.OrderBy(n => n), _lastChord.OrderBy(n => n)))
            {
                niceText.text = "Nice!";
                niceText.color = new Color(0.32f, 0.57f, 0.47f);
                StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.3f, true));
                StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait: 1f));
                _correctChords++;
                scoreCounter.text = $"Chords Complete: {_correctChords}";
            }
            else
            {
                niceText.text = "Oops!";
                niceText.color = new Color(0.76f, 0.43f, 0.41f);
                StartCoroutine(TextFadeSize(niceText, overshootCurve, 0.3f, true));
                StartCoroutine(TextFadeSize(niceText, overshootOutCurve, 0.3f, false, wait: 1f));
            }            
            _playedNotes.Clear();
            StartCoroutine(BlockNotes());
        }
    }

    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(FadeText(scoreCounter, true, 0.5f));
                SpawnNewChord();
                piano.GetComponent<PianoController>().Show(3, showFlats: false, clickable: false);
                break;
        }
        yield return null;
    }    

    private void LevelComplete()
    {
        if (_correctChords > 0)
        {            
            int stars;
            if (_correctChords < 3)
            {
                stars = 1;
            }
            else if (_correctChords < 6)
            {
                stars = 2;
            }
            else
            {
                stars = 3;
            }
            string readout = stars > 1 ? "stars" : "star";
            introText.text = $"Awesome {Persistent.userName}! You matched {_correctChords} chords and got {stars} {readout}. You can try again, or move into the next lesson.";            
            StartCoroutine(FadeText(introText, true, 0.5f));            
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
            StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            List<Vector2> starPositions = new List<Vector2>();
            switch (stars)
            {
                case 1:
                    starPositions.Add(new Vector2(0, -260));
                    break;
                case 2:
                    starPositions.Add(new Vector2(-50, -260));
                    starPositions.Add(new Vector2(50, -260));
                    break;
                case 3:
                    starPositions.Add(new Vector2(-70, -260));
                    starPositions.Add(new Vector2(0, -260));
                    starPositions.Add(new Vector2(70, -260));
                    break;
            }
            _stars = new List<GameObject>();
            for (int i = 0; i < stars; i++)
            {
                _stars.Add(Instantiate(starPrefab, mainContainer.transform));
                _stars[i].transform.localPosition = starPositions[i];
                _stars[i].GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
                StartCoroutine(FadeInObjectScale(_stars[i], overshootCurve, true, 0.3f, wait: (0.2f * i)));
            }
            if (stars > Persistent.harmonyLessons.scores["Major Triads"])
            {
                Persistent.harmonyLessons.scores["Major Triads"] = stars;
                Persistent.harmonyLessons.lessons["Minor Triads"] = true;
                Persistent.UpdateLessonAvailability("Harmony");
            }
        }
        else
        {
            introText.text = "Looks like you didn't match any chords correctly. Press retry to try again.";            
            StartCoroutine(FadeText(introText, true, 0.5f));            
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
        }
    }

    private void SpawnNewChord()
    {
        if(_chordsSpawned == 10)
        {
            LevelComplete();
        }
        else
        {
            _chordsSpawned++;
            var newChord = Instantiate(chordPrefab, mainContainer.transform);
            newChord.transform.localPosition = new Vector3(0, 600);
            var random = new System.Random();
            var ch = chords[random.Next(chords.Count)];
            while(ch[0] == _lastChord[0])
            {
                ch = chords[random.Next(chords.Count)];
            }
            _lastChord = ch;
            newChord.GetComponent<FallingChordController>().Show(_lastChord);
            _playedNotes.Clear();
        }        
    }

    private IEnumerator BlockNotes()
    {
        piano.GetComponent<PianoController>().SetPlayable(false);
        yield return new WaitForSeconds(1);
        piano.GetComponent<PianoController>().SetPlayable(true);
    }
}
