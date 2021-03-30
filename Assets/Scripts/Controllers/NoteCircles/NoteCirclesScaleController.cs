using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteCirclesScaleController : MonoBehaviour
{
    [SerializeField] private List<GameObject> circles;
    [SerializeField] private List<Text> texts;
    [SerializeField] private bool useCustomNotes;
    [SerializeField] private List<string> customNotes;

    public void Show(bool useCustomNotes, List<string> customNotes = null)
    {
        if (useCustomNotes)
        {
            float waitTime = 0f;
            foreach (var (circle, index) in circles.WithIndex())
            {
                circle.GetComponent<NoteCircleController>().waitTime = waitTime;
                circle.GetComponent<NoteCircleController>().note = useCustomNotes ? customNotes[index] : customNotes[index];
                circle.GetComponent<NoteCircleController>().Show();
                waitTime += 0.1f;
            }
            waitTime = 0f;
            foreach (Text text in texts)
            {
                StartCoroutine(FadeText(text, 0.5f, waitTime));
                waitTime += 0.1f;
            }
        }
        else
        {
            string[] notes = { "C2", "D2", "E2", "F2", "G2", "A2", "B2", "C3" };
            float waitTime = 0f;
            foreach (var (circle, index) in circles.WithIndex())
            {
                circle.GetComponent<NoteCircleController>().waitTime = waitTime;
                circle.GetComponent<NoteCircleController>().note = useCustomNotes ? customNotes[index] : notes[index];
                circle.GetComponent<NoteCircleController>().Show();
                waitTime += 0.1f;
            }
            waitTime = 0f;
            foreach (Text text in texts)
            {
                StartCoroutine(FadeText(text, 0.5f, waitTime));
                waitTime += 0.1f;
            }
        }
    }

    private IEnumerator FadeText(Text text, float time, float waitTime)
    {
        if(waitTime > 0)
        {
            float counter = 0f;
            while (counter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                counter += Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        var startColour = text.color;
        var targetColour = new Color(0.196f, 0.196f, 0.196f, 1f);
        float resolution = time / 0.016f;
        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            text.color = Color.Lerp(startColour, targetColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }
}
