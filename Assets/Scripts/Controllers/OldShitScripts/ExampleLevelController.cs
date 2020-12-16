using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

public class ExampleLevelController : MonoBehaviour
{
    public List<GameObject> noteBlocks;
    public GameObject noteBlockPrefab;
    public GameObject messagePrefab;
    public GameObject content;    
    public GameObject hintPrefab;
    public Text tryAgainText;
    public static List<int> clickedOrder;
    public static int numClicks = 0;
    public static int levelStage = 0; // 0 - first message, 1 - pressing numbers, 2 - second message, 3 - press notes, 4 - final message    
    public static bool isNumberRound;
    private int _stage = 0;
    private GameObject _onScreenMessage;
    private Text _tryAgain;
    private bool _hasFailed = false;
    private static List<int> _correctOrder;
    private static bool _failed = false;

    private void Awake()
    {        
        noteBlocks = new List<GameObject>();
        clickedOrder = new List<int>();
        _correctOrder = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7
        };
        _onScreenMessage = Instantiate(messagePrefab, content.transform);
        _onScreenMessage.GetComponent<OnScreenMessageController>().displayText.text = "Press the numbers in the correct order!";        
    }

    private void Update()
    {
        if (_stage != levelStage)
        {
            _stage = levelStage;
            int childCount = content.transform.childCount;
            switch (_stage)
            {
                case 1:
                    for (int i = childCount - 1; i >= 0; i--) Destroy(content.transform.GetChild(i).gameObject);
                    isNumberRound = true;
                    SpawnBlocks();
                    break;
                case 2:
                    for (int i = childCount - 1; i >= 0; i--) Destroy(content.transform.GetChild(i).gameObject);
                    SpawnMessage("Great! Now let's try it with note names. Press the notes of the C Major scale in the right order.");
                    break;
                case 3:
                    for (int i = childCount - 1; i >= 0; i--) Destroy(content.transform.GetChild(i).gameObject);
                    isNumberRound = false;
                    noteBlocks.Clear();
                    Instantiate(hintPrefab, content.transform);
                    SpawnBlocks();
                    break;
                case 4:
                    for (int i = childCount - 1; i >= 0; i--) Destroy(content.transform.GetChild(i).gameObject);                    
                    SpawnMessage("Well done! Let's continue to the next lesson.");                    
                    break;
            }
        }
        if(_hasFailed != _failed)
        {
            _tryAgain = Instantiate(tryAgainText, content.transform);
            _hasFailed = _failed;
        }
    }

    public static void CheckSucces()
    {
        int checkCounter = 0;
        for (int i = 0; i < 8; i++)
            if (clickedOrder[i] == _correctOrder[i])
                checkCounter++;
        if (checkCounter == 8)
            levelStage++;
        else
            _failed = !_failed;
        clickedOrder.Clear();
        numClicks = 0;
    }

    private void SpawnBlocks()
    {
        float delay = 0f;
        for (int i = 0; i < 8; i++)
        {
            int xPos = i % 2 == 0 ? 120 : 360;
            int yPos = 160 * ((int)i / 2 + 1);
            noteBlocks.Add(Instantiate(noteBlockPrefab, new Vector3(xPos, yPos), new Quaternion(0, 0, 0, 0), content.transform));
            noteBlocks[i].GetComponent<NoteBlockController>().delay = delay;
            delay += 0.2f;
        }
        List<int> usedIndexes = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7
        };
        usedIndexes.Shuffle();        
        bool rootFound = false;        
        int j = 0;
        foreach (GameObject nb in noteBlocks)
        {
            int noteIndex = Persistent.majorScale[usedIndexes[j]];
            if (noteIndex == 12) noteIndex = 0;
            nb.GetComponent<NoteBlockController>().note.Add(Persistent.allNotes[noteIndex], Persistent.midiNoteLookup[Persistent.allNotes[noteIndex]]); // aaaaa                  
            if (!isNumberRound)
            {
                nb.GetComponent<NoteBlockController>().displayText.text = Persistent.allNotes[noteIndex];
                if (noteIndex == 0 && !rootFound)
                {
                    nb.GetComponent<NoteBlockController>().id = 0;
                    rootFound = true;
                }
                else if (noteIndex == 0 && rootFound)
                {
                    nb.GetComponent<NoteBlockController>().id = 7;
                    nb.GetComponent<NoteBlockController>().octaveUp = true;
                    nb.GetComponent<NoteBlockController>().displayText.text += "(+8)";
                }
                else
                {
                    nb.GetComponent<NoteBlockController>().id = Persistent.cMajor.IndexOf(Persistent.allNotes[noteIndex]);
                }                
            }
            else
            {                                                    
                nb.GetComponent<NoteBlockController>().displayText.text = (usedIndexes[j] + 1).ToString();
                if (usedIndexes[j] + 1 == 8) nb.GetComponent<NoteBlockController>().octaveUp = true;
                nb.GetComponent<NoteBlockController>().id = usedIndexes[j];                                 
            }
            nb.GetComponent<RawImage>().color = Persistent.rainbowColours[nb.GetComponent<NoteBlockController>().id];
            ++j;            
        }     
    }

    private void SpawnMessage(string textToDisplay)
    {
        _onScreenMessage = Instantiate(messagePrefab, new Vector3(240, 400, 0), new Quaternion(0, 0, 0, 0), content.transform);
        _onScreenMessage.GetComponent<OnScreenMessageController>().displayText.text = textToDisplay;
    }
}
