using System.Collections;
using System.Collections.Generic;
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
    GameObject onScreenMessage;
    Text tryAgain;
    public static List<int> correctOrder;
    public static List<int> clickedOrder;
    public static int numClicks = 0;
    public static int levelStage = 0; // 0 - first message, 1 - pressing numbers, 2 - second message, 3 - press notes, 4 - final message    
    private int stage = 0;
    public static bool isNumberRound;
    private static bool failed = false;
    private bool hasFailed = false;    

    private void Awake()
    {        
        noteBlocks = new List<GameObject>();
        clickedOrder = new List<int>();
        correctOrder = new List<int>
        {
            0, 1, 2, 3, 4, 5, 6, 7
        };
        onScreenMessage = Instantiate(messagePrefab, content.transform);
        onScreenMessage.GetComponent<OnScreenMessageController>().displayText.text = "Press the numbers in the correct order!";        
    }

    private void Update()
    {
        if (stage != levelStage)
        {
            stage = levelStage;
            int childCount = content.transform.childCount;
            switch (stage)
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
        if(hasFailed != failed)
        {
            tryAgain = Instantiate(tryAgainText, content.transform);
            hasFailed = failed;
        }
    }
    /*
    private IEnumerator FadeAndDestroyObjects(GameObject g)
    {        
        if(g.TryGetComponent<Text>(out _))
        {
            while (g.GetComponent<Text>().color.a >= 0f)
            {
                g.GetComponent<Text>().color = new Color(g.GetComponent<Text>().color.r, g.GetComponent<Text>().color.g, g.GetComponent<Text>().color.b, g.GetComponent<Text>().color.a - (Time.deltaTime / 1));
                yield return null;
            }
        }
        else if(g.TryGetComponent<RawImage>(out _))
        {
            while (g.GetComponent<RawImage>().color.a >= 0f)
            {
                g.GetComponent<RawImage>().color = new Color(g.GetComponent<RawImage>().color.r, g.GetComponent<RawImage>().color.g, g.GetComponent<RawImage>().color.b, g.GetComponent<RawImage>().color.a - (Time.deltaTime / 1));
                yield return null;
            }
        }
        else
        {
            Debug.LogError("Couldn't find object of type Text or Raw Image");
        }
        Destroy(g);
    }
    */
    public static void CheckSucces()
    {
        int checkCounter = 0;
        for (int i = 0; i < 8; i++)
        {
            if (clickedOrder[i] == correctOrder[i]) checkCounter++;
        }
        if (checkCounter == 8)
        {
            levelStage += 1;
        }
        else
        {
            if (!failed)
            {
                failed = true;
            }
            else
            {
                failed = false;
            }
        }
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
            int noteIndex = SharedData.majorScale[usedIndexes[j]];
            if (noteIndex == 12) noteIndex = 0;
            nb.GetComponent<NoteBlockController>().note.Add(SharedData.allNotes[noteIndex], SharedData.midiNoteLookup[SharedData.allNotes[noteIndex]]); // aaaaa                  
            if (!isNumberRound)
            {
                nb.GetComponent<NoteBlockController>().displayText.text = SharedData.allNotes[noteIndex];
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
                    nb.GetComponent<NoteBlockController>().id = SharedData.AMajor.IndexOf(SharedData.allNotes[noteIndex]);
                }                
            }
            else
            {                                                    
                nb.GetComponent<NoteBlockController>().displayText.text = (usedIndexes[j] + 1).ToString();
                if (usedIndexes[j] + 1 == 8) nb.GetComponent<NoteBlockController>().octaveUp = true;
                nb.GetComponent<NoteBlockController>().id = usedIndexes[j];                                 
            }
            nb.GetComponent<RawImage>().color = SharedData.colours[nb.GetComponent<NoteBlockController>().id];
            ++j;            
        }     
    }

    private void SpawnMessage(string textToDisplay)
    {
        onScreenMessage = Instantiate(messagePrefab, new Vector3(240, 400, 0), new Quaternion(0, 0, 0, 0), content.transform);
        onScreenMessage.GetComponent<OnScreenMessageController>().displayText.text = textToDisplay;
    }
}
