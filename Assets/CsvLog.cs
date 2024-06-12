using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DateTime = System.DateTime;
using String = System.String;


namespace csvlog
{
    public class CsvLog
    {
        string fname = new String("");
        string path = new String("");
        string content = new String("");

        public CsvLog(string fdesc = "")
        {
            fname = System.DateTime.Now.ToString("yyyyMMdd_HH-mm-ss") + "_" + fdesc + ".csv";
            path = Path.Combine(Application.persistentDataPath, fname);
        }

        public void writeLine(object str)
        {
            var line = string.Format("{0},{1}\n", str, GetTimestamp(System.DateTime.Now));
            Debug.Log(line);
            content += line;
        }

        public void saveFile()
        {
            Debug.Log("Save file in: " + path);
            Debug.Log("Content: " + content);
            File.WriteAllText(path, content);
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }


    }
}
