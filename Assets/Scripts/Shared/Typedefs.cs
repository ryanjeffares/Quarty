using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

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
    [ThreadStatic] private static System.Random _local;    
    public static System.Random ThisThreadsRandom => _local ?? (_local = new System.Random
        (unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
}
