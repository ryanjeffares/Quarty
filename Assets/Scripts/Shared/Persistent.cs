using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

// Data to be used commonly throughout the codebase
public static class Persistent
{
    public static string sceneToLoad;
    public static bool goingHome;
    public static List<string> allNotes;
    public static List<string> cMajor;
    public static List<int> majorScale;
    public static List<int> minorScale;
    public static List<double> sineWaveValues;
    public static Dictionary<string, List<int>> midiNoteLookup;
    public static Dictionary<int, Color> rainbowColours;
    public static Dictionary<string, Color> noteColours;
    public static Dictionary<string, Color> paletteColours;
    public static MelodyLessons melodyLessons;
    public static HarmonyLessons harmonyLessons;
    public static RhythmLessons rhythmLessons;
    public static TimbreLessons timbreLessons;
    public static Settings settings;
    public static readonly LoadingTexts LoadingTexts;
    public static readonly SplashTexts SplashTexts;
    
    static Persistent()
    {
        Application.targetFrameRate = 120;
        #region Theory Dictionaries
        allNotes = new List<string>
        {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };
        cMajor = new List<string>
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
            { 0, new Colour255(255, 117, 117).colour },
            { 1, new Colour255(255, 202, 117).colour },
            { 2, new Colour255(255, 244, 117).colour },
            { 3, new Colour255(156, 255, 177).colour },
            { 4, new Colour255(117, 255, 244).colour },
            { 5, new Colour255(107, 134, 255).colour },
            { 6, new Colour255(185, 107, 255).colour },
            { 7, new Colour255(255, 107, 223).colour }
        };
        noteColours = new Dictionary<string, Color>
        {
            {"C", new ColourHex("FF5D5D", 153).colour},
            {"D", new ColourHex("F3C568", 153).colour},
            {"E", new ColourHex("FFF79B", 153).colour},
            {"F", new ColourHex("B4FFA7", 153).colour},
            {"G", new ColourHex("99C2FF", 153).colour},
            {"A", new ColourHex("B683FF", 153).colour},
            {"B", new ColourHex("D174CC", 153).colour}
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

        if (!(Directory.Exists(Application.dataPath + "/Resources/Files/Lessons/")))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/Files/Lessons/");
        }
        if (!(Directory.Exists(Application.dataPath + "/Resources/Files/Settings/")))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources/Files/Settings/");
        }
        melodyLessons = new MelodyLessons();
        harmonyLessons = new HarmonyLessons();
        rhythmLessons = new RhythmLessons();
        timbreLessons = new TimbreLessons();
        LoadMelodyLessons();
        LoadHarmonyLessons();
        LoadRhythmLessons();
        LoadTimbreLessons();
        
        #endregion
        
        #region Settings
        
        settings = new Settings();
        LoadSettings();
        
        #endregion
        
        #region LoadingTexts
        
        LoadingTexts = new LoadingTexts();
        SplashTexts = new SplashTexts();
        
        #endregion
        
        #region Other
        sineWaveValues = new List<double>();
        for (int i = 0; i < 360; i++)
        {
            sineWaveValues.Add(Math.Sin(i * Math.PI / 180));
        }

        #endregion
    }

    private static void LoadMelodyLessons()
    {
        string path = Application.dataPath + "/Resources/Files/Lessons/MelodyLessons.xml";
        if (File.Exists(path))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNode rootNode = xmlDoc.FirstChild;
            foreach (XmlElement lesson in rootNode.ChildNodes)
            {
                melodyLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
            }
        }
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("Melody");
            xmlDoc.AppendChild(rootNode);
            int counter = 1;
            foreach (var kvp in melodyLessons.lessons)
            {
                XmlElement lessonNode = xmlDoc.CreateElement("Lesson");
                lessonNode.SetAttribute("number", counter.ToString());
                lessonNode.SetAttribute("name", kvp.Key);
                lessonNode.SetAttribute("available", kvp.Value.ToString());
                rootNode.AppendChild(lessonNode);
                counter++;
            }
            xmlDoc.Save(path);
        }
    }
    
    private static void LoadHarmonyLessons()
    {
        string path = Application.dataPath + "/Resources/Files/Lessons/HarmonyLessons.xml";
        if (File.Exists(path))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNode rootNode = xmlDoc.FirstChild;
            foreach (XmlElement lesson in rootNode.ChildNodes)
            {
                harmonyLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
            }
        }
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("Harmony");
            xmlDoc.AppendChild(rootNode);
            int counter = 1;
            foreach (var kvp in harmonyLessons.lessons)
            {
                XmlElement lessonNode = xmlDoc.CreateElement("Lesson");
                lessonNode.SetAttribute("number", counter.ToString());
                lessonNode.SetAttribute("name", kvp.Key);
                lessonNode.SetAttribute("available", kvp.Value.ToString());
                rootNode.AppendChild(lessonNode);
                counter++;
            }
            xmlDoc.Save(path);
        }
    }
    
    private static void LoadRhythmLessons()
    {
        string path = Application.dataPath + "/Resources/Files/Lessons/RhythmLessons.xml";
        if (File.Exists(path))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNode rootNode = xmlDoc.FirstChild;
            foreach (XmlElement lesson in rootNode.ChildNodes)
            {
                rhythmLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
            }
        }
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("Rhythm");
            xmlDoc.AppendChild(rootNode);
            int counter = 1;
            foreach (var kvp in rhythmLessons.lessons)
            {
                XmlElement lessonNode = xmlDoc.CreateElement("Lesson");
                lessonNode.SetAttribute("number", counter.ToString());
                lessonNode.SetAttribute("name", kvp.Key);
                lessonNode.SetAttribute("available", kvp.Value.ToString());
                rootNode.AppendChild(lessonNode);
                counter++;
            }
            xmlDoc.Save(path);
        }
    }
    
    private static void LoadTimbreLessons()
    {
        string path = Application.dataPath + "/Resources/Files/Lessons/TimbreLessons.xml";
        if (File.Exists(path))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNode rootNode = xmlDoc.FirstChild;
            foreach (XmlElement lesson in rootNode.ChildNodes)
            {
                timbreLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
            }
        }
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("Timbre");
            xmlDoc.AppendChild(rootNode);
            int counter = 1;
            foreach (var kvp in timbreLessons.lessons)
            {
                XmlElement lessonNode = xmlDoc.CreateElement("Lesson");
                lessonNode.SetAttribute("number", counter.ToString());
                lessonNode.SetAttribute("name", kvp.Key);
                lessonNode.SetAttribute("available", kvp.Value.ToString());
                rootNode.AppendChild(lessonNode);
                counter++;
            }
            xmlDoc.Save(path);
        }
    }

    public static void UpdateLessonAvailability(string course)
    {
        Dictionary<string, bool> lessonList;
        string path;
        switch (course)
        {
            case "Melody": 
                lessonList = melodyLessons.lessons;
                path = Application.dataPath + "/Resources/Files/Lessons/MelodyLessons.xml";
                break;
            case "Harmony":
                lessonList = harmonyLessons.lessons;
                path = Application.dataPath + "/Resources/Files/Lessons/HarmonyLessons.xml";
                break;
            case "Rhythm":
                lessonList = rhythmLessons.lessons;
                path = Application.dataPath + "/Resources/Files/Lessons/RhythmLessons.xml";
                break;
            case "Timbre":
                lessonList = timbreLessons.lessons;
                path = Application.dataPath + "/Resources/Files/Lessons/TimbreLessons.xml";
                break;
            default:
                Debug.LogError("Invalid string given to UpdateLessonAvailability. Returning.");
                return;
        }
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement(course);
        xmlDoc.AppendChild(rootNode);
        int counter = 1;
        foreach (var kvp in lessonList)
        {
            XmlElement lessonNode = xmlDoc.CreateElement("Lesson");
            lessonNode.SetAttribute("number", counter.ToString());
            lessonNode.SetAttribute("name", kvp.Key);
            lessonNode.SetAttribute("available", kvp.Value.ToString());
            rootNode.AppendChild(lessonNode);
            counter++;
        }
        xmlDoc.Save(path);
    }
    
    private static void LoadSettings()
    {
        string path = Application.dataPath + "/Resources/Files/Settings/UserSettings.xml";
        if (File.Exists(path))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNode rootNode = xmlDoc.FirstChild;
            foreach (XmlElement setting in rootNode)
            {
                settings.valueSettings[setting.Attributes[0].Value] = float.Parse(setting.Attributes[1].Value);
            }
        }
        else
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("Settings");
            xmlDoc.AppendChild(rootNode);
            foreach (var kvp in settings.valueSettings)
            {
                XmlElement setting = xmlDoc.CreateElement("Setting");
                setting.SetAttribute("name", kvp.Key);
                setting.SetAttribute("value", kvp.Value.ToString());
                rootNode.AppendChild(setting);
            }
            xmlDoc.Save(path);
        }
    }

    public static void UpdateSettings()
    {
        string path = Application.dataPath + "/Resources/Files/Settings/UserSettings.xml";
        XmlDocument xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("Settings");
        xmlDoc.AppendChild(rootNode);
        foreach (var kvp in settings.valueSettings)
        {
            XmlElement setting = xmlDoc.CreateElement("Setting");
            setting.SetAttribute("name", kvp.Key);
            setting.SetAttribute("value", kvp.Value.ToString());
            rootNode.AppendChild(setting);
        }
        xmlDoc.Save(path);
    }
}
