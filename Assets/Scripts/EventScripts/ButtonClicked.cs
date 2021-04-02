using System;
using UnityEngine.EventSystems;
using UnityEngine;

public class ButtonClicked : MonoBehaviour, IPointerClickHandler
{
    public static event Action<GameObject> OnButtonClicked;

    public void OnPointerClick(PointerEventData eventData)
    {        
        OnButtonClicked?.Invoke(gameObject);
    }
}