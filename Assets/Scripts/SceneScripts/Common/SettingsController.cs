using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class SettingsController : BaseManager
{
    public static event Action<bool> ToggleDevButton;
    [SerializeField] private GameObject musicVolumeSlider, objectVolumeSlider, cancelButton, acceptButton;
    [SerializeField] private Toggle devButtonToggle;
    private float _initVolumeSliderVal, _initObjectSliderVal;

    protected override void OnAwake()
    {
        sliderCallbackLookup = new Dictionary<GameObject, Action<GameObject, float>>
        {
            {musicVolumeSlider, MusicVolumeSliderCallback},
            {objectVolumeSlider, ObjectVolumeSliderCallback}
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {cancelButton, CancelButtonCallback},
            {acceptButton, AcceptButtonCallback}
        };
        devButtonToggle.onValueChanged.AddListener((state) => ToggleDevButton?.Invoke(state));
        // This function only loads settings on to their UI elements, actual values are set on the main menu script
        LoadSettings();
    }

    private void MusicVolumeSliderCallback(GameObject g, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName("MusicVolume", value);
    }

    private void ObjectVolumeSliderCallback(GameObject g, float value)
    {
        RuntimeManager.StudioSystem.setParameterByName("ObjectVolume", value);
    }

    private void CancelButtonCallback(GameObject g)
    {
        // Need to discard changes to settings if user cancels instead of accepts
        musicVolumeSlider.GetComponent<Slider>().value = _initVolumeSliderVal;
        objectVolumeSlider.GetComponent<Slider>().value = _initObjectSliderVal;
        Destroy(gameObject);
    }

    private void AcceptButtonCallback(GameObject g)
    {
        SaveSettings();
        Exit();
    }

    private void LoadSettings()
    {
        musicVolumeSlider.GetComponent<Slider>().value = Persistent.settings.valueSettings["MusicVolume"];
        objectVolumeSlider.GetComponent<Slider>().value = Persistent.settings.valueSettings["ObjectVolume"];
        _initVolumeSliderVal = musicVolumeSlider.GetComponent<Slider>().value;
        _initObjectSliderVal = objectVolumeSlider.GetComponent<Slider>().value;
    }

    private void SaveSettings()
    {
        Persistent.settings.valueSettings["MusicVolume"] = musicVolumeSlider.GetComponent<Slider>().value;
        Persistent.settings.valueSettings["ObjectVolume"] = objectVolumeSlider.GetComponent<Slider>().value;
        Persistent.UpdateSettings();
    }

    private void Exit()
    {
        Destroy(gameObject);
    }
}
