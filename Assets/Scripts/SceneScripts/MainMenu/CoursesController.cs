using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoursesController : BaseManager
{
    [Header("Parent Page Object")] [SerializeField] private GameObject coursesObject;
    [Header("Content View")] [SerializeField] private RectTransform mainContent;
    [Header("Prefabs")] [SerializeField] private GameObject lessonTabPrefab;
    [Header("Buttons")]
    [SerializeField] private GameObject cancelButton;
    [SerializeField] private GameObject melodyButton;
    [SerializeField] private GameObject harmonyButton;
    [SerializeField] private GameObject rhythmButton;
    [SerializeField] private GameObject timbreButton;
    [Header("Course Content pages")] 
    [SerializeField] private GameObject melodyContent;
    [SerializeField] private GameObject harmonyContent;
    [SerializeField] private GameObject rhythmContent;
    [SerializeField] private GameObject timbreContent;
    [Header("Arrows")] [SerializeField] private List<GameObject> arrows;
    private List<GameObject> _courseButtons;
    private List<GameObject> _melodyLessons = new List<GameObject>();
    private List<GameObject> _harmonyLessons = new List<GameObject>();
    private List<GameObject> _rhythmLessons = new List<GameObject>();
    private List<GameObject> _timbreLessons = new List<GameObject>();
    private List<int> _colourIndexes;
    private Dictionary<GameObject, bool> _tabMovingLookup;
    private Dictionary<GameObject, List<GameObject>> _lessonListLookup;
    private Dictionary<GameObject, GameObject> _contentLookup;
    private bool _melodyClicked, _harmonyClicked, _rhythmClicked, _timbreClicked;
    private const float TimeToTake = 0.1f;
    private const float TimeInc = 0.005f;
    private const float NumIncs = TimeToTake / TimeInc;

    protected override void OnAwake()
    {
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {cancelButton, CancelButtonCallback},
            {melodyButton, CourseButtonCallback},    // Melody
            {harmonyButton, CourseButtonCallback},    // Harmony
            {rhythmButton, CourseButtonCallback},    // Rhythm
            {timbreButton, CourseButtonCallback}     // Timbre
        };
        _courseButtons = new List<GameObject>
        {
            melodyButton, harmonyButton, rhythmButton, timbreButton
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
            {melodyButton, _melodyLessons},
            {harmonyButton, _harmonyLessons},
            {rhythmButton, _rhythmLessons},
            {timbreButton, _timbreLessons}
        };
        _contentLookup = new Dictionary<GameObject, GameObject>
        {
            {melodyButton, melodyContent},
            {harmonyButton, harmonyContent},
            {rhythmButton, rhythmContent},
            {timbreButton, timbreContent}
        };
        _colourIndexes = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7
        };
        _colourIndexes.Shuffle();
        var height = 5f + _courseButtons.Sum(button => button.GetComponent<RectTransform>().sizeDelta.y + 5);
        mainContent.sizeDelta = new Vector2(300, height);
    }

    private void CancelButtonCallback(GameObject g)
    {
        Destroy(coursesObject);
    }

    private void CourseButtonCallback(GameObject g)
    {
        switch (_courseButtons.IndexOf(g))
        {
            case 0:
                _melodyClicked = !_melodyClicked; 
                StartCoroutine(RotateArrow(arrows[0], _melodyClicked));
                StartCoroutine(ResizeTab(g, _melodyClicked));
                StartCoroutine(SpawnLessonTabs(g, _melodyClicked));
                break;
            case 1:
                _harmonyClicked = !_harmonyClicked;
                StartCoroutine(RotateArrow(arrows[1], _harmonyClicked));
                StartCoroutine(ResizeTab(g, _harmonyClicked));
                StartCoroutine(SpawnLessonTabs(g, _harmonyClicked));
                break;
            case 2:
                _rhythmClicked = !_rhythmClicked;
                StartCoroutine(RotateArrow(arrows[2], _rhythmClicked));
                StartCoroutine(ResizeTab(g, _rhythmClicked));
                StartCoroutine(SpawnLessonTabs(g, _rhythmClicked));
                break;
            case 3:
                _timbreClicked = !_timbreClicked;
                StartCoroutine(RotateArrow(arrows[3], _timbreClicked));
                StartCoroutine(ResizeTab(g, _timbreClicked));
                StartCoroutine(SpawnLessonTabs(g, _timbreClicked));
                break;
        }
    }

    private void LessonButtonCallback(GameObject g)
    {
        string scene = g.transform.GetChild(1).GetComponent<Text>().text;
        scene = string.Concat(scene.Where(c => !char.IsWhiteSpace(c)));
        SceneManager.LoadScene(scene);
        Debug.Log(scene);
    }
    
    private IEnumerator SpawnLessonTabs(GameObject g, bool open)
    {
        if (open)
        {
            List<string> lessons;
            switch (_courseButtons.IndexOf(g))
            {
                case 0: lessons = SharedData.melodyLessons; break;
                case 1: lessons = SharedData.harmonyLessons; break;
                case 2: lessons = SharedData.rhythmLessons; break;
                case 3: lessons = SharedData.timbreLessons; break;
                default: lessons = new List<string>(); break;
            }
            int counter = 0;
            foreach (string s in lessons)
            {
                Debug.Log(s);
                _lessonListLookup[g].Add(Instantiate(lessonTabPrefab, _contentLookup[g].transform));
                _lessonListLookup[g][counter].transform.GetChild(1).GetComponent<Text>().text = s;
                _lessonListLookup[g][counter].transform.GetChild(0).GetComponent<Image>().color =
                    SharedData.colours[_colourIndexes[counter > 7 ? counter - 8 : counter]];
                buttonCallbackLookup.Add(_lessonListLookup[g][counter], LessonButtonCallback);
                counter++;
            }
            var size = _contentLookup[g].transform.GetComponent<RectTransform>().sizeDelta;
            size.y = lessons.Count * 70;
            _contentLookup[g].transform.GetComponent<RectTransform>().sizeDelta = size;
        }
        else
        {
            _lessonListLookup[g].Clear();
            yield return new WaitUntil(() => !_tabMovingLookup[g]);
            int childCount = _contentLookup[g].transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(_contentLookup[g].transform.GetChild(i).gameObject);
            }
        }
    }

    private IEnumerator RotateArrow(GameObject g, bool down)
    {
        float timer = 0f;
        const float inc = 90 / NumIncs;
        while (timer <= TimeToTake)
        {
            var rotation = g.transform.eulerAngles;
            g.transform.eulerAngles = new Vector3(0, 0, down ? rotation.z - inc : rotation.z + inc);
            timer += TimeInc;
            yield return new WaitForSeconds(TimeInc);
        }
    }

    private IEnumerator ResizeTab(GameObject g, bool enlarge)
    {
        var rt = g.GetComponent<RectTransform>();
        var bgCol = g.GetComponent<Image>().color;
        var startHeight = rt.rect.height;
        float timer = 0;
        float target = enlarge ? startHeight + 250 : startHeight - 250;
        float heightIncr = (enlarge ? target - startHeight : startHeight - target) / NumIncs;
        float alphaInc = (enlarge ? 0.2f : -0.2f) / NumIncs;
        _tabMovingLookup[g] = true;
        while (timer <= TimeToTake)
        {
            var sizeDelta = rt.sizeDelta;
            sizeDelta = new Vector2(sizeDelta[0], enlarge ? sizeDelta[1] + heightIncr : sizeDelta[1] - heightIncr);
            rt.sizeDelta = sizeDelta;
            bgCol.a += alphaInc;
            g.GetComponent<Image>().color = bgCol;
            timer += TimeInc;
            yield return new WaitForSeconds(TimeInc);
        }
        _tabMovingLookup[g] = false;
        var height = 5f + _courseButtons.Sum(button => button.GetComponent<RectTransform>().sizeDelta.y + 5);
        mainContent.sizeDelta = new Vector2(300, height);
    }
}
