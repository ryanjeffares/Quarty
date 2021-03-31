using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : BaseManager
{
    public static event Action Unpaused;
    [SerializeField] private GameObject resume, settings, home, quit, glossary;
    [SerializeField] private GameObject settingsPage, glossaryPage;
    
    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {resume, ResumeButtonCallback},
            {settings, SettingsButtonCallback},
            {home, HomeButtonCallback},
            {quit, QuitButtonCallback},
            {glossary, GlossaryButtonCallback }
        };
    }

    private void ResumeButtonCallback(GameObject g)
    {
        Unpaused?.Invoke();
        Destroy(gameObject);
    }

    private void SettingsButtonCallback(GameObject g)
    {
        Instantiate(settingsPage, transform);
    }

    private void HomeButtonCallback(GameObject g)
    {
        Unpaused?.Invoke();
        Persistent.sceneToLoad = "MainMenu";
        Persistent.goingHome = true;
        SceneManager.LoadScene("LoadingScreen");
    }
    
    private void GlossaryButtonCallback(GameObject g)
    {
        Instantiate(glossaryPage, transform);
    }

    private void QuitButtonCallback(GameObject g)
    {
        Application.Quit();
    }
}
