using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlossaryTabController : MonoBehaviour
{
    [SerializeField] private Image bg, arrow;
    [SerializeField] private Text text;

    public void Show(Color tabColour, float waitTime)
    {
        bg.color = Color.clear;
        arrow.color = Color.clear;
        text.color = Color.clear;        
        StartCoroutine(FadeIn(tabColour, waitTime));
    }

    private IEnumerator FadeIn(Color tabColour, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        var clear = Color.clear;
        var txtCol = new Color(0.196f, 0.196f, 0.196f);
        float timeCounter = 0f;
        while(timeCounter <= 0.5f)
        {
            bg.color = Color.Lerp(clear, tabColour, timeCounter / 0.5f);
            arrow.color = Color.Lerp(clear, Color.white, timeCounter / 0.5f);
            text.color = Color.Lerp(clear, txtCol, timeCounter / 0.5f);
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }
}
