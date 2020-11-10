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
    private Dictionary<GameObject, bool> _tabMovingLookup;
    private Dictionary<GameObject, List<GameObject>> _lessonListLookup;
    private Dictionary<GameObject, GameObject> _contentLookup;
    private bool _melodyClicked, _harmonyClicked, _rhythmClicked, _timbreClicked;
    private const float TimeToTake = 0.2f;
    private const float TimeInc = 0.01f;
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
        //SceneManager.LoadScene(scene);
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
                buttonCallbackLookup.Add(_lessonListLookup[g][counter], LessonButtonCallback);
                counter++;
            }
        }
        else
        {
            _lessonListLookup[g].Clear();
            while (_tabMovingLookup[g])
            {
                yield return null;
            }
            int childCount = _contentLookup[g].transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(_contentLookup[g].transform.GetChild(i).gameObject);
            }
        }
    }

    private IEnumerator RotateArrow(GameObject g, bool down)
    {
        // FIX
        var rt = g.GetComponent<RectTransform>();
        var startRotation = rt.rotation;
        float timer = 0;
        int target = down ? 0 : 90;
        float rotationIncr = (down ? target - startRotation.z : startRotation.z - target) / NumIncs;
        Debug.Log(down ? target - startRotation.z : startRotation.z - target);
        while (timer <= TimeToTake)
        {
            var rotation = rt.rotation;
            rotation = new Quaternion(rotation[0], rotation[1], down ? rotation[2] + rotationIncr : rotation[2] - rotationIncr, rotation[3]);
            rt.rotation = rotation;
            timer += TimeInc;
            yield return new WaitForSeconds(TimeInc);
        }
    }

    private IEnumerator ResizeTab(GameObject g, bool enlarge)
    {
        _tabMovingLookup[g] = true;
        var rt = g.GetComponent<RectTransform>();
        var startHeight = rt.rect.height;
        float timer = 0;
        float target = enlarge ? startHeight * 4 : startHeight / 4;
        float heightIncr = (enlarge ? target - startHeight : startHeight - target) / NumIncs;
        while (timer <= TimeToTake)
        {
            var sizeDelta = rt.sizeDelta;
            sizeDelta = new Vector2(sizeDelta[0], enlarge ? sizeDelta[1] + heightIncr : sizeDelta[1] - heightIncr);
            rt.sizeDelta = sizeDelta;
            timer += TimeInc;
            yield return new WaitForSeconds(TimeInc);
        }
        _tabMovingLookup[g] = false;
    }
}

/*
    private void MelodyButtonCallback(GameObject g)
    {
        Debug.Log(g.gameObject.name);
        _melodyClicked = !_melodyClicked;
        foreach (GameObject tab in _courseButtons.Where(tab => tab != g))
        {
            var pos = tab.transform.localPosition;
            pos.y = _melodyClicked ? pos.y - 100 : pos.y + 100;
            tab.transform.localPosition = pos;
        }
        StartCoroutine(RotateArrow(arrows[0], _melodyClicked));
    }
    private void HarmonyButtonCallback(GameObject g)
    {
        _harmonyClicked = !_harmonyClicked;
        foreach (GameObject tab in _courseButtons.Where(tab => tab != harmonyButton))
        {
            var pos = tab.transform.localPosition;
            //if()
        }
        StartCoroutine(RotateArrow(arrows[1], _harmonyClicked));
    }
    private void RhythmButtonCallback(GameObject g)
    {
        StartCoroutine(RotateArrow(arrows[2], _rhythmClicked));
    }
    private void TimbreButtonCallback(GameObject g)
    {
        StartCoroutine(RotateArrow(arrows[3], _timbreClicked));
    }
*/
