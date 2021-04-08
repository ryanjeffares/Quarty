using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoursesController : BaseManager
{
    #region GameObjects set in editor
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
    [Header("Arrows")] 
    [SerializeField] private List<GameObject> arrows;
    #endregion
    #region Private Members
    private List<GameObject> _courseButtons;
    private Dictionary<GameObject, bool> _tabMovingLookup;
    private Dictionary<GameObject, bool> _tabOpenLookup;
    private Dictionary<GameObject, List<GameObject>> _lessonListLookup;
    private Dictionary<GameObject, GameObject> _contentLookup;            
    #endregion

    protected override void OnAwake()
    {
        #region Callbacks
        buttonCallbackLookup = new Dictionary<GameObject, Action<GameObject>>
        {
            {cancelButton, CancelButtonCallback},
            {melodyButton, CourseButtonCallback},    // Melody
            {harmonyButton, CourseButtonCallback},    // Harmony
            {rhythmButton, CourseButtonCallback},    // Rhythm
            {timbreButton, CourseButtonCallback}     // Timbre
        };
        #endregion
        #region Lists
        _courseButtons = new List<GameObject>
        {
            melodyButton, harmonyButton, rhythmButton, timbreButton
        };
        #endregion
        #region Dictionaries
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
        #endregion
        #region UI Init
        var height = 5f + _courseButtons.Sum(button => button.GetComponent<RectTransform>().sizeDelta.y + 5);
        mainContent.sizeDelta = new Vector2(300, height);
        #endregion
    }

    private void CancelButtonCallback(GameObject g)
    {
        Destroy(coursesObject);
    }

    private void CourseButtonCallback(GameObject g)
    {
        // Set whether the tab is being opened/closed and start the coroutines appropriately
        _tabOpenLookup[g] = !_tabOpenLookup[g];
        StartCoroutine(RotateArrow(arrows[_courseButtons.IndexOf(g)], 0.2f, _tabOpenLookup[g]));
        StartCoroutine(ResizeTab(g, 0.2f, _tabOpenLookup[g]));
        StartCoroutine(SpawnLessonTabs(g, _tabOpenLookup[g]));
    }

    private void LessonButtonCallback(GameObject g)
    {
        // Loads a scene with corresponding name to the lesson button
        string scene = g.transform.GetChild(1).GetComponent<Text>().text;
        Persistent.sceneToLoad = string.Concat(scene.Where(c => !char.IsWhiteSpace(c)));
        Persistent.goingHome = false;
        SceneManager.LoadScene("LoadingScreen");
    }
    
    private IEnumerator SpawnLessonTabs(GameObject g, bool open)
    {
        if (open)
        {
            // Populate the scroll view with lesson buttons, names are parsed from XML on load
            Dictionary<string, bool> lessons;
            switch (_courseButtons.IndexOf(g))
            {
                case 0: lessons = Persistent.melodyLessons.lessons; break;
                case 1: lessons = Persistent.harmonyLessons.lessons; break;
                case 2: lessons = Persistent.rhythmLessons.lessons; break;
                //case 3: lessons = Persistent.timbreLessons.lessons; break;
                default: lessons = new Dictionary<string, bool>();
                    Debug.LogError("No lessons lists found...");
                    break;
            }
            int counter = 0;
            foreach (var kvp in lessons)
            {
                int colourIndex = counter % 8;
                _lessonListLookup[g].Add(Instantiate(lessonTabPrefab, _contentLookup[g].transform));
                _lessonListLookup[g][counter].transform.GetChild(1).GetComponent<Text>().text = kvp.Key;
                _lessonListLookup[g][counter].transform.GetChild(1).GetComponent<Text>().color = new Color(
                    0.196f, 0.196f, 0.196f, kvp.Value ? 1 : 0.3f);
                _lessonListLookup[g][counter].transform.GetChild(0).GetComponent<Image>().color = new Color(
                    Persistent.rainbowColours[colourIndex].r, 
                    Persistent.rainbowColours[colourIndex].g,
                    Persistent.rainbowColours[colourIndex].b, 
                    Persistent.rainbowColours[colourIndex].a * (kvp.Value ? 0.3f : 0.1f));
                if (kvp.Value)
                {
                    buttonCallbackLookup.Add(_lessonListLookup[g][counter], LessonButtonCallback);   
                }
                counter++;
            }
            // Resize the content view depending on how many lessons there are
            var size = _contentLookup[g].transform.GetComponent<RectTransform>().sizeDelta;
            size.y = lessons.Count * 70;
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
