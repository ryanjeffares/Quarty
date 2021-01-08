using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class UserSettings
{
    private static string _path = Application.persistentDataPath + "/files/settings.json";

    public static Dictionary<string, dynamic> settings;

    //static UserSettings()
    //{
    //    if (System.IO.File.Exists(_path))
    //    {
    //        JObject obj = JObject.Parse(_path);
    //        settings = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(_path);
    //        Debug.Log(obj);
    //    }
    //    else
    //    {
    //        var obj = JsonConvert.SerializeObject(settings);
    //    }
    //}
}
