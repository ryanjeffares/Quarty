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

    public static Color Colour255(int r, int g, int b, int a=255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }
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

public static class Settings
{
    public static Dictionary<string, float> valueSettings = new Dictionary<string, float>();
}

public class MelodyLessons
{
    public Dictionary<string, bool> lessons;
    
    public MelodyLessons()
    {
        lessons = new Dictionary<string, bool>
        {
            {"Melody Introduction", true},
            {"Notes", false},
            {"Tones And Semitones", false},
            {"Major Scale", false},
            {"Major And Perfect Intervals", false},
            {"Minor Scale", false},
            {"Minor Intervals", false},
            {"Melody Writing", false}
        };
    }
}

public class HarmonyLessons
{
    public Dictionary<string, bool> lessons;

    public HarmonyLessons()
    {
        lessons = new Dictionary<string, bool>
        {
            {"Harmony Introduction", true},
            {"Keys", false},
            {"Triads", false},
            {"Extended Chords", false},
            {"Suspended Chords", false},
            {"Inversions", false},
            {"Circle Of Fifths", false},
            {"Functions", false},
            {"Cadences", false}
        };
    }
}

public class RhythmLessons
{
    public Dictionary<string, bool> lessons;

    public RhythmLessons()
    {
        lessons = new Dictionary<string, bool>
        {
            {"Rhythm Introduction", true},
            {"Note Values", false},
            {"Time Signatures", false},
            {"Dotted Notes", false},
            {"Tied Notes", false},
            {"Syncopation", false},
            {"Compound Time", false},
            {"Triplets", false}
        };
    }
}

public class TimbreLessons
{
    public Dictionary<string, bool> lessons;

    public TimbreLessons()
    {
        lessons = new Dictionary<string, bool>
        {
            {"Timbre Introduction", true},
            {"Strings", false},
            {"Woodwinds", false},
            {"Brass", false},
            {"Percussion", false},
            {"Tremolo", false},
            {"Vibrato", false},
            {"Synthesis", false}
        };
    }
}

public class LoadingTexts
{
    public List<string> loadingLesson, loadingHome;

    public LoadingTexts()
    {
        loadingLesson = new List<string>
        {
            "Tuning pianos...",
            "Warming up...",
            "Sending data to the CIA...",
            "Resurrecting Mozart...",
            "Training musicians...",
            "Loading..."
        };
        loadingHome = new List<string>
        {
            "Cooling down...",
            "Deflating trumpets...",
            "Decomposing...",
            "Loading...",
            "Going home...",
            "Retrieving data from the CIA..."
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
            "{placeholderfunnycomment}",
            "NOT sponsored by the CIA.",
            "Drink responsibly.",
            "Featuring 100 gecs!",
            "сука блять, иди нахуй!",
            "The prequels were just BETTER",
            "Brought to you by Raid Shadow Legends"
        };
    }
}