using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;

public class XylophoneKeyController : MonoBehaviour, IPointerDownHandler
{
    public static event Action<GameObject> XylophoneKeyClicked;
    public string note;
    public float waitTime;
    private Text _text;
    private Color _keyColour, _textColour;    

    private void Awake()
    {
        _text = gameObject.transform.GetChild(0).GetComponent<Text>();
        _text.text = note.ToUpper();
        
        _textColour = _text.color;
        _keyColour = GetComponent<Image>().color;
        
        _text.color = Color.clear;
        GetComponent<Image>().color = new Color(_keyColour.r, _keyColour.g, _keyColour.b, 0);
        
        StartCoroutine(FadeIn(1f));
    }

    private IEnumerator FadeIn(float time)
    {
        float counter = 0f;
        float resolution = time / 0.016f;
        while (counter <= waitTime)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            counter += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        float interval = time / resolution;
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            _text.color = Color.Lerp(_text.color, _textColour, timeCounter / time);
            GetComponent<Image>().color = Color.Lerp(GetComponent<Image>().color, _keyColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RuntimeManager.PlayOneShot("event:/Xylophone/" + note);        
    }
}
