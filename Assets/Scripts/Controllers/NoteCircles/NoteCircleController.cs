using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class NoteCircleController : MonoBehaviour
{
    [SerializeField] private Text text;
    [SerializeField] private AnimationCurve curve;
    private Vector2 _size;
    private RectTransform _rt;
    private Color _textColour, _circleColour; 

    [HideInInspector]
    public float waitTime;
    [HideInInspector]
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
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            text.color = Color.Lerp(text.color, _textColour, timeCounter / time);
            GetComponent<Image>().color = Color.Lerp(GetComponent<Image>().color, _circleColour, timeCounter / time);
            timeCounter += Time.deltaTime;
            yield return null;
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
        float timeCounter = 0f;
        while (timeCounter <= 0.1f)
        {
            var scaleDiff = 0.1f * curve.Evaluate(timeCounter / 0.1f);
            transform.localScale = enlarge ? new Vector3(1 + scaleDiff, 1 + scaleDiff) : new Vector3(1.1f - scaleDiff, 1.1f - scaleDiff);
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }
}
