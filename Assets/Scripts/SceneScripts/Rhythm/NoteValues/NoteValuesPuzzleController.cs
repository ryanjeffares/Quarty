using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NoteValuesPuzzleController : BaseManager
{
    [SerializeField] private Text introText;
    [SerializeField] private GameObject nextButton, quartersButton, eighthsButton, sixteenthsButton;
    [SerializeField] private GameObject drumContainer, drumPrefab;

    private readonly System.Random _random = new System.Random();
    private DrumKitController _drumkitController;
    private GameObject _drumkit;
    private GameObject Drumkit
    {
        get => _drumkit;
        set
        {
            _drumkit = value;
            _drumkitController = _drumkit.GetComponent<DrumKitController>();
        }
    }

    private int _levelStage;

    enum Patterns
    {
        KickQuarters,
        KickEights,
        SnareEights,
        SnareSixteenths,
        HatsEights,
        HatsSixteenths,
        TomsEights,
        TomsSixteenths
    }

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {quartersButton, PatternButtonCallback },
            {eighthsButton, PatternButtonCallback },
            {sixteenthsButton, PatternButtonCallback }
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {nextButton.GetComponentInChildren<Text>(), true },
            {quartersButton.GetComponentInChildren<Text>(), true },
            {eighthsButton.GetComponentInChildren<Text>(), true },
            {sixteenthsButton.GetComponentInChildren<Text>(), true }
        };
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f));
        Drumkit = Instantiate(drumPrefab, drumContainer.transform);
        Drumkit.transform.localPosition = new Vector3(0, 0);
        _drumkitController.Show(clickable: false);
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        StartCoroutine(AdvanceLevelStage());
    }

    private void PatternButtonCallback(GameObject g)
    {

    }

    protected override IEnumerator AdvanceLevelStage()
    {        
        switch (_levelStage)
        {
            case 1:
                break;
        }
        yield return null;
    }

    private void PlayPattern()
    {        
        int idx = _random.Next(8);
        var pattern = (Patterns)idx;
    }
}
