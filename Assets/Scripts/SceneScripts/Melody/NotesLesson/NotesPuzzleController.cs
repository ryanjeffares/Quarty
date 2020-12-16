using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NotesPuzzleController : BaseManager
{
    [SerializeField] private GameObject nextButton, tryButton, emptyNoteCirclesPrefab, movableCirclePrefab, arrow;
    [SerializeField] private Text introText, timeText, niceText;
    [SerializeField] private Slider timeRemaining;
    [SerializeField] private AnimationCurve overshootCurve, overshootOutCurve, easeInOutCurve;
    [SerializeField] private List<AudioClip> clips;

    private GameObject _emptyNoteCircles;
    private Dictionary<Text, bool> _canTextLerp;
    private List<GameObject> _movableCircles;
    public List<string> _correctOrder, _playedNotes;
    private int _levelStage = 0;
    private int _circlesPlaced = 0;
    private int _notesPlayed = 0;
    private int _scalesDone = 0;
    private bool _arrowMoving = false;
    private bool _playing;
    
    protected override void OnAwake()
    {
        NoteCircleMovableController.NotePlayed += NotePlayedCallback;
        NoteCircleMovableController.CirclePlaced += CirclePlacedCallback;
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {nextButton, NextButtonCallback},
            {tryButton, TryButtonCallback},
        };
        _canTextLerp = new Dictionary<Text, bool>
        {
            {introText, true},
            {niceText, true},
            {nextButton.transform.GetChild(0).GetComponent<Text>(), true},
            {tryButton.transform.GetChild(0).GetComponent<Text>(), true},
        };
        _playedNotes = new List<string>();
        timeRemaining.fillRect.GetComponent<Image>().color = Persistent.rainbowColours[3];
        StartCoroutine(FadeText(introText, true, 0.5f, 200f));
        StartCoroutine(FadeButtonText(true, 0.5f, 200f));
        StartCoroutine(FadeSlider(0.2f, 100f));
        _emptyNoteCircles = Instantiate(emptyNoteCirclesPrefab, transform.GetChild(0));
        _emptyNoteCircles.transform.localPosition = new Vector3(0, -200);
        StartCoroutine(SpawnMovableCircles());
    }

    private void NextButtonCallback(GameObject g)
    {
        ++_levelStage;
        StartCoroutine(AdvanceLevelStage());
    }

    private void TryButtonCallback(GameObject g)
    {
        StartCoroutine(MoveArrow(new Vector2(215, -200), 1.2f, 200f));
    }
    
    private IEnumerator AdvanceLevelStage()
    {
        switch (_levelStage)
        {
            case 1:
                StartCoroutine(FadeText(introText, false, 0.5f, 200f));
                StartCoroutine(FadeText(tryButton.transform.GetChild(0).GetComponent<Text>(), true, 0.1f, 200f));
                StartCoroutine(FadeButtonText(false, 0.5f, 200f));
                StartCoroutine(DecreaseTimer());
                foreach (var circle in _movableCircles)
                {
                    circle.GetComponent<NoteCircleMovableController>().draggable = true;
                }
                break;
        }

        yield return null;
    }

    private IEnumerator SpawnMovableCircles(float waitTime = 0f)
    {
        if (waitTime > 0)
        {
            float waitInterval = waitTime / 100f;
            float waitCounter = 0f;
            while (waitCounter <= waitTime)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            } 
        }
        _movableCircles = new List<GameObject>();
        List<string> notes = new List<string>{"C", "D", "E", "F", "G", "A", "B", "C", "D", "E", "F", "G", "A", "B"};
        List<string> notesToShuffle = new List<string>{"C", "D", "E", "F", "G", "A", "B"};
        int[] localXs = {-125, -75, -25, 25, 75, 125};
        List<int> indexes = new List<int>{0, 1, 2, 3, 4, 5};
        indexes.Shuffle();
        notesToShuffle.Shuffle();
        float wait = 0f;
        var startNote = notesToShuffle[0]; // random start note...
        int idx = notes.IndexOf(notes.First(s => s ==startNote));
        _correctOrder = new List<string>();
        for (int i = 0; i < 8; i++)
        {
            _movableCircles.Add(Instantiate(movableCirclePrefab, _emptyNoteCircles.transform));
            _correctOrder.Add(notes[idx + i]);
            _movableCircles[i].GetComponent<NoteCircleMovableController>().note = notes[idx + i];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().waitTime = wait;
            _movableCircles[i].GetComponent<NoteCircleMovableController>().circleColour = Persistent.noteColours[notesToShuffle[i % 7]];
            _movableCircles[i].GetComponent<NoteCircleMovableController>().draggable = _playing;
            _movableCircles[i].GetComponent<AudioSource>().clip = clips[i + idx];
            switch (i)
            {
                case 0: _movableCircles[i].transform.localPosition = new Vector3(-175, 0);
                    break;
                case 7: _movableCircles[i].transform.localPosition = new Vector3(175, 0);
                    _movableCircles[i].GetComponent<NoteCircleMovableController>().octaveUp = true;
                    break;
                default: _movableCircles[i].transform.localPosition = new Vector3(localXs[indexes[i - 1]], -60);
                    break;
            }
            _movableCircles[i].GetComponent<NoteCircleMovableController>().Show();
            wait += 0.1f;
        }
    }

    private void TimerEnd()
    {
        if (_scalesDone > 0)
        {
            
        }
        else
        {
            introText.text = "Looks like you didn't fill in any scales correctly. Press next to try again.";
            StartCoroutine(FadeText(introText, false, 0.5f, 200f));
        }
    }
    
    private void NotePlayedCallback(string note)
    {
        _playedNotes.Add(note);
        _notesPlayed++;
        if (_notesPlayed == 8)
        {
            if (_playedNotes.SequenceEqual(_correctOrder))
            {
                _scalesDone++;
                niceText.text = "Nice!";
                niceText.color = Persistent.rainbowColours[3];
                StartCoroutine(ResizeText(0.3f, 200f, true));
                StartCoroutine(ResizeText(0.3f, 200f, false, 1f));
            }
            else
            {
                niceText.text = "Oops!";
                niceText.color = Persistent.rainbowColours[0];
                StartCoroutine(ResizeText(0.3f, 200f, true));
                StartCoroutine(ResizeText(0.3f, 200f, false, 1f));
            }
            foreach (var circle in _movableCircles)
            {
                StartCoroutine(circle.GetComponent<NoteCircleMovableController>().Destroy());
            }
            _movableCircles.Clear();
            StartCoroutine(SpawnMovableCircles(0.5f));
        }
    }

    private void CirclePlacedCallback()
    {
        _circlesPlaced++;
        if (_circlesPlaced == 6)
        {
            //StartCoroutine(MoveArrow(new Vector2(215, -200), 1.2f, 200f));
        }
    }
