using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NoteBlockController : MonoBehaviour, IPointerClickHandler
{        
    public Text displayText;
    public GameObject block;
    public Dictionary<string, List<int>> note;
    public bool octaveUp = false;
    public int id;
    private CsoundUnity csoundUnity;
    public float delay;

    private void Awake()
    {
        csoundUnity = GameObject.Find("CsoundInstance").GetComponent<CsoundUnity>();        
        note = new Dictionary<string, List<int>>();
        displayText.color = new Color(displayText.color.r, displayText.color.g, displayText.color.b, 0);
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {        
        yield return new WaitForSeconds(delay);
        while (displayText.color.a <= 1.0f)
        {
            displayText.color = new Color(displayText.color.r, displayText.color.g, displayText.color.b, displayText.color.a + (Time.deltaTime / 1));
            block.GetComponent<RawImage>().color = new Color(block.GetComponent<RawImage>().color.r, block.GetComponent<RawImage>().color.g, block.GetComponent<RawImage>().color.b, block.GetComponent<RawImage>().color.a + (Time.deltaTime / 1));
            yield return null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ExampleLevelController.clickedOrder.Add(id);
        ExampleLevelController.numClicks++;
        int octave = octaveUp ? 4 : 3;
        string s = "i\"ExamplePlayer\" 0 0.5 {0}";
        string noteName = "";        
        if (!ExampleLevelController.isNumberRound)
        {
            if (displayText.text.Contains("("))
            {
                noteName = displayText.text.Remove(displayText.text.IndexOf("("));
            }
            else
            {
                noteName = displayText.text;
            }            
        }
        else
        {
            foreach(KeyValuePair<string, List<int>> kvp in note)
            {
                noteName = kvp.Key;                
            }
        }
        string scoreLine = string.Format(s, note[noteName][octave]);
        csoundUnity.sendScoreEvent(scoreLine);        
        if (ExampleLevelController.numClicks == 8) ExampleLevelController.CheckSucces();
        StartCoroutine(AnimateOnClick());
    }

    private IEnumerator AnimateOnClick()
    {
        RectTransform rt = block.GetComponent<RectTransform>();
        float startSize = rt.rect.width;        
        while (rt.rect.width <= startSize * 1.1)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta[0] + (Time.deltaTime / 0.0005f), rt.sizeDelta[1] + (Time.deltaTime / 0.0005f));
            yield return null;
        }
        while(rt.rect.width >= startSize)
        {
            rt.sizeDelta = new Vector2(rt.sizeDelta[0] - (Time.deltaTime / 0.02f), rt.sizeDelta[1] - (Time.deltaTime / 0.02f));
            yield return null;
        }
    }
}
