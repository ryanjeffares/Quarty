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
    public static string sceneToLoad, userName;
    public static bool goingHome;
    public static List<string> allNotes;
    public static List<string> cMajor;
    public static List<int> majorScale;
    public static List<int> minorScale;
    public static List<double> sineWaveValues;
    public static Dictionary<string, List<int>> midiNoteLookup;
    public static List<Color> rainbowColours;
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
        rainbowColours = new List<Color>
        {
            new Colour255(255, 117, 117).colour,
            new Colour255(255, 202, 117).colour,
            new Colour255(255, 244, 117).colour,
            new Colour255(156, 255, 177).colour,
            new Colour255(117, 255, 244).colour,
            new Colour255(107, 134, 255).colour,
            new Colour255(185, 107, 255).colour,
            new Colour255(255, 107, 223).colour
        };
        noteColours = new Dictionary<string, Color>
        {
            {"C", new ColourHex("FF5D5D", 153).colour},
            {"C#", new ColourHex("FF5D5D", 153).colour},
            {"D", new ColourHex("F3C568", 153).colour},
            {"D#", new ColourHex("F3C568", 153).colour},
            {"Db", new ColourHex("F3C568", 153).colour},
            {"E", new ColourHex("FFF79B", 153).colour},
            {"Eb", new ColourHex("FFF79B", 153).colour},
            {"F", new ColourHex("B4FFA7", 153).colour},
            {"F#", new ColourHex("B4FFA7", 153).colour},
            {"G", new ColourHex("99C2FF", 153).colour},
            {"G#", new ColourHex("99C2FF", 153).colour},
            {"Gb", new ColourHex("99C2FF", 153).colour},
            {"A", new ColourHex("B683FF", 153).colour},
            {"A#", new ColourHex("B683FF", 153).colour},
            {"Ab", new ColourHex("B683FF", 153).colour},
            {"B", new ColourHex("D174CC", 153).colour},
            {"Bb", new ColourHex("D174CC", 153).colour}
        };
        paletteColours = new Dictionary<string, Color>
        {
            {"Light Steel Blue", new Colour255(167, 190, 211).colour},
            {"Colombia Blue", new Colour255(198, 226, 233).colour},
            {"Cream", new Colour255(241, 255, 196).colour},
            {"Apricot", new Colour255(255, 202, 175).colour},
            {"Tan", new Colour255(218, 184, 148).colour}
        };
        #endregion
        
        #region Lesson Lists

        if (!(Directory.Exists(Application.persistentDataPath + "/Files/Lessons/")))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Files/Lessons/");
        }
        if (!(Directory.Exists(Application.persistentDataPath + "/Files/Settings/")))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Files/Settings/");
        }
        melodyLessons = new MelodyLessons();
        harmonyLessons = new HarmonyLessons();
        rhythmLessons = new RhythmLessons();
        timbreLessons = new TimbreLessons();
        LoadLessons();
        
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

    private static void LoadLessons()
    {
        string[] courses = new string[] { "Melody", "Harmony", "Rhythm", "Timbre" };
        foreach(string course in courses)
        {
            string path = Application.persistentDataPath + $"/Files/Lessons/{course}Lessons.dat";
            if (File.Exists(path))
            {
                using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    var data = br.ReadString();
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(data);
                    XmlNode rootNode = xmlDoc.FirstChild;
                    foreach (XmlElement lesson in rootNode.ChildNodes)
                    {
                        switch (course)
                        {
                            case "Melody": 
                                melodyLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
                                melodyLessons.scores[lesson.Attributes[1].Value] = int.Parse(lesson.Attributes[3].Value);
                                break;
                            case "Harmony": 
                                harmonyLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value); 
                                harmonyLessons.scores[lesson.Attributes[1].Value] = int.Parse(lesson.Attributes[3].Value); 
                                break;
                            case "Rhythm": 
                                rhythmLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
                                rhythmLessons.scores[lesson.Attributes[1].Value] = int.Parse(lesson.Attributes[3].Value);
                                break;
                            case "Timbre": 
                                timbreLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value); 
                                timbreLessons.scores[lesson.Attributes[1].Value] = int.Parse(lesson.Attributes[3].Value); 
                                break;
                        }                        
                    }
                }
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode rootNode = xmlDoc.CreateElement(course);
                xmlDoc.AppendChild(rootNode);
                var lessons = new Dictionary<string, bool>();
                var scores = new Dictionary<string, int>();
                switch (course)
                {
                    case "Melody":
                        lessons = melodyLessons.lessons;
                        scores = melodyLessons.scores;
                        break;
                    case "Harmony":
                        lessons = harmonyLessons.lessons;
                        scores = harmonyLessons.scores;
                        break;
                    case "Rhythm":
                        lessons = rhythmLessons.lessons;
                        scores = rhythmLessons.scores;
                        break;
                    case "Timbre":
                        lessons = timbreLessons.lessons;
                        scores = timbreLessons.scores;
                        break;
                }                         
                int counter = 1;
                foreach (var kvp in lessons)
                {
                    XmlElement lessonNode = xmlDoc.CreateElement("Lesson");
                    lessonNode.SetAttribute("number", counter.ToString());
                    lessonNode.SetAttribute("name", kvp.Key);
                    lessonNode.SetAttribute("available", kvp.Value.ToString());
                    lessonNode.SetAttribute("score", scores[kvp.Key].ToString());
                    rootNode.AppendChild(lessonNode);
                    counter++;
                }
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    string xmlData = xmlDoc.DocumentElement.OuterXml;
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(xmlData);
                    bw.Close();
                }
            }
        }
    }    

    public static void UpdateLessonAvailability(string course)
    {
        Dictionary<string, bool> lessonList;
        Dictionary<string, int> scores;
        string path;
        switch (course)
        {
            case "Melody": 
                lessonList = melodyLessons.lessons;
                scores = melodyLessons.scores;
                path = Application.persistentDataPath + "/Files/Lessons/MelodyLessons.dat";
                break;
            case "Harmony":
                lessonList = harmonyLessons.lessons;
                scores = harmonyLessons.scores;
                path = Application.persistentDataPath + "/Files/Lessons/HarmonyLessons.dat";
                break;
            case "Rhythm":
                lessonList = rhythmLessons.lessons;
                scores = rhythmLessons.scores;
                path = Application.persistentDataPath + "/Files/Lessons/RhythmLessons.dat";
                break;
            case "Timbre":
                lessonList = timbreLessons.lessons;
                scores = timbreLessons.scores;
                path = Application.persistentDataPath + "/Files/Lessons/TimbreLessons.dat";
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
            var name = kvp.Key;
            Debug.Log(name);
            lessonNode.SetAttribute("number", counter.ToString());
            lessonNode.SetAttribute("name", name);
            lessonNode.SetAttribute("available", kvp.Value.ToString());
            lessonNode.SetAttribute("score", scores[name].ToString());
            rootNode.AppendChild(lessonNode);
            counter++;
        }
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            string xmlData = xmlDoc.DocumentElement.OuterXml;
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(xmlData);
            bw.Close();
        }
    }
    
    private static void LoadSettings()
    {
        string path = Application.persistentDataPath + "/Files/Settings/UserSettings.dat";
        if (File.Exists(path))
        {
            using(BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                string data = br.ReadString();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);
                XmlNode rootNode = xmlDoc.FirstChild;
                foreach (XmlElement setting in rootNode)
                {
                    settings.valueSettings[setting.Attributes[0].Value] = float.Parse(setting.Attributes[1].Value);
                }
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
            using(FileStream fs = new FileStream(path, FileMode.Create))
            {
                string xmlData = xmlDoc.DocumentElement.OuterXml;
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(xmlData);
                bw.Close();
            }
        }
    }

    public static void UpdateSettings()
    {
        string path = Application.persistentDataPath + "/Files/Settings/UserSettings.dat";
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
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            string xmlData = xmlDoc.DocumentElement.OuterXml;
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(xmlData);
            bw.Close();
        }
    }
}
