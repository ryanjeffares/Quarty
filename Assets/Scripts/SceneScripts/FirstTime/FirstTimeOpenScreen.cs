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
    [SerializeField] private Text warningText;
    private List<char> otherAllowedCharacters;

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
        otherAllowedCharacters = new List<char>
        {
            '-', '_'
        };
    }

    private void ContinueButtonCallback(GameObject g)
    {
        if (input.text != "")
        {
            if (!input.text.All(c =>char.IsLetterOrDigit(c) || !char.IsWhiteSpace(c) || otherAllowedCharacters.Contains(c)))
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
            using (FileStream fs = new FileStream(Application.persistentDataPath + "/Files/User/user.dat", FileMode.CreateNew))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(input.text);
                bw.Close();
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
