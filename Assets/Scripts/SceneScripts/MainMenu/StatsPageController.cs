using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatsPageController : BaseManager
{
    [SerializeField] private RectTransform mainContent;
    [SerializeField] private GameObject backButton, melodyButton, harmonyButton, rhythmButton, timbreButton;
    [SerializeField] private GameObject statsTab;
    [SerializeField] private List<GameObject> arrows;
    [SerializeField] private GameObject melodyContent;
    [SerializeField] private GameObject harmonyContent;
    [SerializeField] private GameObject rhythmContent;
    [SerializeField] private GameObject timbreContent;

    private Dictionary<GameObject, bool> _tabMovingLookup;
    private Dictionary<GameObject, bool> _tabOpenLookup;
    private Dictionary<GameObject, List<GameObject>> _lessonListLookup;
    private List<GameObject> _courseButtons;
    private Dictionary<GameObject, GameObject> _contentLookup;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, System.Action<GameObject>>
        {
            {backButton, BackButtonCallback },
            {melodyButton, CourseButtonCallback},    // Melody
            {harmonyButton, CourseButtonCallback},    // Harmony
            {rhythmButton, CourseButtonCallback},    // Rhythm
            {timbreButton, CourseButtonCallback}     // Timbre
        };
        _tabOpenLookup = new Dictionary<GameObject, bool>
        {
            {melodyButton, false},
            {harmonyButton, false},
            {rhythmButton, false},
            {timbreButton, false}
        };
        _tabMovingLookup = new Dictionary<GameObject, bool>
        {
            {melodyButton, false},
            {harmonyButton, false},
            {rhythmButton, false},
            {timbreButton, false}
        };
        _lessonListLookup = new Dictionary<GameObject, List<GameObject>>
        {
            {melodyButton, new List<GameObject>()},
            {harmonyButton, new List<GameObject>()},
            {rhythmButton, new List<GameObject>()},
            {timbreButton, new List<GameObject>()}
        };
        _contentLookup = new Dictionary<GameObject, GameObject>
        {
            {melodyButton, melodyContent},
            {harmonyButton, harmonyContent},
            {rhythmButton, rhythmContent},
            {timbreButton, timbreContent}
        };
        _courseButtons = new List<GameObject>
        {
            melodyButton, harmonyButton, rhythmButton, timbreButton
        };
        var height = 5f + _courseButtons.Sum(button => button.GetComponent<RectTransform>().sizeDelta.y + 5);
        mainContent.sizeDelta = new Vector2(300, height);
    }

    private void BackButtonCallback(GameObject g)
    {
        Destroy(gameObject);
    }

    private void CourseButtonCallback(GameObject g)
    {
        // Set whether the tab is being opened/closed and start the coroutines appropriately
        _tabOpenLookup[g] = !_tabOpenLookup[g];
        StartCoroutine(RotateArrow(arrows[_courseButtons.IndexOf(g)], 0.2f, _tabOpenLookup[g]));
        StartCoroutine(ResizeTab(g, 0.2f, _tabOpenLookup[g]));
        StartCoroutine(SpawnLessonTabs(g, _tabOpenLookup[g]));
    }

    private IEnumerator SpawnLessonTabs(GameObject g, bool open)
    {
        if (open)
        {
            // Populate the scroll view with lesson buttons, names are parsed from XML on load
            Dictionary<string, int> scores;
            switch (_courseButtons.IndexOf(g))
            {
                case 0: scores = Persistent.melodyLessons.scores; break;
                case 1: scores = Persistent.harmonyLessons.scores; break;
                case 2: scores = Persistent.rhythmLessons.scores; break;
                case 3: scores = Persistent.timbreLessons.scores; break;
                default:
                    scores = new Dictionary<string, int>();
                    Debug.LogError("No lessons lists found...");
                    break;
            }
            int counter = 0;
            foreach (var kvp in scores)
            {
                int colourIndex = counter % 8;
                _lessonListLookup[g].Add(Instantiate(statsTab, _contentLookup[g].transform));
                _lessonListLookup[g][counter].transform.GetChild(1).GetComponent<Text>().text = kvp.Key;
                _lessonListLookup[g][counter].transform.GetChild(2).GetComponent<Text>().text = kvp.Value.ToString();
                _lessonListLookup[g][counter].transform.GetChild(1).GetComponent<Text>().color = new Color(0.196f, 0.196f, 0.196f, 1);
                var imgColour = Persistent.rainbowColours[colourIndex];
                imgColour.a *= 0.3f;
                _lessonListLookup[g][counter].transform.GetChild(0).GetComponent<Image>().color = imgColour;
                counter++;
            }
            // Resize the content view depending on how many lessons there are
            var size = _contentLookup[g].transform.GetComponent<RectTransform>().sizeDelta;
            size.y = scores.Count * 70;
            _contentLookup[g].transform.GetComponent<RectTransform>().sizeDelta = size;
        }
        else
        {
            // Clear the list, wait until the tab is no longer resizing and destroy the buttons
            _lessonListLookup[g].Clear();
            yield return new WaitUntil(() => !_tabMovingLookup[g]);
            int childCount = _contentLookup[g].transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(_contentLookup[g].transform.GetChild(i).gameObject);
            }
        }
    }

    private IEnumerator RotateArrow(GameObject g, float time, bool down)
    {
        float timer = 0f;
        while (timer <= time)
        {
            var rotation = g.transform.eulerAngles;
            float rot = down ? Mathf.Lerp(90, 0, timer / time) : Mathf.Lerp(0, 90, timer / time);
            g.transform.eulerAngles = new Vector3(0, 0, rot);
            timer += Time.deltaTime;
            yield return null;
        }
        g.transform.eulerAngles = new Vector3(0, 0, down ? 0 : 90);
    }

    private IEnumerator ResizeTab(GameObject g, float time, bool enlarge)
    {
        var rt = g.GetComponent<RectTransform>();
        var bgCol = g.GetComponent<Image>().color;
        float timer = 0;
        _tabMovingLookup[g] = true;
        float startHeight = rt.sizeDelta.y;
        while (timer <= time)
        {
            var sizeDelta = rt.sizeDelta;
            float newHeight = enlarge ? Mathf.Lerp(startHeight, startHeight + 300, timer / time) : Mathf.Lerp(startHeight, startHeight - 300, timer / time);
            float newAlpha = enlarge ? Mathf.Lerp(0, 0.2f, timer / time) : Mathf.Lerp(0.2f, 0, timer / time);
            sizeDelta = new Vector2(sizeDelta[0], newHeight);
            rt.sizeDelta = sizeDelta;
            g.GetComponent<Image>().color = new Color(bgCol.r, bgCol.g, bgCol.b, newAlpha);
            timer += Time.deltaTime;
            yield return null;
        }
        _tabMovingLookup[g] = false;
        // Resize the main scroll view to be the same size as all the course buttons in their open/closed state
        var height = 5f + _courseButtons.Sum(button => button.GetComponent<RectTransform>().sizeDelta.y + 5);
        mainContent.sizeDelta = new Vector2(300, height);
    }
}
