using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MelodyWritingLessonController : BaseManager
{
    [SerializeField] private Text introText;
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject nextButton, tryButton;
    [SerializeField] private GameObject arrow;
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private AnimationCurve easeInOutCurve;

    private bool _arrowMoving = false;
    private List<string> _allNotes;        

    protected override void OnAwake()
    {
        canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true },
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true },
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true }
        };
        fullCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback },
            {tryButton, TryButtonCallback }
        };
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>();
        _allNotes = new List<string>
        {
            "C1", "C#1", "D1", "D#1", "E1", "F1", "F#1", "G1", "G#1", "A1", "A#1", "B1",
            "C2", "C#2", "D2", "D#2", "E2", "F2", "F#2", "G2", "G#2", "A2", "A#2", "B2",
            "C3"
        };
        int[] availableX = new int[] { -210, -150, -90, -30, 30, 90, 150, 210 };
        int x = -175;
        int y = -200;
        float waitTime = 0f;
        foreach(var (n, i) in _allNotes.WithIndex())
        {
            if (i % 8 == 0 && i > 0) y -= 100;
            var pos = new Vector2(x, y);
            var circle = Instantiate(circlePrefab, mainContainer.transform);
            circle.transform.localPosition = new Vector3(x, y);            
            var controller = circle.GetComponent<NoteCircleMovableController>();
            controller.circleColour = Persistent.noteColours[n.Substring(0, n.Length - 1)];
            controller.draggable = true;
            controller.clickToPlay = true;
            controller.note = n;
            controller.waitTime = waitTime;
            controller.localY = -100;
            controller.availableX = availableX.ToList();
            controller.Show();
            x += 50;
            if (x > 175) x = -175;            
            waitTime += 0.1f;
        }
        StartCoroutine(FadeText(introText, true, 0.5f));
        StartCoroutine(FadeButtonText(nextButton, true, 0.5f, 4f));
        StartCoroutine(FadeButtonText(tryButton, true, 0.5f, 2f));
        StartCoroutine(FadeInArrow(0.5f));
    }

    private void NextButtonCallback(GameObject g)
    {
        Persistent.sceneToLoad = "MainMenu";
        Persistent.goingHome = true;
        SceneManager.LoadScene("LoadingScreen");
    }

    private void TryButtonCallback(GameObject g)
    {
        if (_arrowMoving) return;
        StartCoroutine(MoveArrow(new Vector2(270, -100), 2f));
    }

    private IEnumerator MoveArrow(Vector2 target, float time, bool disableTrigger = false)
    {
        yield return new WaitUntil(() => !_arrowMoving);
        _arrowMoving = true;
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }
        float resolution = time / 0.016f;
        float timeCounter = 0f;
        float interval = time / resolution;
        var startPos = arrow.transform.localPosition;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            arrow.transform.localPosition = Vector2.Lerp(startPos, target, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        _arrowMoving = false;
        StartCoroutine(MoveArrowLog(new Vector2(-270, -100), 1f, true, 0.2f));
    }

    private IEnumerator MoveArrowLog(Vector2 target, float time, bool disableTrigger, float waitTime = 0f)
    {
        if (waitTime >= 0)
        {
            float waitCounter = 0f;
            while (waitCounter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += Time.deltaTime;
                yield return null;
            }
        }
        yield return new WaitUntil(() => !_arrowMoving);
        _arrowMoving = true;
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }
        float resolution = time / 0.016f;
        float targetX = target.x;
        float targetY = target.y;
        var startPos = arrow.transform.localPosition;
        float yDiff = targetY - startPos.y;
        float xDiff = targetX - startPos.x;
        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            var pos = arrow.transform.localPosition;
            pos.y = startPos.y + (easeInOutCurve.Evaluate(timeCounter / time) * yDiff);
            pos.x = startPos.x + (easeInOutCurve.Evaluate(timeCounter / time) * xDiff);
            arrow.transform.localPosition = pos;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = true;
        }
        _arrowMoving = false;
    }

    private IEnumerator FadeInArrow(float time)
    {
        float alpha;
        float timeCounter = 0f;
        while (timeCounter <= time)
        {
            alpha = Mathf.Lerp(0f, 1f, timeCounter / time);
            arrow.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
            timeCounter += Time.deltaTime;
            yield return null;
        }
    }
}
