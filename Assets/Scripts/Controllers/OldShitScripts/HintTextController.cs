using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HintTextController : MonoBehaviour, IPointerClickHandler
{
    public Text hintText;
    public Text hintContent;
    bool _contentShowing = false;

    private void Awake()
    {
        hintContent.color = new Color(0.196f, 0.196f, 0.196f, 0);
        hintText.color = new Color(0.196f, 0.196f, 0.196f, 1);
    }
    public void OnPointerClick(PointerEventData eventData)
    {        
        StartCoroutine(SwitchState());
        _contentShowing = !_contentShowing;
    }

    private IEnumerator SwitchState()
    {
        while(hintContent.color.a <= 1)
        {
            hintContent.color = new Color(0.196f, 0.196f, 0.196f, hintContent.color.a + (Time.deltaTime / 0.5f));
            hintText.color = new Color(0.196f, 0.196f, 0.196f, hintText.color.a - (Time.deltaTime / 0.5f));
            yield return null;
        }
        yield return new WaitForSeconds(3f);
        while (hintContent.color.a >= 0)
        {
            hintContent.color = new Color(0.196f, 0.196f, 0.196f, hintContent.color.a - (Time.deltaTime / 0.5f));
            hintText.color = new Color(0.196f, 0.196f, 0.196f, hintText.color.a + (Time.deltaTime / 0.5f));
            yield return null;
        }        
    }
}
