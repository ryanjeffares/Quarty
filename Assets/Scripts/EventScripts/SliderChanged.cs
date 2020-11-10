using System;
using UnityEngine;
using UnityEngine.UI;

public class SliderChanged : MonoBehaviour
{
    public static event Action<GameObject, float> OnSliderChanged; 
    private Slider _currentSlider;

    private void Awake()
    {
        _currentSlider = GetComponent<Slider>();
        _currentSlider.onValueChanged.AddListener(delegate { SliderMoved();});
    }

    private void SliderMoved()
    {
        OnSliderChanged?.Invoke(gameObject, _currentSlider.value);
    }
}