using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class MainMenu : BaseManager
{
    [SerializeField] private GameObject playButton, settingsButton, statsButton, quitButton;
    [SerializeField] private GameObject settingsPage, coursesPage;
    [SerializeField] private AudioMixer audioMixer;
    
    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {playButton, PlayButtonCallback},
            {settingsButton, SettingsButtonCallback},
            {statsButton, StatsButtonCallback},
            {quitButton, QuitButtonCallback}
        };
    }

    protected override void OnStart()
    {
        audioMixer.SetFloat("MasterVolume", Settings.valueSettings["Volume"]);
    }

    private void PlayButtonCallback(GameObject g)
    {
        Instantiate(coursesPage, transform);
    }

    private void SettingsButtonCallback(GameObject g)
    {
        Instantiate(settingsPage, transform);
    }

    private void StatsButtonCallback(GameObject g)
    {
        
    }
    
    private void QuitButtonCallback(GameObject g)
    {
        Application.Quit();
    }
}
