using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

// Extension functions to be used in the codebase
public static class Typedefs
{
    // Thread safe function to shuffle the items in a generic List<T>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while(n > 1)
        {
            n--;
            int k = ThreadSafeRandom.RandomThread.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }    

    public static bool CheckIfAllStrings<T>(this IList<T> list)
    {
        return list.All(item => item.GetType() == typeof(string));
    }

    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
        => self.Select((item, index) => (item, index));

    public static bool CheckElementsEqualUnordered<T>(this IList<T> list1, IList<T> list2)
        => Enumerable.SequenceEqual(list1.OrderBy(l => l), list2.OrderBy(l => l));    
}

// This class exists purely so we can implement Shuffle<T> above
public static class ThreadSafeRandom
{
    [ThreadStatic] private static System.Random _local;    
    public static System.Random RandomThread => _local ?? (_local = new System.Random
        (unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
}

public class Colour255
{
    public Color colour;
    public Colour255(int r, int g, int b, int a = 255)
    {
        colour = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}

public class ColourHex
{
    public Color colour;
    
    public ColourHex(string hex, int a = 255)
    {
        int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        colour = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
}

public class Settings
{
    public Dictionary<string, float> valueSettings;

    public Settings()
    {
        valueSettings = new Dictionary<string, float>
        {
            {"MusicVolume", 1f},
            {"ObjectVolume", 1f },            
        };
    }
}

public class MelodyLessons
{
    public readonly Dictionary<string, bool> lessons;
    public readonly Dictionary<string, int> scores;
    
    public MelodyLessons()
    {
        lessons = new Dictionary<string, bool>
        {
            {"Melody Introduction", true},
            {"Notes", true},
            {"Tones And Semitones", true},
            {"Major Scale", true},            
            {"Minor Scale", true},            
            {"Perfect Intervals", true},
            {"Major And Minor Second", true},
            {"Melody Writing", true }
        };
        scores = new Dictionary<string, int>
        {            
            {"Notes", 0},
            {"Tones And Semitones", 0},            
            {"Major Scale", 0},
            {"Minor Scale", 0},
            {"Perfect Intervals", 0},            
        };
    }
}

public class HarmonyLessons
{
    public Dictionary<string, bool> lessons;
    public Dictionary<string, int> scores;

    public HarmonyLessons()
    {
        lessons = new Dictionary<string, bool>
        {
            {"Harmony Introduction", true},            
            {"Major Triads", true},
            {"Minor Triads", true},
            {"Major Seventh Chords", true},
            {"Minor Seventh Chords", true},
            {"Suspended Chords", true},
            {"Diminished Chords", true },
            {"Major Keys", true},
            {"Minor Keys", true},            
            {"Chord Progressions", true}            
        };
        scores = new Dictionary<string, int>
        {         
            {"Major Triads", 0},
            {"Minor Triads", 0},
            {"Major Seventh Chords", 0},
            {"Minor Seventh Chords", 0},
            {"Suspended Chords", 0}         
        };
    }
}

public class RhythmLessons
{
    public Dictionary<string, bool> lessons;
    public Dictionary<string, int> scores;

    public RhythmLessons()
    {
        lessons = new Dictionary<string, bool>
        {
            {"Rhythm Introduction", true},            
            {"Time Signatures", true},
            {"Note Values", true},
            {"Tempo", true},
            {"Putting It All Together", true}
        };
        scores = new Dictionary<string, int>
        {
            {"Note Values", 0},
            {"Tempo", 0}                  
        };
    }
}

/*public class TimbreLessons
{
    public Dictionary<string, bool> lessons;
    public Dictionary<string, int> scores;

    public TimbreLessons()
    {
        lessons = new Dictionary<string, bool>
        {
            {"Timbre Introduction", false},
            {"Strings", false},
            {"Woodwinds", false},
            {"Brass", false},
            {"Percussion", false},
            {"Tremolo", false},
            {"Vibrato", false},
            {"Synthesis", true}
        };
        scores = new Dictionary<string, int>
        {
            {"Timbre Introduction", 0},
            {"Strings", 0},
            {"Woodwinds", 0},
            {"Brass", 0},
            {"Percussion", 0},
            {"Tremolo", 0},
            {"Vibrato", 0},
            {"Synthesis", 0}
        };
    }
}*/

public class LoadingTexts
{
    public List<string> loadingLesson, loadingHome;

    public LoadingTexts()
    {
        loadingLesson = new List<string>
        {
            "Tuning pianos...",
            "Warming up...",
            "Stringing guitars...",
            "Resurrecting Mozart...",
            "Training musicians...",
            "Loading...",
            "Bowing violins..."
        };
        loadingHome = new List<string>
        {
            "Cooling down...",
            "Deflating trumpets...",
            "Decomposing...",
            "Loading...",
            "Going home...",
            "Detuning..."
        };
    }
}

public class SplashTexts
{
    public List<string> texts;

    public SplashTexts()
    {
        texts = new List<string>
        {
            "Now with classes!",
            "Hacked together with Perl.",
            "Reliably sourced!",
            "Take the survey!",
            "Mostly bug free!",
            "The newest craze!"
        };
    }
}