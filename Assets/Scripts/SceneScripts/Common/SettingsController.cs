using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : BaseManager
{
    [SerializeField] private GameObject volumeSlider, cancelButton, acceptButton, settingsObject;
    [SerializeField] private AudioMixer audioMixer;
    private float _initVolumeSliderVal;

    protected override void OnAwake()
    {
        sliderCallbackLookup = new Dictionary<GameObject, Action<GameObject, float>>
        {
            {volumeSlider, VolumeSliderCallback}
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {cancelButton, CancelButtonCallback},
            {acceptButton, AcceptButtonCallback}
        };
        // This function only loads settings on to their UI elements, actual values are set on the main menu script
        LoadSettings();
    }

    private void VolumeSliderCallback(GameObject g, float value)
    {
        audioMixer.SetFloat("MasterVolume", value);
    }

    private void CancelButtonCallback(GameObject g)
    {
        // Need to discard changes to settings if user cancels instead of accepts
        volumeSlider.GetComponent<Slider>().value = _initVolumeSliderVal;
        Destroy(settingsObject);
    }

    private void AcceptButtonCallback(GameObject g)
    {
        SaveSettings();
        Exit();
    }

    private void LoadSettings()
    {
        volumeSlider.GetComponent<Slider>().value = Persistent.settings.valueSettings["Volume"];
        _initVolumeSliderVal = volumeSlider.GetComponent<Slider>().value;
    }

    private void SaveSettings()
    {
        Persistent.settings.valueSettings["Volume"] = volumeSlider.GetComponent<Slider>().value;
        Persistent.UpdateSettings();
    }

    private void Exit()
    {
        Destroy(settingsObject);
    }
}
