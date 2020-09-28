using System;
using System.Collections;
using System.Collections.Generic;
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
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

// This class exists purely so we can implement Shuffle<T> above
public static class ThreadSafeRandom
{
    [ThreadStatic] private static System.Random Local;    
    public static System.Random ThisThreadsRandom
    {
        get
        {
            return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
        }
    }
}

// Data to be used commonly throughout the codebase
public static class SharedData
{    
    public static List<string> allNotes;
    public static List<string> AMajor;
    public static List<int> majorScale;
    public static List<int> minorScale;
    public static Dictionary<string, List<int>> midiNoteLookup;
    public static Dictionary<int, Color> colours;

    static SharedData()
    {
        allNotes = new List<string>
        {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };
        AMajor = new List<string>
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
        colours = new Dictionary<int, Color>
        {
            { 0, new Color(255 / 255f, 117 / 255f, 117 / 255f, 0f) },
            { 1, new Color(255 / 255f, 202 / 255f, 117 / 255f, 0f) },
            { 2, new Color(255 / 255f, 244 / 255f, 117 / 255f, 0f) },
            { 3, new Color(156 / 255f, 255 / 255f, 177 / 255f, 0f) },
            { 4, new Color(117 / 255f, 255 / 255f, 244 / 255f, 0f) },
            { 5, new Color(107 / 255f, 134 / 255f, 255 / 255f, 0f) },
            { 6, new Color(185 / 255f, 107 / 255f, 255 / 255f, 0f) },
            { 7, new Color(255 / 255f, 107 / 255f, 223 / 255f, 0f) }
        };
    }
}
