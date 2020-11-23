using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class LoadingScreen : MonoBehaviour
{
    private readonly Random _random = new Random();
    [SerializeField] private Text text;
    
    private void Awake()
    {
        text.text = SharedData.loadingTexts[_random.Next(SharedData.loadingTexts.Length)];
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(1.5f);  
        AsyncOperation status = SceneManager.LoadSceneAsync(SharedData.sceneToLoad);
        while (!status.isDone)
        {
            yield return null;
        }
    }
}
