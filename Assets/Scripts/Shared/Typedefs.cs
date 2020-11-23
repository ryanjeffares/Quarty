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
    public Colour255(int r, int g, int b)
    {
        colour = new Color(r / 255f, g / 255f, b / 255f);
    }
}

public class Settings
{
    public static Dictionary<string, float> valueSettings = new Dictionary<string, float>();
}