using System;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : BaseManager
{
    [SerializeField] private GameObject pauseMenu, pauseButton;
    private bool _paused;
    
    protected override void OnAwake()
    {
        PauseMenu.Unpaused += UnpausedCallback;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {pauseButton, PauseButtonCallback}
        };
    }

    private void PauseButtonCallback(GameObject g)
    {
        if(!_paused)
            Instantiate(pauseMenu);
        _paused = true;
    }

    private void UnpausedCallback()
    {
        _paused = false;
    }
}