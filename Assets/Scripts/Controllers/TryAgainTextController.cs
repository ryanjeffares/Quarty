using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TryAgainTextController : MonoBehaviour
{
    public Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
        StartCoroutine(FadeIn());        
    }

    private IEnumerator FadeIn()
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
        while(text.color.a <= 1.0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime / 0.5f));
            yield return null;
        }
        StartCoroutine(SelfDestruct());
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(3f);
        while (text.color.a >= 0f)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / 1));
            yield return null;
        }
        Destroy(gameObject);
    }
}
