using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MelodyIntroductionController : BaseManager
{
    [SerializeField] private Text introText;
    [SerializeField] private Text notesText;
    [SerializeField] private GameObject nextButton, xylophonePrefab;
    
    private GameObject _xylophone;
    private Dictionary<Text, bool> _canTextLerp;
    private int _levelStage;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback}
        };
        _canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true},
            {notesText, true},
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true}
        };
        StartCoroutine(FadeText(introText, true, 0.5f, 200f));
        StartCoroutine(FadeText(notesText, true, 0.5f, 200f, 1.5f));
        StartCoroutine(FadeButtonText(true, 0.5f, 200f, 4f));
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        if (_levelStage < 3)
        {
            StartCoroutine(AdvanceLevelStage());   
        }
        else
        {
            Persistent.sceneToLoad = "Notes";
            Persistent.goingHome = false;
            Persistent.melodyLessons.lessons["Notes"] = true;
            Persistent.UpdateLessonAvailability("Melody");
            SceneManager.LoadScene("LoadingScreen");
        }
    }

    private IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f, 200f));
                StartCoroutine(FadeText(notesText, false, 0.5f, 200f, 0f, true));
                StartCoroutine(FadeButtonText( false, 0.5f, 200f));
                float counter = 0f;
                while (counter <= 1.5f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }

                    counter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text =
                    "More on that later, for now have a play with the notes. Pay attention to which notes sound good together, and which don't.";
                StartCoroutine(FadeText(introText, true, 0.5f, 200f));
                _xylophone = Instantiate(xylophonePrefab, transform.GetChild(0));
                _xylophone.transform.localPosition = new Vector3(0, -180, 0);
                StartCoroutine(FadeText(nextButton.transform.GetChild(0).GetComponent<Text>(), true, 0.5f, 200f, 5f));
                break;
            case 2:
                StartCoroutine(FadeText(introText, false, 0.5f, 200f));
                StartCoroutine(FadeButtonText(false, 0.5f, 200f));
                counter = 0f;
                while (counter <= 1.5f)
                {
                    if (PauseManager.paused)
                    {
                        yield return new WaitUntil(() => !PauseManager.paused);
                    }

                    counter += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                introText.text = "Whenever you're ready, let's move into the first lesson!";
                StartCoroutine(FadeText(introText, true, 0.5f, 200f));
                StartCoroutine(FadeButtonText(true, 0.5f, 200f, 1f));
                break;
        }
    }

    private IEnumerator FadeText(Text text, bool fadeIn, float time, float resolution, float wait = 0f, bool destroy = false)
    {
        yield return new WaitUntil(() => _canTextLerp[text]);
        _canTextLerp[text] = false;
        var startColour = text.color;
        var targetColour = new Color(0.196f, 0.196f, 0.196f, fadeIn ? 1f : 0f);

        if (wait > 0f)
        {
            float waitInterval = wait / resolution;
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            }   
        }

        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            text.color = Color.Lerp(startColour, targetColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }

        _canTextLerp[text] = true;
        if (destroy)
        {
            _canTextLerp.Remove(text);
            Destroy(text);
        }
    }
    
    private IEnumerator FadeButtonText(bool fadeIn, float time, float resolution, float wait = 0f)
    {
        if (buttonCallbackLookup.ContainsKey(nextButton))
        {
            buttonCallbackLookup.Remove(nextButton);
        }
        yield return new WaitUntil(() => _canTextLerp[nextButton.transform.GetChild(0).GetComponent<Text>()]);
        _canTextLerp[nextButton.transform.GetChild(0).GetComponent<Text>()] = false;
        var startColour = nextButton.transform.GetChild(0).GetComponent<Text>().color;
        var targetColour = new Color(0.196f, 0.196f, 0.196f, fadeIn ? 1f : 0f);

        if (wait > 0f)
        {
            float waitInterval = wait / resolution;
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            }   
        }

        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            nextButton.transform.GetChild(0).GetComponent<Text>().color = Color.Lerp(startColour, targetColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }

        _canTextLerp[nextButton.transform.GetChild(0).GetComponent<Text>()] = true;
        if(fadeIn)
        {
            buttonCallbackLookup.Add(nextButton, NextButtonCallback);
        }
    }
}
