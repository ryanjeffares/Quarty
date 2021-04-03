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
    [SerializeField] private GameObject nextButton;
    [SerializeField] private List<GameObject> staticBeatTexts, clickableBeatTexts;

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
            {nextButton, NextButtonCallback }
        };
        foreach(var t in clickableBeatTexts)
        {
            fullCallbackLookup.Add(t, TempoTextClickedCallback);
        }
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {tempoText, true },
            {nextButton.GetComponentInChildren<Text>(), true }
        };
        foreach(var (t, index) in staticBeatTexts.WithIndex())
        {
            canTextLerp.Add(t.GetComponent<Text>(), true);
            StartCoroutine(FadeText(t.GetComponent<Text>(), true, 0.5f, wait: 0.1f * index));
        }
        foreach(var (t, index) in clickableBeatTexts.WithIndex())
        {
            canTextLerp.Add(t.GetComponentInChildren<Text>(), true);
            StartCoroutine(FadeText(t.GetComponentInChildren<Text>(), true, 0.5f, wait: 0.1f * index));
        }
        _playedTimes = new List<float>();
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        StartCoroutine(AdvanceLevelStage());
    }

    private void TempoTextClickedCallback(GameObject g)
    {
        Debug.Log("Function called");
        if (!_sequencePlaying || clickableBeatTexts.IndexOf(g) != _nextIndex) return;
        if(_levelStage == 1)
        {
            Debug.Log("Code Reached");
            _playedTimes.Add(_currentTime);
            Debug.Log(_currentTime);
            if(_playedTimes.Count == 8)
            {
                _nextIndex++;
            }            
        }
        else if(_levelStage == 2)
        {

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
        }
        yield return null;
    }

    private IEnumerator HighlightTexts(int bpm)
    {
        _sequencePlaying = true;
        float beatTime = 60f / bpm;
        foreach (var (t, index) in staticBeatTexts.WithIndex())
        {
            StartCoroutine(ResizeText(t.GetComponent<Text>()));            
            if(index < staticBeatTexts.Count - 1)
            {
                yield return new WaitForSeconds(beatTime);
            }            
        }
        float timeCounter = -0.5f;
        while(timeCounter <= 4f)
        {
            _currentTime = timeCounter;
            timeCounter += Time.deltaTime;
            yield return null;
        }
        _sequencePlaying = false;
    }

    private IEnumerator ResizeText(Text t)
    {
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
