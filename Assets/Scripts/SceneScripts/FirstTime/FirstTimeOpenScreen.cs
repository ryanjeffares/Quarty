using System;
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
    [SerializeField] private Text warningText, mainText;
    private List<char> otherAllowedCharacters;
    private bool _readyToContinue = false;

    protected override void OnAwake()
    {
        Debug.Log(Application.persistentDataPath);
        if (File.Exists(Application.persistentDataPath + "/Files/User/user.dat"))
        {
            using(BinaryReader br = new BinaryReader(File.Open(Application.persistentDataPath + "/Files/User/user.dat", FileMode.Open)))
            {
                Persistent.userName = br.ReadString();
            }
            Debug.Log(Persistent.userName);
            Persistent.sceneToLoad = "MainMenu";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
        else
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Files/User/");
        }
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {continueButton, ContinueButtonCallback}
        };
        canTextLerp = new Dictionary<Text, bool>
        {
            {mainText, true },
            {warningText, true }
        };
        otherAllowedCharacters = new List<char>
        {
            '-', '_'
        };
    }

    private void ContinueButtonCallback(GameObject g)
    {
        if (_readyToContinue)
        {
            Persistent.sceneToLoad = "MainMenu";
            Persistent.goingHome = false;
            SceneManager.LoadScene("LoadingScreen");
        }
        else
        {
            if (input.text != "")
            {
                if (!input.text.All(c => char.IsLetterOrDigit(c) || !char.IsWhiteSpace(c) || otherAllowedCharacters.Contains(c)))
                {
                    input.text = "";
                    warningText.text = "Illegal character found, try a different username.";
                    StartCoroutine(FadeText(warningText, true, 0.5f, fadeOut: true, duration: 1f));
                    return;
                }
                var filter = new ProfanityFilter.ProfanityFilter();
                if (filter.IsProfanity(input.text))
                {
                    input.text = "";
                    warningText.text = "Username not allowed! Try a different username.";
                    StartCoroutine(FadeText(warningText, true, 0.5f, fadeOut: true, duration: 1f));
                    return;
                }
                using (FileStream fs = new FileStream(Application.persistentDataPath + "/Files/User/user.dat", FileMode.CreateNew))
                {
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(input.text);
                    bw.Close();
                }
                Destroy(input.gameObject);
                StartCoroutine(FadeText(mainText, false, 0.5f));
                StartCoroutine(ShowMessage());                
            }
        }        
    }

    private IEnumerator ShowMessage()
    {
        yield return new WaitForSeconds(1f);
        mainText.text = "Quarty is a game I made for my degree research project to investigate if gamification can be used to enhance music theory education. If you want to help me with that, please consider filling in the survey linked through the settings page after playing for a while.\n \nI hope you enjoy Quarty, I had a lot of fun making it. Thank you for playing my game - I really appreciate it.";
        mainText.fontSize = 24;
        StartCoroutine(FadeText(mainText, true, 0.5f));
        _readyToContinue = true;
    }
}
