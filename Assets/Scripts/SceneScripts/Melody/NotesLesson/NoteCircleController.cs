using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class NoteCircleController : MonoBehaviour
{
    [SerializeField] private Text text;
    private Vector2 _size;
    private RectTransform _rt;
    private Color _textColour, _circleColour; 
    public float waitTime;
    public string note;

    private void Awake()
    {
        _textColour = text.color;
        text.color = Color.clear;
        _circleColour = GetComponent<Image>().color;
        GetComponent<Image>().color = new Color(_circleColour.r, _circleColour.g, _circleColour.b, 0);
        _rt = GetComponent<RectTransform>();
        _size = _rt.sizeDelta;        
    }

    public void Show()
    {
        StartCoroutine(FadeIn(0.5f));
    }

    private IEnumerator FadeIn(float time)
    {
        if(waitTime > 0)
        {
            float counter = 0f;
            while (counter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                counter += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        float resolution = time / 0.016f;
        float interval = time / resolution;
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            text.color = Color.Lerp(text.color, _textColour, timeCounter / time);
            GetComponent<Image>().color = Color.Lerp(GetComponent<Image>().color, _circleColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RuntimeManager.PlayOneShot("event:/SineNotes/" + note);
        StartCoroutine(Resize(true));
    }    

    private void OnTriggerExit2D(Collider2D other)
    {        
        StartCoroutine(Resize(false));
    }

    private IEnumerator Resize(bool enlarge)
    {
        float target = enlarge ? _size.x * 1.1f : _size.x;
        if (enlarge)
        {            
            while (_rt.rect.width <= target)
            {
                var sizeDelta = _rt.sizeDelta;
                sizeDelta.x = sizeDelta.x > target ? target : sizeDelta.x + Time.deltaTime / 0.01f;
                sizeDelta.y = sizeDelta.y > target ? target : sizeDelta.y + Time.deltaTime / 0.01f;
                _rt.sizeDelta = sizeDelta;
                yield return null;
            }
        }
        else
        {
            while (_rt.rect.width >= target)
            {
                var sizeDelta = _rt.sizeDelta;
                sizeDelta.x = sizeDelta.x < target ? target : sizeDelta.x - Time.deltaTime / 0.01f;
                sizeDelta.y = sizeDelta.y < target ? target : sizeDelta.y - Time.deltaTime / 0.01f;
                _rt.sizeDelta = sizeDelta;
                yield return null;
            }
        }
    }
}
