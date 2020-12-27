using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

public class NoteCirclesScaleEmptyController : MonoBehaviour
{
    [SerializeField] private List<GameObject> circles;
    [SerializeField] private List<Text> texts;

    private void Awake()
    {
        float waitTime = 0f;

        foreach (var circle in circles)
        {
            circle.GetComponent<Image>().color = Color.clear;
            StartCoroutine(FadeCircle(circle, 0.5f, 200f, waitTime));
            waitTime += 0.1f;
        }

        waitTime = 0f;
        foreach (var text in texts)
        {
            text.color = Color.clear;
            StartCoroutine(FadeText(text, 0.5f, 200f, waitTime));
            waitTime += 0.1f;
        }
    }

    private IEnumerator FadeCircle(GameObject circle, float time, float resolution, float waitTime)
    {
        if(waitTime > 0)
        {
            float counter = 0f;
            while (counter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }

                counter += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }

        var startColour = circle.GetComponent<Image>().color;
        var targetColourCircle = new Color(0.44f, 0.44f, 0.44f, 1f);

        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            circle.GetComponent<Image>().color = Color.Lerp(startColour, targetColourCircle, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator FadeText(Text text, float time, float resolution, float waitTime)
    {
        if(waitTime > 0)
        {
            float counter = 0f;
            while (counter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }

                counter += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        var startColour = text.color;
        var targetColour = new Color(0.196f, 0.196f, 0.196f, 1f);

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
    }
}