/*
    private void RearrangeCircles()
    {
        var random = new System.Random();
        List<string> notes = new List<string>{"C", "D", "E", "F", "G", "A", "B", "C", "D", "E", "F", "G", "A", "B"};
        List<string> notesToShuffle = new List<string>{"C", "D", "E", "F", "G", "A", "B"};
        int[] localXs = {-125, -75, -25, 25, 75, 125};
        List<int> indexes = new List<int>{0, 1, 2, 3, 4, 5};
        indexes.Shuffle();
        notesToShuffle.Shuffle();
        float wait = 0f;
        var startNote = notesToShuffle[0]; // random start note...
        Debug.Log(startNote);
        int idx = notes.IndexOf(startNote);
        _correctOrder.Clear();
        for (int i = 0; i < 8; i++)
        {
            _correctOrder.Add(notes[(idx +i ) % 7]);
        }
        
        foreach (var circle in _movableCircles)
        {
            Vector3 target;
            if (circle.GetComponent<NoteCircleMovableController>().note == notesToShuffle[0])
            {
                Debug.Log("Root");
                if (circle.GetComponent<NoteCircleMovableController>().octaveUp)
                {
                    target = new Vector3(175, 0);
                }
                else
                {
                    target = new Vector3(-175, 0);
                }
            }
            else
            {
                Debug.Log(circle.GetComponent<NoteCircleMovableController>().note);
                target = new Vector3(localXs[random.Next(localXs.Length)], -60);
            }
            StartCoroutine(MoveCircle(circle, circle.transform.localPosition, target, 0.3f, 100f));
        }
    }

    private IEnumerator MoveCircle(GameObject circle, Vector3 startPos, Vector3 target, float time, float resolution)
    {
        arrow.GetComponent<BoxCollider2D>().enabled = false;
        float targetX = target.x;
        float targetY = target.y;
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
            var pos = circle.transform.localPosition;
            pos.y = startPos.y + (easeInOutCurve.Evaluate(timeCounter / time) * yDiff);
            pos.x = startPos.x + (easeInOutCurve.Evaluate(timeCounter / time) * xDiff);
            circle.transform.localPosition = pos;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        arrow.GetComponent<BoxCollider2D>().enabled = true;
    }
    */
    private IEnumerator MoveArrow(Vector2 target, float time, float resolution, bool disableTrigger = false)
    {
        yield return new WaitUntil(() => !_arrowMoving);
        _arrowMoving = true;
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }
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
        StartCoroutine(MoveArrowLog(new Vector2(-215, arrow.transform.localPosition.y), 1f, 200f, true, true));
    }

    private IEnumerator MoveArrowLog(Vector2 target, float time, float resolution, bool disableTrigger, bool reset)
    {
        yield return new WaitUntil(() => !_arrowMoving);
        _arrowMoving = true;
        if (disableTrigger)
        {
            arrow.GetComponent<BoxCollider2D>().enabled = false;
        }
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
        if (reset)
        {
            _playedNotes.Clear();
            _notesPlayed = 0;
        }
        _arrowMoving = false;
    }
    
    private IEnumerator DecreaseTimer()
    {
        _playing = true;
        float timeCounter = 0f;
        float resolution = 1000f;
        float interval = 45 / resolution;
        timeRemaining.fillRect.GetComponent<Image>().color = Persistent.rainbowColours[3];
        while (timeCounter <= 45f)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            float remaining = 45 - timeCounter;
            timeRemaining.value = remaining;
            if (remaining < 5f)
            {
                if (remaining < 2f)
                {
                    var t = remaining.ToString().Substring(0, 4);
                    timeText.text = "Time remaining: " + t;
                }
                else
                {
                    var t = remaining.ToString().Substring(0, 3);
                    timeText.text = "Time remaining: " + t;
                }
            }
            else
            {
                timeText.text = "Time remaining: " + ((int) (45 - timeCounter));   
            }

            timeRemaining.fillRect.GetComponent<Image>().color = Color.Lerp(Persistent.rainbowColours[3], Persistent.rainbowColours[0], timeCounter / 45f);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
        _playing = false;
        foreach (var circle in _movableCircles)
        {
            circle.GetComponent<NoteCircleMovableController>().draggable = false;
        }
        timeText.text = "Time Remaining: 0";
        TimerEnd();
    }
    
    private IEnumerator FadeSlider(float time, float resolution)
    {
        float timeCounter = 0f;
        float interval = time / resolution;
        var startScale = timeRemaining.transform.localScale;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            if (_levelStage == 0)
            {
                arrow.GetComponent<Image>().color = Color.Lerp(Color.clear, Color.black, timeCounter / time);
            }
            var sc = timeRemaining.transform.localScale;
            sc.x = startScale.x + overshootCurve.Evaluate(timeCounter / time);
            sc.y = startScale.y + overshootCurve.Evaluate(timeCounter / time);
            timeRemaining.transform.localScale = sc;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }
    
    private IEnumerator FadeText(Text text, bool fadeIn, float time, float resolution, float wait = 0f, bool destroy = false, bool fadeOut = false, float duration = 0f)
    {
        yield return new WaitUntil(() => _canTextLerp[text]);
        _canTextLerp[text] = false;
        var startColour = text.color;
        var targetColour = new Color(0.196f, 0.196f, 0.196f, fadeIn ? 1f : 0f);

        if (wait > 0f)
        {
            float waitInterval = wait / resolution;
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            }   
        }

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

        if (fadeOut)
        {
            yield return new WaitForSeconds(duration);
            timeCounter = 0f;
            while (timeCounter <= time)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                text.color = Color.Lerp(targetColour, startColour, timeCounter / time);
                timeCounter += interval;
                yield return new WaitForSeconds(interval);
            }
        }
        _canTextLerp[text] = true;
        if (destroy)
        {
            _canTextLerp.Remove(text);
            Destroy(text);
        }
    }
    
    private IEnumerator FadeButtonText(bool fadeIn, float time, float resolution, float wait = 0f)
    {
        if (buttonCallbackLookup.ContainsKey(nextButton))
        {
            buttonCallbackLookup.Remove(nextButton);
        }
        yield return new WaitUntil(() => _canTextLerp[nextButton.transform.GetChild(0).GetComponent<Text>()]);
        _canTextLerp[nextButton.transform.GetChild(0).GetComponent<Text>()] = false;
        var startColour = nextButton.transform.GetChild(0).GetComponent<Text>().color;
        var targetColour = new Color(0.196f, 0.196f, 0.196f, fadeIn ? 1f : 0f);

        if (wait > 0f)
        {
            float waitInterval = wait / resolution;
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            }   
        }

        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            nextButton.transform.GetChild(0).GetComponent<Text>().color = Color.Lerp(startColour, targetColour, timeCounter / time);
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }

        _canTextLerp[nextButton.transform.GetChild(0).GetComponent<Text>()] = true;
        if(fadeIn)
        {
            buttonCallbackLookup.Add(nextButton, NextButtonCallback);
        }
    }

    private IEnumerator ResizeText(float time, float resolution, bool enlarge, float wait = 0f)
    {
        if (wait > 0f)
        {
            float waitInterval = wait / resolution;
            float waitCounter = 0f;
            while (waitCounter <= wait)
            {
                if (PauseManager.paused)
                {
                    yield return new WaitUntil(() => !PauseManager.paused);
                }
                waitCounter += waitInterval;
                yield return new WaitForSeconds(waitInterval);
            }   
        }

        var startScale = niceText.transform.localScale;
        float timeCounter = 0f;
        float interval = time / resolution;
        while (timeCounter <= time)
        {
            if (PauseManager.paused)
            {
                yield return new WaitUntil(() => !PauseManager.paused);
            }
            var sc = niceText.transform.localScale;
            if(enlarge)
            {
                sc.x = startScale.x + overshootCurve.Evaluate(timeCounter / time);
                sc.y = startScale.y + overshootCurve.Evaluate(timeCounter / time);
            }
            else
            {
                sc.x = startScale.x - overshootOutCurve.Evaluate(timeCounter / time);
                sc.y = startScale.y - overshootOutCurve.Evaluate(timeCounter / time);
            }
            niceText.transform.localScale = sc;
            timeCounter += interval;
            yield return new WaitForSeconds(interval);
        }
    }
}
