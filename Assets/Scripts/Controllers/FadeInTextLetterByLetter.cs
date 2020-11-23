using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// ? 2017 TheFlyingKeyboard and released under MIT License
// theflyingkeyboard.net
public class FadeInTextLetterByLetter : MonoBehaviour {
    [SerializeField] private Text textToUse;
    [SerializeField] private bool useThisText = false;
    [TextAreaAttribute(4, 15)]
    private string textToShow = "This is some text";
    [SerializeField] private bool useTextText = false;
    [SerializeField] private float fadeSpeedMultiplier = 0.25f;
    [SerializeField] private bool fade;
    private float _colorFloat = 0.1f;
    private int _colorInt;
    private int _letterCounter = 0;
    private string _shownText;
    private void Start()
    {
        if (useThisText)
        {
            textToUse = GetComponent<Text>();
        }
        if (useTextText)
        {
            textToShow = textToUse.text;
        }
        if (fade)
        {
            Fade();
        }
    }
    private IEnumerator FadeInText()
    {
        while (_letterCounter < textToShow.Length)
        {
            if (_colorFloat < 1.0f)
            {
                _colorFloat += Time.deltaTime * fadeSpeedMultiplier;
                _colorInt = (int)(Mathf.Lerp(0.0f, 1.0f, _colorFloat) * 255.0f);
                textToUse.text = _shownText + "<color=\"#FFFFFF" + string.Format("{0:X}", _colorInt) + "\">" + textToShow[_letterCounter] + "</color>";
            }
            else
            {
                _colorFloat = 0.1f;
                _shownText += textToShow[_letterCounter];
                _letterCounter++;
            }
            yield return null;
        }
    }
    public void Fade()
    {
        StartCoroutine(FadeInText());
    }
}