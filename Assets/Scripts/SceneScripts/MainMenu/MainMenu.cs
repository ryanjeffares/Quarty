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
        StartCoroutine(LoadSettings());
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

    private IEnumerator LoadSettings()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Application.dataPath + "/Resources/Files/usersettings.xml");
        XmlNode rootNode = xmlDoc.FirstChild;
        foreach (XmlNode setting in rootNode)
        {
            if (setting.Attributes != null && setting.Attributes[0].Value == "Volume")
            {
                float val = float.Parse(setting.Attributes[1].Value);
                audioMixer.SetFloat("MasterVolume", val);
            }
        }
        yield return null;
    }
}
