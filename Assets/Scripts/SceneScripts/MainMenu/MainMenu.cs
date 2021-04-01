using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class MainMenu : BaseManager
{
    [SerializeField] private GameObject playButton, settingsButton, statsButton, glossaryButton, quitButton;
    [SerializeField] private GameObject settingsPage, coursesPage, glossaryPage, statsPage;    
    [SerializeField] private Button devButton;    
    [EventRef] private FMOD.Studio.EventInstance _musicEvent;    
    private GameObject _settings, _courses, _glossary, _stats;

    protected override void OnAwake()
    {
        SettingsController.ToggleDevButton += ToggleDevButtonCallback;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {playButton, PlayButtonCallback},
            {settingsButton, SettingsButtonCallback},
            {statsButton, StatsButtonCallback},
            {glossaryButton, GlossaryButtonCallback },
            {quitButton, QuitButtonCallback}
        };
        devButton.onClick.AddListener(() =>
        {
            foreach(var dir in Directory.GetDirectories(Application.persistentDataPath + "/Files"))
            {
                Directory.Delete(dir, true);                                
            }
            Debug.Log("Deleted local files");
            Application.Quit();
        });
        _musicEvent = RuntimeManager.CreateInstance("event:/Music/MenuMusic");        
    }

    protected override void DestroyManager()
    {
        SettingsController.ToggleDevButton -= ToggleDevButtonCallback;
        _musicEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    protected override void OnStart()
    {
        RuntimeManager.StudioSystem.setParameterByName("MusicVolume", Persistent.settings.valueSettings["MusicVolume"]);
        RuntimeManager.StudioSystem.setParameterByName("ObjectVolume", Persistent.settings.valueSettings["ObjectVolume"]);
        _musicEvent.start();        
    }

    private void ToggleDevButtonCallback(bool state)
    {
        devButton.gameObject.SetActive(state);
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
        _stats = Instantiate(statsPage, transform.GetChild(0));
        _stats.transform.GetChild(0).localScale = new Vector3(1, 1);
        _stats.transform.localScale = new Vector3(1.2f, 1.2f);
    }
    
    private void GlossaryButtonCallback(GameObject g)
    {
        _glossary = Instantiate(glossaryPage, transform.GetChild(0));
        _glossary.transform.GetChild(0).localScale = new Vector3(1, 1);
        _glossary.transform.localScale = new Vector3(1.2f, 1.2f);
    }

    private void QuitButtonCallback(GameObject g)
    {        
        Application.Quit();
    }
}
