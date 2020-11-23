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
    private bool _saved;
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
        StartCoroutine(SaveSettings());
        StartCoroutine(Exit());
    }

    private void LoadSettings()
    {
        volumeSlider.GetComponent<Slider>().value = Settings.valueSettings["Volume"];
    }

    private IEnumerator SaveSettings()
    {
        string path = Application.dataPath + "/Resources/Files/usersettings.xml";
        
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement rootNode = xmlDoc.CreateElement("Settings");
        XmlElement volumeNode = xmlDoc.CreateElement("Setting");
        volumeNode.SetAttribute("name", "Volume");
        volumeNode.SetAttribute("value", volumeSlider.GetComponent<Slider>().value.ToString());
        Settings.valueSettings["Volume"] = volumeSlider.GetComponent<Slider>().value;

        xmlDoc.AppendChild(rootNode);
        rootNode.AppendChild(volumeNode);
        
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
        xmlDoc.Save(path);
        _saved = true;
        yield return null;
    }

    private IEnumerator Exit()
    {
        while (!_saved) yield return null;
        Destroy(settingsObject);
    }
}
