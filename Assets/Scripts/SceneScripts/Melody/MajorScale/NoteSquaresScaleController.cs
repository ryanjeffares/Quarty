using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSquaresScaleController : MonoBehaviour
{
    [SerializeField] List<GameObject> noteSquares;
    
    public void Show(bool useCustomNotes = false, List<string> customNotes = null)
    {
        string[] notes = new string[] { "C2", "D2", "D#2", "E2", "F2", "G2", "G#2", "A2", "B2", "C3" };
        float waitTime = 0f;
        foreach(var (n, index) in noteSquares.WithIndex())
        {
            var controller = n.GetComponent<NoteSquareMovableController>();
            controller.note = useCustomNotes ? customNotes[index] : notes[index];
            controller.squareColour = Persistent.noteColours[notes[index].Substring(0, notes[index].Length - 1)];
            controller.waitTime = waitTime;
            controller.Show();
            waitTime += 0.1f;
        }
    }

    public void MakeDraggable(bool state)
    {
        foreach (var n in noteSquares)
        {
            n.GetComponent<NoteSquareMovableController>().draggable = state;
        }
    }
}
