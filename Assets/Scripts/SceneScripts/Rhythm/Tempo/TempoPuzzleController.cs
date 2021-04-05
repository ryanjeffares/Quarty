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
    [SerializeField] private GameObject nextButton, retryButton;
    [SerializeField] private List<GameObject> staticBeatTexts, clickableBeatTexts, tapTexts, niceTexts;
    [SerializeField] private AnimationCurve overshootCurve, overshootOutCurve;

    private int _levelStage;
    private int _nextIndex = 0;
    private bool _sequencePlaying, _clickTime;
    private float _currentTime;
    private float[] _correctTimes;
    private List<float> _playedTimes;

    protected override void OnAwake()
    {        
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {retryButton, RetryButtonCallback }
        };
        foreach(var t in clickableBeatTexts)
        {
            fullCallbackLookup.Add(t, TempoTextClickedCallback);
        }
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {tempoText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {retryButton.GetComponentInChildren<Text>(), true }
        };
        foreach(var (t, index) in tapTexts.WithIndex())
        {
            canTextLerp.Add(t.GetComponent<Text>(), true);
            StartCoroutine(FadeText(t.GetComponent<Text>(), true, 0.5f, wait: 0.1f * index));
        }
        foreach(var (t, index) in staticBeatTexts.WithIndex())
        {
            canTextLerp.Add(t.GetComponent<Text>(), true);
            StartCoroutine(FadeText(t.GetComponent<Text>(), true, 0.5f, wait: 0.1f * index));
        }
        foreach(var (t, index) in clickableBeatTexts.WithIndex())
        {
            canTextLerp.Add(t.GetComponentInChildren<Text>(), true);
            StartCoroutine(FadeButtonText(t, true, 0.5f, wait: 0.1f * index));
        }
        _playedTimes = new List<float>();
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
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
            Persistent.rhythmLessons.lessons["Note Values"] = true;
            Persistent.UpdateLessonAvailability("Harmony");
            Persistent.sceneToLoad = "NoteValues";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private void RetryButtonCallback(GameObject g)
    {
        StartCoroutine(FadeText(introText, false, 0.5f));
        StartCoroutine(FadeButtonText(retryButton, false, 0.5f));        
        _nextIndex = 0;
        _success = true;
        for(int i = 0; i < _playedTimes.Count; i++)
        {
            StartCoroutine(TextFadeSize(niceTexts[i].GetComponent<Text>(), overshootOutCurve, 0.2f, false, wait: i * 0.05f));
        }
        _playedTimes = new List<float>();
        if (_levelStage == 1)
        {            
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tempo/Click120bpm 2");
            StartCoroutine(HighlightTexts(120));            
        }
        else if(_levelStage == 2)
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/Tempo/Click90bpm 2");
            StartCoroutine(HighlightTexts(90));
        }
    }

    private bool _success = true;

    private void TempoTextClickedCallback(GameObject g)
    {        
        if (!_sequencePlaying || clickableBeatTexts.IndexOf(g) != _nextIndex) return;
        if(_levelStage == 1 || _levelStage == 2)
        {     
            _playedTimes.Add(_currentTime);         
            if(_playedTimes[_nextIndex] > _correctTimes[_nextIndex] + 0.3f)
            {
                niceTexts[_nextIndex].GetComponent<Text>().text = "Too late!";
                niceTexts[_nextIndex].GetComponent<Text>().color = new Color(0.92f, 0.28f, 0.32f);
                StartCoroutine(TextFadeSize(niceTexts[_nextIndex].GetComponent<Text>(), overshootCurve, 0.2f, true));
                _success = false;
            }            
            else if(_playedTimes[_nextIndex] < _correctTimes[_nextIndex] - 0.3f)
            {
                niceTexts[_nextIndex].GetComponent<Text>().text = "Too early!";
                niceTexts[_nextIndex].GetComponent<Text>().color = new Color(0.92f, 0.28f, 0.32f);
                StartCoroutine(TextFadeSize(niceTexts[_nextIndex].GetComponent<Text>(), overshootCurve, 0.2f, true));
                _success = false;
            }
            else
            {
                niceTexts[_nextIndex].GetComponent<Text>().text = "Nice!";
                niceTexts[_nextIndex].GetComponent<Text>().color = new Color(0.25f, 0.43f, 0.25f);
                StartCoroutine(TextFadeSize(niceTexts[_nextIndex].GetComponent<Text>(), overshootCurve, 0.2f, true));                
            }
            _nextIndex++;          
        }             
    }

    protected override IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                StartCoroutine(FadeText(tempoText, true, 0.5f));
                _correctTimes = new float[]
                {
                    0, 0.5f, 1, 1.5f, 2, 2.5f, 3, 3.5f
                };
                FMODUnity.RuntimeManager.PlayOneShot("event:/Tempo/Click120bpm 2");
                StartCoroutine(HighlightTexts(120));
                break;
            case 2:
                StartCoroutine(FadeText(introText, false, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, false, 0.5f));
                foreach(var (t, i) in niceTexts.WithIndex())
                {
                    StartCoroutine(TextFadeSize(t.GetComponent<Text>(), overshootOutCurve, 0.2f, false, wait: i * 0.01f));
                }
                tempoText.text = "90 BPM";
                _correctTimes = new float[]
                {
                    0, 0.666f, 1.333f, 2, 2.666f, 3.333f, 4, 4.666f
                };
                _playedTimes = new List<float>();
                _nextIndex = 0;
                _success = true;
                FMODUnity.RuntimeManager.PlayOneShot("event:/Tempo/Click90bpm 2");
                StartCoroutine(HighlightTexts(90));
                break;

        }
        yield return null;
    }

    private void TimerEnd()
    {
        if (_success)
        {
            if (_levelStage == 1)
            {
                introText.text = $"Nice job {Persistent.userName}! Hit next to try the other tempo.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            }
            else
            {
                introText.text = $"Amazing, {Persistent.userName}! Hit next to move into the next lesson.";
                StartCoroutine(FadeText(introText, true, 0.5f));
                StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
            }
        }
        else
        {
            introText.text = $"So close! Hit retry to try that tempo again.";
            StartCoroutine(FadeText(introText, true, 0.5f));
            StartCoroutine(FadeButtonText(retryButton, true, 0.5f));
        }
    }

    private IEnumerator HighlightTexts(int bpm)
    {
        _sequencePlaying = true;
        float beatTime = 60f / bpm;
        foreach (var (t, index) in staticBeatTexts.WithIndex())
        {
            StartCoroutine(ResizeText(t));            
            if(index < staticBeatTexts.Count - 1)
            {
                yield return new WaitForSeconds(beatTime);
            }            
        }
        float timeCounter = -beatTime;
        Debug.Log(timeCounter);
        foreach(var (t, index) in clickableBeatTexts.WithIndex())
        {
            StartCoroutine(ResizeText(t, wait: beatTime + (beatTime * index)));
        }
        while(timeCounter <= (bpm == 120 ? 4f : 5f))
        {
            _currentTime = timeCounter;
            timeCounter += Time.deltaTime;
            yield return null;
        }
        _sequencePlaying = false;
        TimerEnd();
    }

    private IEnumerator ResizeText(GameObject t, float wait = 0f)
    {
        if (wait > 0)
        {
            yield return new WaitForSeconds(wait);
        }
        float timeCounter = 0f;
        while (timeCounter <= 0.5f)
        {
            if (timeCounter <= 0.25f)
            {
                t.gameObject.transform.localScale = new Vector3(1 + (timeCounter * 2), 1 + (timeCounter * 2));
            }
            else
            {
                t.gameObject.transform.localScale = new Vector3(2 - (timeCounter * 2), 2 - (timeCounter * 2));
            }
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }
}
