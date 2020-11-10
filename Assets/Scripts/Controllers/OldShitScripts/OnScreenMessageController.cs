using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class OnScreenMessageController : MonoBehaviour
{
    public Text displayText;
    public Text buttonText;
    public GameObject message;
    public Button button;

    private void Awake()
    {        
        message.GetComponent<RawImage>().color = new Color(1, 1, 1, 0);        
        displayText.color = new Color(0.196f, 0.196f, 0.196f, 0);
        button.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        buttonText.color = new Color(0.196f, 0.196f, 0.196f, 0);
        StartCoroutine(FadeIn());            
    }

    private void OnDestroy()
    {
        //StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        while (displayText.color.a <= 1.0f)
        {
            displayText.color = new Color(displayText.color.r, displayText.color.g, displayText.color.b, displayText.color.a + (Time.deltaTime / 1));
            message.GetComponent<RawImage>().color = new Color(message.GetComponent<RawImage>().color.r, message.GetComponent<RawImage>().color.g, message.GetComponent<RawImage>().color.b, message.GetComponent<RawImage>().color.a + (Time.deltaTime / 1));
            button.GetComponent<Image>().color = new Color(1, 1, 1, button.GetComponent<Image>().color.a + (Time.deltaTime / 1));
            buttonText.color = new Color(0.196f, 0.196f, 0.196f, buttonText.color.a + (Time.deltaTime / 1));
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        while(displayText.color.a >= 0)
        {
            displayText.color = new Color(displayText.color.r, displayText.color.g, displayText.color.b, displayText.color.a - (Time.deltaTime / 1));
            yield return null;
        }
    }
}
