using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class LoadingScreen : MonoBehaviour
{
    private readonly Random _random = new Random();
    [SerializeField] private Text text;
    [SerializeField] private Image background;
    
    private void Awake()
    {
        int index = _random.Next(Persistent.paletteColours.Count);
        background.color = Persistent.paletteColours.Values.ElementAt(index);
        if (Persistent.goingHome)
        {
            text.text = Persistent.LoadingTexts.loadingHome[_random.Next(Persistent.LoadingTexts.loadingHome.Count)];
        }
        else
        {
            text.text = Persistent.LoadingTexts.loadingLesson[_random.Next(Persistent.LoadingTexts.loadingLesson.Count)];   
        }
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(1.5f);  
        AsyncOperation status = SceneManager.LoadSceneAsync(Persistent.sceneToLoad);
        while (!status.isDone)
        {
            yield return null;
        }
    }
}
