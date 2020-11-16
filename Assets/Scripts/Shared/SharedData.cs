using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

// Data to be used commonly throughout the codebase
public class SharedData : MonoBehaviour
{    
    public static List<string> allNotes;
    public static List<string> aMajor;
    public static List<int> majorScale;
    public static List<int> minorScale;
    public static List<double> sineWaveValues;
    public static Dictionary<string, List<int>> midiNoteLookup;
    public static Dictionary<int, Color> colours;
    public static List<string> melodyLessons  = new List<string>();
    public static List<string> harmonyLessons = new List<string>();
    public static List<string> rhythmLessons  = new List<string>();
    public static List<string> timbreLessons  = new List<string>();

    private void Awake()
    {
        #region Theory Dictionaries
        allNotes = new List<string>
        {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };
        aMajor = new List<string>
        {
            "C", "D", "E", "F", "G", "A", "B", "C"
        };
        majorScale = new List<int>
        {
            0, 2, 4, 5, 7, 9, 11, 12
        };        
        minorScale = new List<int>
        {
            0, 2, 3, 5, 7, 8, 10, 12
        };
        midiNoteLookup = new Dictionary<string, List<int>>();
        int lowestNote = 24;
        foreach (string s in allNotes)
        {
            int startNote = lowestNote;
            midiNoteLookup.Add(s, new List<int>());
            while (startNote <= 127)
            {
                midiNoteLookup[s].Add(startNote);
                startNote += 12;
            }
            lowestNote++;
        }
        #endregion
        #region UI
        colours = new Dictionary<int, Color>
        {
            { 0, new Color(255 / 255f, 117 / 255f, 117 / 255f, 0.2f) },
            { 1, new Color(255 / 255f, 202 / 255f, 117 / 255f, 0.2f) },
            { 2, new Color(255 / 255f, 244 / 255f, 117 / 255f, 0.2f) },
            { 3, new Color(156 / 255f, 255 / 255f, 177 / 255f, 0.2f) },
            { 4, new Color(117 / 255f, 255 / 255f, 244 / 255f, 0.2f) },
            { 5, new Color(107 / 255f, 134 / 255f, 255 / 255f, 0.2f) },
            { 6, new Color(185 / 255f, 107 / 255f, 255 / 255f, 0.2f) },
            { 7, new Color(255 / 255f, 107 / 255f, 223 / 255f, 0.2f) }
        };
        #endregion
        #region Lesson Lists
        LoadLessons(Application.dataPath + "/Resources/Files/Lessons/melodylessons.xml", ref melodyLessons);
        LoadLessons(Application.dataPath + "/Resources/Files/Lessons/harmonylessons.xml", ref harmonyLessons);
        LoadLessons(Application.dataPath + "/Resources/Files/Lessons/rhythmlessons.xml", ref rhythmLessons);
        LoadLessons(Application.dataPath + "/Resources/Files/Lessons/timbrelessons.xml", ref timbreLessons);
        #endregion
        #region Other
        sineWaveValues = new List<double>();
        for (int i = 0; i < 360; i++)
        {
            sineWaveValues.Add(Math.Sin(i * Math.PI / 180));
        }
        #endregion
    }

    private static void LoadLessons(string path, ref List<string> lessonList)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        XmlNode rootNode = xmlDoc.FirstChild;
        lessonList.AddRange(from XmlNode lesson in rootNode where lesson.Attributes != null select lesson.Attributes[1].Value);
    }
}
