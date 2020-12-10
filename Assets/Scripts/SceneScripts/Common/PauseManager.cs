using System;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : BaseManager
{
    [SerializeField] private GameObject pauseMenu, pauseButton;
    public static bool paused;
    
    protected override void OnAwake()
    {
        PauseMenu.Unpaused += UnpausedCallback;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {pauseButton, PauseButtonCallback}
        };
    }

    protected override void DestroyManager()
    {
        PauseMenu.Unpaused -= UnpausedCallback;
    }

    private void PauseButtonCallback(GameObject g)
    {
        Debug.Log("Pause clicked");
        if(!paused)
        {
            Instantiate(pauseMenu);
            paused = true;
        }
    }

    private void UnpausedCallback()
    {
        paused = false;
    }
}