using System;
using UnityEngine;

public class KeyOutlineCollider : MonoBehaviour
{
    public static event Action<bool> KeyInsideBounds;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "DraggableKey")
        {
            Debug.Log("Hello");
            KeyInsideBounds?.Invoke(true);
        }        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "DraggableKey")
        {
            Debug.Log("Bye");
            KeyInsideBounds?.Invoke(false);
        }
    }
}
