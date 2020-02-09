using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileWritingController
{
    public static void SaveResult(string subject, string result)
    {
        string path = Application.dataPath + "/TestResult/";
        path = path + subject + ".txt";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, result);
        }
    }
}
