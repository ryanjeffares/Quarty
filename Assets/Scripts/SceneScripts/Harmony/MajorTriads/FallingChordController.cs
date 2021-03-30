using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FallingChordController : MonoBehaviour
{
    public static event Action Destroyed;
    [SerializeField] private GameObject root, third, fifth;
    [SerializeField] private Text rootText, thirdText, fifthText;

    private string rootNote, thirdNote, fifthNote;

    //private void Awake()
    //{
    //    Show(new string[] { "C", "E", "G" });
    //}

    public void Show(string[] chord)
    {
        rootNote = chord[0];
        thirdNote = chord[1];
        fifthNote = chord[2];
        rootText.text = rootNote;
        root.GetComponent<Image>().color = Persistent.noteColours[rootNote];
        thirdText.text = thirdNote;
        third.GetComponent<Image>().color = Persistent.noteColours[thirdNote];
        fifthText.text = fifthNote;
        fifth.GetComponent<Image>().color = Persistent.noteColours[fifthNote];
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        while(transform.localPosition.y >= 0)
        {
            var pos = transform.localPosition;
            transform.localPosition = new Vector3(pos.x, pos.y - 3);
            yield return new WaitForFixedUpdate();
        }
        Destroyed?.Invoke();
        Destroy(gameObject);
    }
}
