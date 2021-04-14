using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TempoPuzzleController : BaseManager
{
    [SerializeField] private Text introText, tempoText;
    [SerializeField] private GameObject nextButton, playButton;    
    [SerializeField] private GameObject mainContainer;    
    [SerializeField] private AnimationCurve overshootCurve, overshootOutCurve;
    [SerializeField] private GameObject drumPiecePrefab;
    [SerializeField] private GameObject sequencer;

    private bool _success;
    private bool _playing, _ready;
    private List<GameObject> _kicks, _snares, _hats;
    private int _levelStage;
    private List<Vector3> _correctHatPositions = new List<Vector3>
    {
        new Vector3(-210, -60),
        new Vector3(-150, -60),
        new Vector3(-90, -60),
        new Vector3(-30, -60),
        new Vector3(30, -60),
        new Vector3(90, -60),
        new Vector3(150, -60),
        new Vector3(210, -60)
    };
    private List<Vector3> _correctSnarePositions = new List<Vector3>
    {
        new Vector3(-90, 0),
        new Vector3(150, 0)
    };
    private List<Vector3> _correctKickPositions = new List<Vector3>
    {
        new Vector3(-210, 60),
        new Vector3(30, 60)
    };

    protected override void OnAwake()
    {
        DrumSequencerController.PlayerDone += PlayerDoneCallback;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },            
            {playButton, PlayButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {tempoText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {playButton.GetComponentInChildren<Text>(), true }
        };
        _kicks = new List<GameObject>();
        _snares = new List<GameObject>();
        _hats = new List<GameObject>();
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
    }

    protected override void DestroyManager()
    {
        DrumSequencerController.PlayerDone -= PlayerDoneCallback;
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
            Persistent.rhythmLessons.scores["Tempo"] = 3;
            Persistent.rhythmLessons.lessons["Time Signatures"] = true;
            Persistent.UpdateLessonAvailability("Rhythm");
            Persistent.sceneToLoad = "TimeSignatures";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void PlayButtonCallback(GameObject g)
    {
        if (_playing || !_ready || _levelStage < 1) return;
        _playing = true;
        if (!_success)
        {
            StartCoroutine(FadeText(introText, false, 0.5f));
        }            
        sequencer.GetComponent<DrumSequencerController>().Playing = true;
        sequencer.GetComponent<DrumSequencerController>().PlaySequence(_levelStage == 1 ? 2 : 2.666f);            
    }

    private void PlayerDoneCallback()
    {
        _playing = false;
        if (!_success)
        {
            CheckArrangement();
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
                introText.text = "Each square is an Eighth Note, and this is 120 BPM. Arrange the drums so that:\nKicks are on beats 1 and 3.\nSnares are on beats 2 and 4.\nHats are on every eighth note.\n \nYou might not have to use all the drums! Hit Play to hear it.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeText(tempoText, true, 0.5f));
                //StartCoroutine(FadeButtonText(nextButton, true, 0.5f, wait: 2f));
                StartCoroutine(FadeButtonText(playButton, true, 0.5f, wait: 2f));
                SpawnDrums();
                sequencer.GetComponent<DrumSequencerController>().FadeInScale();
                _ready = true;
                break;
            case 2:
                _ready = false;
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
                introText.text = "The tempo is now 90 BPM. Try this arrangement:\nKicks on every beat.\nSnares on beat 2, beat 4, and the last eighth note.\nHats on every second eighth note and beat 4.\nHit Play to try it!";
                StartCoroutine(FadeText(introText, true, 0.5f));
                tempoText.text = "90 BPM";
                _ready = true;
                _success = false;
                _correctHatPositions = new List<Vector3>
                {                    
                    new Vector3(-150, -60),
                    new Vector3(-30, -60),
                    new Vector3(90, -60),
                    new Vector3(150, -60),
                    new Vector3(210, -60)
                };
                _correctSnarePositions = new List<Vector3>
                {
                    new Vector3(-90, 0),
                    new Vector3(150, 0),
                    new Vector3(210, 0)
                };
                _correctKickPositions = new List<Vector3>
                {
                    new Vector3(-210, 60),
                    new Vector3(-90, 60),
                    new Vector3(30, 60),
                    new Vector3(150, 60)
                };
                break;

        }
        yield return null;
    }

    private void TimerEnd()
    {

    }

    private void SpawnDrums()
    {        
        for(int i = 0; i < 4; i++)
        {
            var kick = Instantiate(drumPiecePrefab, sequencer.transform);
            kick.transform.localPosition = new Vector3(-150 + (100 * i), -240);
            kick.GetComponent<DrumPieceMovableController>().Show(DrumType.Kick, i * 0.1f);
            _kicks.Add(kick);
            var snare = Instantiate(drumPiecePrefab, sequencer.transform);
            snare.transform.localPosition = new Vector3(-150 + (100 * i), -320);
            snare.GetComponent<DrumPieceMovableController>().Show(DrumType.Snare, i * 0.1f);
            _snares.Add(snare);
            var hat = Instantiate(drumPiecePrefab, sequencer.transform);
            var hat2 = Instantiate(drumPiecePrefab, sequencer.transform);
            hat.transform.localPosition = new Vector3(-150 + (100 * i), -400);
            hat2.transform.localPosition = new Vector3(-150 + (100 * i), -480);
            hat.GetComponent<DrumPieceMovableController>().Show(DrumType.HiHatClosed, i * 0.1f);
            hat2.GetComponent<DrumPieceMovableController>().Show(DrumType.HiHatClosed, i * 0.1f);
            _hats.Add(hat);
            _hats.Add(hat2);
        }
    }

    private void CheckArrangement()
    {        
        var hatPositions = new List<Vector3>();
        foreach(var h in _hats.Where(h => h.GetComponent<DrumPieceMovableController>().Snapped))
        {
            hatPositions.Add(h.transform.localPosition);
        }
        var hatSuccess = CompareVectorLists(hatPositions, _correctHatPositions);
        var snarePositions = new List<Vector3>();
        foreach(var s in _snares.Where(s => s.GetComponent<DrumPieceMovableController>().Snapped))
        {
            snarePositions.Add(s.transform.localPosition);
        }
        var snrSuccess = CompareVectorLists(snarePositions, _correctSnarePositions);
        var kickPositions = new List<Vector3>();
        foreach (var k in _kicks.Where(k => k.GetComponent<DrumPieceMovableController>().Snapped))
        {
            kickPositions.Add(k.transform.localPosition);
        }
        var kickSuccess = CompareVectorLists(kickPositions, _correctKickPositions);
        _success = kickSuccess && snrSuccess && hatSuccess;
        if (_success)
        {
            if(_levelStage == 1)
            {
                introText.text = "Nice job! Hit Next to try a new arrangement.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            }            
            else if(_levelStage == 2)
            {
                introText.text = $"Incredible, {Persistent.userName}! You can keep playing with the drums, and hit Next to move into the next lesson whenever you're ready.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            }
        }
        else
        {
            if(_levelStage == 1)
            {
                introText.text = "That wasn't quite right! Here are the instructions again:\nKicks on beats 1 and 3.\nSnares on beats 2 and 4.\nHats on every eighth note.\nHit play to try again when you think you have it!";
                StartCoroutine(FadeText(introText, true, 0.5f));
            }
            else if(_levelStage == 2)
            {
                introText.text = "That wasn't quite right! Here are the instructions again:\nKicks on every beat.\nSnares on beat 2, beat 4, and the last eighth note.\nHats on every second eighth note and beat 4.\nHit play to try again when you think you have it!";
                StartCoroutine(FadeText(introText, true, 0.5f));
            }
        }
    }

    private bool CompareVectorLists(List<Vector3> l1, List<Vector3> l2)
    {
        if (l1.Count != l2.Count) return false;
        foreach(var v in l1)
        {
            bool found = false;
            foreach(var v2 in l2)
            {
                if(v2 == v)
                {
                    found = true;
                }
            }
            if (!found)
            {
                return false;
            }
        }
        return true;
    }
}
