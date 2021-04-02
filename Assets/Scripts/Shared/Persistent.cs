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
    public static List<string> glossaryWords;
    public static Dictionary<string, string> glossaryDescriptions;
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

        #region Glossary
        glossaryWords = new List<string>();
        glossaryDescriptions = new Dictionary<string, string>
        {
            {"Notes", "A Note is the term we use to describe a sound's pitch. The twelve notes we use in Western Music are A, A#, B, C, C#, D, D#, E, F, F#, G, and G#." },
            {"Melody", "A Melody is a sequency of single notes." },
            {"Scale", "A sequence of Notes that goes up or down." },            
            {"Tone", "A gap of two notes (for example, C to D)." },
            {"Semitone", "A gap of one note (for example, C to C#)." },
            {"Root Note", "The Root is the first note in a scale or chord. If we start a scale on C, that is a C scale, and C is the root of that scale." },
            {"Major Scale", "A scale that is commonly used in Western Music. It is called \"Major\" because it has a bright and happy sound. A Tone (T) and Semitone (S) pattern of T, T, S, T, T, T, S makes a Major Scale." },
            {"Interval", "An interval is the gap between two notes of different pitch." },
            {"Major Third", "The Major Third is the third note in a Major Scale, and has an interval of 2 Tones or 4 Semitones." },
            {"Major Sixth", "The Major Third is the sixth note in a Major Scale, and has an interval of 9 Semitones." },
            {"Major Seventh", "The Major Seventh is the seventh note in a Major Scale, and has an interval of 11 Semitones. It is only 1 semitone below the octave of the root note." },
            {"Minor Scale", "A scale that is commonly used in Western Music. It is called \"Minor\" because it has a dark and sad sound. A Tone (T) and Semitone (S) pattern of T, S, T, T, S, T, T makes a Minor Scale." },
            {"Minor Third", "The Minor Third is the third note in a Minor Scale, and has an interval of 3 Semitones." },
            {"Minor Sixth", "The Minor Third is the sixth note in a Minor Scale, and has an interval of 4 Tones or 8 Semitones." },
            {"Minor Seventh", "The Minor Seventh is the seventh note in a Minor Scale, and has an interval of 5 Tones or 10 Semitones. It is only 1 tone below the octave of the root note." },
            {"Perfect Fourth", "The Perfect 4th appears in the Major and Minor Scale. It has an interval of 5 Semitones." },
            {"Perfect Fifth", "The Perfect 5th appears in the Major and Minor Scale. It has an interval of 7 Semitones." },
            {"Perfect Octave", "The Perfect Octave, or just Octave, has an interval of 12 Semitones or 6 Tones. The Octave of any note is the same note, but at twice the pitch." },
            {"Major Second", "The Major 2nd is the second note in both the Major and Minor scales. It has an interval of 1 Tone or 2 Semitones." },
            {"Minor Second", "The Minor 2nd has an interval of just 1 semitone." },
            {"Harmony", "Harmony is playing more than one note at the same time." },
            {"Chord", "A Chord is any combination of more than one note played together." },
            {"Triad", "A Triad is a chord made of the root, third, and fifth of any scale." },
            {"Major Chord", "A Major Chord is a triad made from the Major Scale, so the Root, Major Third, and Perfect Fifth." },
            {"Minor Chord", "A Minor Chord is a triad made from the Minor Scale, so the Root, Minor Third, and Perfect Fifth." },
            {"Major Seventh Chord", "A Major Seventh Chord is made of a Major Triad with a Major Seventh on top." },
            {"Minor Seventh Chord", "A Minor Seventh Chord is made of a Minor Triad with a Minor Seventh on top." },
            {"Dominant Seventh Chord", "A Dominant Seventh Chord is made of a Major Triad with a Minor Seventh on top." },
            {"Suspended Second Chord", "A Suspended Second chord is made from the Root, Major 2nd, and Perfect Fifth of any scale." },
            {"Suspended Fourth Chord", "A Suspended Second chord is made from the Root, Perfect 4th, and Perfect Fifth of any scale." },
            {"Diminished Chord", "A Diminished Chord is made from the Root, Minor Third, and Tritone. It is dissonant and rarely used." },
            {"Dissonant", "Dissonant means unpleasant sounding and not useful in Harmony. The notes in a Dissonant chord clash with eachother." },
            {"Consonant", "Consonant means pleasant sounding and useful in Harmony. The notes in a Consonant chord sound good together." },
            {"Key", "A Key is a collection of notes and chords, based on a scale, that work together." },
            {"Major Key", "The Major Key is a key based on the Major Scale of any note. It contains all the notes from that scale and chords with those notes as roots. The notes, in order, give us Major, Minor, Minor, Major, Major, Minor, and Diminished Chords." },
            {"Minor Key", "The Minor Key is a key based on the Minor Scale of any note. It contains all the notes from that scale and chords with those notes as roots. The notes, in order, give us Minor, Diminished, Major, Minor, Minor, Major, and Major Chords." },
            {"Chord Progression", "A Chord Progression is a sequency of chords. They can be designed to create certain emotions or feelings of movement." },
            {"Rhythm", "Rhythm is a piece of music's pattern in time. It describes its speed and pattern." }
        };
        GetUserGlossary();
        #endregion
    }

    private static void GetUserGlossary()
    {
        string path = Application.persistentDataPath + "/Files/User/Glossary.dat";
        if (File.Exists(path))
        {
            using (var br = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                var data = br.ReadString();
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);
                foreach(XmlElement term in xmlDoc.FirstChild.ChildNodes)
                {
                    glossaryWords.Add(term.Attributes[0].Value);
                }
            }
        }
    }

    public static void UpdateUserGlossary(string newTerm)
    {
        glossaryWords.Add(newTerm);
        UpdateGlossaryFile();
    }

    public static void UpdateUserGlossary(string[] newTerms)
    {
        foreach(var t in newTerms)
        {
            glossaryWords.Add(t);
        }
        UpdateGlossaryFile();
    }

    private static void UpdateGlossaryFile()
    {
        var path = Application.persistentDataPath + "/Files/User/Glossary.dat";
        var xmlDoc = new XmlDocument();
        XmlNode rootNode = xmlDoc.CreateElement("Glossary");
        xmlDoc.AppendChild(rootNode);
        foreach (var term in glossaryWords)
        {
            XmlElement el = xmlDoc.CreateElement("Term");
            el.SetAttribute("term", term);
            rootNode.AppendChild(el);
        }
        using (var fs = new FileStream(path, FileMode.Create))
        {
            string data = xmlDoc.DocumentElement.OuterXml;
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(data);
                bw.Close();
            }
        }
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
                                if (melodyLessons.scores.ContainsKey(lesson.Attributes[1].Value))
                                {
                                    melodyLessons.scores[lesson.Attributes[1].Value] = int.Parse(lesson.Attributes[3].Value);
                                }                                
                                break;
                            case "Harmony": 
                                harmonyLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
                                if (harmonyLessons.scores.ContainsKey(lesson.Attributes[1].Value))
                                {
                                    harmonyLessons.scores[lesson.Attributes[1].Value] = int.Parse(lesson.Attributes[3].Value);
                                }
                                break;
                            case "Rhythm": 
                                rhythmLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
                                if (rhythmLessons.scores.ContainsKey(lesson.Attributes[1].Value))
                                {
                                    rhythmLessons.scores[lesson.Attributes[1].Value] = int.Parse(lesson.Attributes[3].Value);
                                }
                                break;
                            case "Timbre": 
                                timbreLessons.lessons[lesson.Attributes[1].Value] = bool.Parse(lesson.Attributes[2].Value);
                                if (timbreLessons.scores.ContainsKey(lesson.Attributes[1].Value))
                                {
                                    timbreLessons.scores[lesson.Attributes[1].Value] = int.Parse(lesson.Attributes[3].Value);
                                }
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
                    if (scores.ContainsKey(kvp.Key))
                    {
                        lessonNode.SetAttribute("score", scores[kvp.Key].ToString());
                    }
                    else
                    {
                        lessonNode.SetAttribute("score", "0");
                    }
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
            lessonNode.SetAttribute("number", counter.ToString());
            lessonNode.SetAttribute("name", name);
            lessonNode.SetAttribute("available", kvp.Value.ToString());
            if (scores.ContainsKey(kvp.Key))
            {
                lessonNode.SetAttribute("score", scores[kvp.Key].ToString());
            }
            else
            {
                lessonNode.SetAttribute("score", "0");
            }
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
