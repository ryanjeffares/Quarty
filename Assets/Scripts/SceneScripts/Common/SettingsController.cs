using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsController : BaseManager
{
    [SerializeField] private GameObject volumeSlider, cancelButton, acceptButton, settingsObject;
    [SerializeField] private AudioMixer audioMixer;
    private bool _saved;

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
        StartCoroutine(LoadSettings());
    }

    private void VolumeSliderCallback(GameObject g, float value)
    {
        audioMixer.SetFloat("MasterVolume", value);
    }

    private void CancelButtonCallback(GameObject g)
    {
        Destroy(settingsObject);
    }

    private void AcceptButtonCallback(GameObject g)
    {
        StartCoroutine(SaveSettings());
        StartCoroutine(Exit());
    }

    private IEnumerator LoadSettings()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Application.dataPath + "/Resources/Files/usersettings.xml");
        XmlNode rootNode = xmlDoc.FirstChild;
        foreach (XmlNode setting in rootNode)
        {
            if (setting.Attributes != null && setting.Attributes[0].Value == "Volume")
            {
                volumeSlider.GetComponent<Slider>().value = float.Parse(setting.Attributes[1].Value);
            }
            yield return null;
        }
    }

    private IEnumerator SaveSettings()
    {
        string path = Application.dataPath + "/Resources/Files/usersettings.xml";
        
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement rootNode = xmlDoc.CreateElement("Settings");
        XmlElement volumeNode = xmlDoc.CreateElement("Setting");
        volumeNode.SetAttribute("name", "Volume");
        volumeNode.SetAttribute("value", volumeSlider.GetComponent<Slider>().value.ToString());

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
