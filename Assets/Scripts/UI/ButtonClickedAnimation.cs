using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickedAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Image _image;
    private Color _startColour;
    private Color _textColour;
    private Vector3 _position;
    private Text _text;
    private bool _hasText;
    private void Awake()
    {
        _image = GetComponent<Image>();
        _startColour = _image.color;
        _position = transform.localPosition;
        if (transform.GetChild(0).TryGetComponent<Text>(out _))
        {
            _hasText = true;
            _text = transform.GetChild(0).GetComponent<Text>();
            _textColour = _text.color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _image.color = new Color(_startColour.r, _startColour.g, _startColour.b, _startColour.a * 0.7f);
        if(_hasText)
            _text.color = new Color(_textColour.r, _textColour.g, _textColour.b, _textColour.a * 0.7f);
        _position.x += 2;
        _position.y -= 2;
        transform.localPosition = _position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _image.color = _startColour;
        if(_hasText)
            _text.color = _textColour;
        _position.x -= 2;
        _position.y += 2;
        transform.localPosition = _position;
    }
}