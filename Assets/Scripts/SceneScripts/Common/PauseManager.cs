using System;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : BaseManager
{
    [SerializeField] private GameObject pauseMenu, pauseButton;
    public static bool paused;
    
    protected override void OnAwake()
    {
        PauseMenu.Unpaused += () => paused = false;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {pauseButton, (g) => 
                { 
                    if(!paused)
                    {
                        Instantiate(pauseMenu);
                        paused = true; 
                    } 
                } 
            }
        };
    }

    protected override void DestroyManager()
    {
        PauseMenu.Unpaused -= () => paused = false;
    }
}