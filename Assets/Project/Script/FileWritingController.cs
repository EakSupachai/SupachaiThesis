using System.IO;
using UnityEngine;

public class FileWritingController
{
    public static void SaveResult(string fileName, string result)
    {
        string path = Application.dataPath + "/TestResult/";
        Directory.CreateDirectory(path);
        path = path + fileName + ".txt";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, result);
        }
    }
}
