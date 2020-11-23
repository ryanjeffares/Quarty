using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseManager : MonoBehaviour
{
    protected Dictionary<GameObject, Action<GameObject>> buttonCallbackLookup;
    protected Dictionary<GameObject, Action<GameObject, float>> sliderCallbackLookup;

    private void Awake()
    {
        ButtonClicked.OnButtonClicked += ButtonClickedCallback;
        SliderChanged.OnSliderChanged += SliderChangedCallback;
        
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        sliderCallbackLookup = new Dictionary<GameObject, Action<GameObject, float>>();
        
        OnAwake();
    }
    
    protected abstract void OnAwake();

    private void Start()
    {
        OnStart();
    }
    
    protected virtual void OnStart(){}

    private void OnDestroy()
    {
        ButtonClicked.OnButtonClicked -= ButtonClickedCallback;
        SliderChanged.OnSliderChanged -= SliderChangedCallback;
    }

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
}