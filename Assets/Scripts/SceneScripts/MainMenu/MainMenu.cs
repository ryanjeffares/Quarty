using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenu : BaseManager
{
    [SerializeField] private GameObject playButton, settingsButton, statsButton, quitButton;
    [SerializeField] private GameObject settingsPage, coursesPage;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Button devButton;
    private GameObject _settings, _courses;
    
    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {playButton, PlayButtonCallback},
            {settingsButton, SettingsButtonCallback},
            {statsButton, StatsButtonCallback},
            {quitButton, QuitButtonCallback}
        };
        devButton.onClick.AddListener(() =>
        {
            foreach(var dir in Directory.GetDirectories(Application.persistentDataPath + "/Files"))
            {
                Directory.Delete(dir, true);
                Application.Quit();
            }
        });
    }

    protected override void OnStart()
    {
        audioMixer.SetFloat("MasterVolume", Persistent.settings.valueSettings["Volume"]);
    }

    private void PlayButtonCallback(GameObject g)
    {
        _courses = Instantiate(coursesPage, transform.GetChild(0));
        _courses.transform.GetChild(0).localScale = new Vector3(1, 1);
        _courses.transform.localScale = new Vector3(1.2f, 1.2f);
    }

    private void SettingsButtonCallback(GameObject g)
    {
        _settings = Instantiate(settingsPage, transform.GetChild(0));
        _settings.transform.GetChild(0).localScale = new Vector3(1, 1);
        _settings.transform.localScale = new Vector3(1.2f, 1.2f);
    }

    private void StatsButtonCallback(GameObject g)
    {
        
    }
    
    private void QuitButtonCallback(GameObject g)
    {
        Application.Quit();
    }
}
