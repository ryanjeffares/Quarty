using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

// Data to be used commonly throughout the codebase
public static class SharedData
{
    public static string sceneToLoad;
    public static List<string> allNotes;
    public static List<string> aMajor;
    public static List<int> majorScale;
    public static List<int> minorScale;
    public static List<double> sineWaveValues;
    public static Dictionary<string, List<int>> midiNoteLookup;
    public static Dictionary<int, Color> rainbowColours;
    public static Dictionary<string, Color> paletteColours;
    public static readonly List<string> MelodyLessons  = new List<string>();
    public static readonly List<string> HarmonyLessons = new List<string>();
    public static readonly List<string> RhythmLessons  = new List<string>();
    public static readonly List<string> TimbreLessons  = new List<string>();
    public static string[] loadingTexts;
    
    static SharedData()
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
        rainbowColours = new Dictionary<int, Color>
        {
            { 0, Typedefs.Colour255(255, 117, 117) },
            { 1, Typedefs.Colour255(255, 202, 117) },
            { 2, Typedefs.Colour255(255, 244, 117) },
            { 3, Typedefs.Colour255(156, 255, 177) },
            { 4, Typedefs.Colour255(117, 255, 244) },
            { 5, Typedefs.Colour255(107, 134, 255) },
            { 6, Typedefs.Colour255(185, 107, 255) },
            { 7, Typedefs.Colour255(255, 107, 223) }
        };
        paletteColours = new Dictionary<string, Color>
        {
            {"Light Steel Blue", Typedefs.Colour255(167, 190, 211)},
            {"Colombia Blue", Typedefs.Colour255(198, 226, 233)},
            {"Cream", Typedefs.Colour255(241, 255, 196)},
            {"Apricot", Typedefs.Colour255(255, 202, 175)},
            {"Tan", Typedefs.Colour255(218, 184, 148)}
        };
        #endregion
        #region Lesson Lists
        LoadLessons(Application.dataPath + "/Resources/Files/Lessons/melodylessons.xml", MelodyLessons);
        LoadLessons(Application.dataPath + "/Resources/Files/Lessons/harmonylessons.xml", HarmonyLessons);
        LoadLessons(Application.dataPath + "/Resources/Files/Lessons/rhythmlessons.xml", RhythmLessons);
        LoadLessons(Application.dataPath + "/Resources/Files/Lessons/timbrelessons.xml", TimbreLessons);
        LoadSettings();
        LoadLoadingTexts(Application.dataPath + "/Resources/Files/loadingscreentexts.txt");
        #endregion
        #region Other
        sineWaveValues = new List<double>();
        for (int i = 0; i < 360; i++)
        {
            sineWaveValues.Add(Math.Sin(i * Math.PI / 180));
        }
        #endregion
    }

    private static void LoadLessons(string path, List<string> lessonList)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(path);
        XmlNode rootNode = xmlDoc.FirstChild;
        lessonList.AddRange(from XmlNode lesson in rootNode where lesson.Attributes != null select lesson.Attributes[1].Value);
    }
    
    private static void LoadSettings()
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Application.dataPath + "/Resources/Files/usersettings.xml");
        XmlNode rootNode = xmlDoc.FirstChild;
        foreach (XmlNode setting in rootNode.Cast<XmlNode>().Where(setting => setting.Attributes != null))
        {
            if (setting.Attributes != null)
                Settings.valueSettings.Add(setting.Attributes[0].Value, float.Parse(setting.Attributes[1].Value));
        }
    }

    private static void LoadLoadingTexts(string path)
    {
        loadingTexts = File.ReadAllLines(path);
    }
}
