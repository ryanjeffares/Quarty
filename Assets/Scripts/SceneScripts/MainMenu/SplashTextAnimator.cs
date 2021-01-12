using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashTextAnimator : MonoBehaviour
{
    private Text _text;
    private readonly System.Random _random = new System.Random();

    private void Awake()
    {
        _text = GetComponent<Text>();
        _text.text = Persistent.SplashTexts.texts[_random.Next(Persistent.SplashTexts.texts.Count)];
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        while (enabled)
        {
            for (int i = 0; i < 360; i+=2)
            {
                float value = (float) Persistent.sineWaveValues[i];
                transform.localScale = new Vector3(1 + (value * 0.1f), 1 + (value * 0.1f));
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }
}
