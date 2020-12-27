﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirstTimeOpenScreen : BaseManager
{
    [SerializeField] private InputField input;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private Text warningText;

    protected override void OnAwake()
    {
        if (File.Exists(Application.dataPath + "/Resources/Files/User/username.txt"))
        {
            Persistent.sceneToLoad = "MainMenu";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
        else
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/Files/User/");
        }
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {continueButton, ContinueButtonCallback}
        }; 
    }

    private void ContinueButtonCallback(GameObject g)
    {
        if (input.text != "")
        {
            if (!input.text.All(c =>char.IsLetterOrDigit(c) || !char.IsWhiteSpace(c)))
            {
                input.text = "";
                warningText.text = "Illegal character found, try a different username.";
                StartCoroutine(FadeText(0.2f, 200f));
                return;
            }
            var filter = new ProfanityFilter.ProfanityFilter();
            if (filter.IsProfanity(input.text))
            {
                input.text = "";
                warningText.text = "Username not allowed! Try a different username.";
                StartCoroutine(FadeText(0.2f, 200f));
                return;
            }

            using (StreamWriter sw = File.CreateText(Application.dataPath + "/Resources/Files/User/username.txt"))
            {
                sw.WriteLine(input.text);
            }

            Persistent.sceneToLoad = "MainMenu";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private IEnumerator FadeText(float time, float resolution)
    {
        var startColour = warningText.color;
        var targetColour = new Color(0.196f, 0.196f, 0.196f, 1);
        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            warningText.color = Color.Lerp(startColour, targetColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        yield return new WaitForSeconds(1);
        timeCounter = 0f;
        while (timeCounter <= time)
        {
            warningText.color = Color.Lerp(targetColour, startColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }
}
