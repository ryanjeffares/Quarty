using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class SplashTextAnimator : MonoBehaviour
{
    private Text _text;
    private readonly Random _random = new Random();

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
            for (int i = 0; i < 360; i++)
            {
                float value = (float) Persistent.sineWaveValues[i];
                transform.localScale = new Vector3(1 + (value * 0.1f), 1 + (value * 0.1f));
                yield return new WaitForSeconds(0.005f);
            }
        }
    }
}
