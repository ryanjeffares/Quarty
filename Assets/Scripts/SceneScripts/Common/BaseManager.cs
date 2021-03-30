using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseManager : MonoBehaviour
{
    protected Dictionary<GameObject, Action<GameObject>> buttonCallbackLookup;
    protected Dictionary<GameObject, Action<GameObject>> fullCallbackLookup;
    protected Dictionary<GameObject, Action<GameObject, float>> sliderCallbackLookup;
    protected Dictionary<Text, bool> canTextLerp;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        ButtonClicked.OnButtonClicked += ButtonClickedCallback;
        SliderChanged.OnSliderChanged += SliderChangedCallback;
        
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        sliderCallbackLookup = new Dictionary<GameObject, Action<GameObject, float>>();
        canTextLerp = new Dictionary<Text, bool>();
        
        OnAwake();
    }
    
    protected abstract void OnAwake();
    
    private void Start()
    {
        OnStart();
    }
    
    protected virtual void OnStart(){}

    protected virtual IEnumerator AdvanceLevelStage()
    {
        yield return null;
    }

    protected virtual IEnumerator MoveObject(GameObject obj, Vector2 target, float time,
        float wait = 0f,
        bool disableTrigger = false, 
        bool destroy = false)
    {
        yield return null;
    }

    protected virtual IEnumerator MoveObjectLog(GameObject obj, Vector2 target, float time,
        float wait = 0f,
        bool disableTrigger = false, 
        bool reset = false, 
        bool destroy = false)
    {
        yield return null;
    }

    private void OnDestroy()
    {
        ButtonClicked.OnButtonClicked -= ButtonClickedCallback;
        SliderChanged.OnSliderChanged -= SliderChangedCallback;
        DestroyManager();
    }
    
    protected virtual void DestroyManager(){}

    protected virtual void ButtonClickedCallback(GameObject g)
    {
        if (buttonCallbackLookup.ContainsKey(g))
        {
            buttonCallbackLookup[g]?.Invoke(g);
        }
    }

    protected virtual void SliderChangedCallback(GameObject g, float value)
    {
        if (sliderCallbackLookup.ContainsKey(g))
        {
            sliderCallbackLookup[g]?.Invoke(g, value);
        }
    }
    
    protected virtual IEnumerator FadeText(Text text, bool fadeIn, float time, float wait = 0f, bool destroy = false, bool fadeOut = false, float duration = 0f)
    {
        yield return new WaitUntil(() => canTextLerp[text]);
        canTextLerp[text] = false;
        float resolution = time / 0.016f;
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
        text.color = targetColour;
        if (fadeOut)
        {
            yield return new WaitForSeconds(duration);
            timeCounter = 0f;
            while (timeCounter <= time)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                text.color = Color.Lerp(targetColour, startColour, timeCounter / time);
                timeCounter += interval;
                yield return new WaitForSeconds(interval);
            }
            text.color = new Color(0.196f, 0.196f, 0.196f, fadeIn ? 0f : 1f);
        }
        canTextLerp[text] = true;
        if (destroy)
        {
            canTextLerp.Remove(text);
            Destroy(text);
        }
    }
    protected virtual IEnumerator FadeButtonText(GameObject button, bool fadeIn, float time, float wait = 0f, float targetAlpha = 1f)
    {
        if (buttonCallbackLookup.ContainsKey(button))
        {
            buttonCallbackLookup.Remove(button);
        }
        yield return new WaitUntil(() => canTextLerp[button.transform.GetChild(0).GetComponent<Text>()]);
        canTextLerp[button.transform.GetChild(0).GetComponent<Text>()] = false;
        var startColour = button.transform.GetChild(0).GetComponent<Text>().color;
        var targetColour = new Color(0.196f, 0.196f, 0.196f, fadeIn ? targetAlpha : 0f);
        float resolution = time / 0.016f;
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
            button.transform.GetChild(0).GetComponent<Text>().color = Color.Lerp(startColour, targetColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        button.transform.GetChild(0).GetComponent<Text>().color = targetColour;
        canTextLerp[button.transform.GetChild(0).GetComponent<Text>()] = true;
        if(fadeIn)
        {
            buttonCallbackLookup.Add(button, fullCallbackLookup[button]);
        }
    }
    
    protected virtual IEnumerator TextFadeSize(Text text, AnimationCurve curve, float time, bool enlarge, float wait = 0f)
    {
        float resolution = time / 0.016f;
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

        var startScale = text.transform.localScale;
        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            var sc = text.transform.localScale;
            sc.x = startScale.x + (enlarge ? curve.Evaluate(timeCounter / time) : -curve.Evaluate(timeCounter / time));
            sc.y = startScale.y + (enlarge ? curve.Evaluate(timeCounter / time) : -curve.Evaluate(timeCounter / time));
            text.transform.localScale = sc;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        text.transform.localScale = enlarge ? new Vector2(1, 1) : new Vector2(0, 0);
    }
    
    protected virtual IEnumerator FadeInObjectScale(GameObject obj, AnimationCurve curve, bool fadeIn, float time, float wait = 0f)
    {
        yield return new WaitForSeconds(wait);
        float resolution = time / 0.016f;
        float timeCounter = 0f;
        float interval = time / resolution;
        var startScale = obj.transform.localScale;
        while (timeCounter <= time)
        {
            var scale = obj.transform.localScale;
            scale.x = startScale.x + (fadeIn ? curve.Evaluate(timeCounter / time) : -curve.Evaluate(timeCounter / time));
            scale.y = startScale.y + (fadeIn ? curve.Evaluate(timeCounter / time) : -curve.Evaluate(timeCounter / time));
            obj.transform.localScale = scale;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        obj.transform.localScale = fadeIn ? new Vector3(1, 1) : new Vector3(0, 0);
    }

    protected virtual IEnumerator FadeImage(Image img, Color startColour, Color targetColour, float time, float wait = 0f)
    {
        yield return new WaitForSeconds(wait);        
        float timeCounter = 0f;        
        while(timeCounter <= time)
        {
            img.color = Color.Lerp(startColour, targetColour, timeCounter / time);
            timeCounter += Time.deltaTime;
            yield return null;
        }
        img.color = targetColour;
    }
}
