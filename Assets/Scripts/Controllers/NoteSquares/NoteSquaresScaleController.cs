using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class NoteSquaresScaleController : MonoBehaviour
{
    [SerializeField] List<GameObject> noteSquares;
    private string _rootNote;
    
    public void Show(bool useCustomNotes = false, List<string> customNotes = null, bool squaresDraggableRight = true, bool squaresDraggableLeft = true)
    {
        string[] notes = new string[] { "C2", "D2", "D#2", "E2", "F2", "G2", "G#2", "A2", "B2", "C3" };
        _rootNote = useCustomNotes ? customNotes[0].Substring(0, customNotes[0].Length - 1) : notes[0].Substring(0, notes[0].Length - 1);
        float waitTime = 0f;
        foreach(var (n, index) in noteSquares.WithIndex())
        {
            var controller = n.GetComponent<NoteSquareMovableController>();
            controller.note = useCustomNotes ? customNotes[index] : notes[index];
            controller.squareColour = Persistent.noteColours[useCustomNotes ?
                customNotes[index].Substring(0, customNotes[index].Length - 1) : notes[index].Substring(0, notes[index].Length - 1)];
            controller.waitTime = waitTime;
            controller.canMoveRight = squaresDraggableRight;
            controller.canMoveLeft = squaresDraggableLeft;
            controller.Show();
            waitTime += 0.1f;
        }
    }

    public void DestroySquares()
    {
        foreach(var n in noteSquares)
        {
            StartCoroutine(n.GetComponent<NoteSquareMovableController>().Destroy());
        }
    }

    public void MakeDraggable(bool state)
    {
        foreach (var n in noteSquares.Where(ns => noteSquares.IndexOf(ns) != 0 && noteSquares.IndexOf(ns) != 9))
        {
            n.GetComponent<NoteSquareMovableController>().draggable = state;
        }
    }
}
