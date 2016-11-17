using UnityEngine;
using System.IO;
using System.Collections;
using System;

public class ResUtil
{
    public static void SaveResourceFile(WWW www,string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        if (!File.Exists(path))
        {
            FileStream fs = File.Create(path);
            fs.Write(www.bytes, 0, www.bytes.Length);
            fs.Flush();
            fs.Close();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="strs"></param>
    /// <param name="overload">是否覆盖 true覆盖</param>
    public static void AddText(string path,string[] strs,bool overload=true)
    {
        FileStream fs = null;

        if (!Directory.Exists(Application.persistentDataPath))
        {
            Directory.CreateDirectory(Application.persistentDataPath);
        }

        if (!File.Exists(Application.persistentDataPath + "/"+ path))
        {
            fs=File.Create(Application.persistentDataPath + "/" + path);
            fs.Close();
            Debug.Log("文件不存在，需要添加");
        }

        StreamWriter sw = null;

        if (overload)
        {
            using (sw = new StreamWriter(Application.persistentDataPath + "/" + path, false))
            {
                for (int i = 0; i < strs.Length; i++)
                {
                    sw.WriteLine(strs[i]);
                    sw.Flush();
                }

                sw.Close();
                Debug.Log("写入完成");
            }
        }
        else
        {
            using (sw = File.AppendText(Application.persistentDataPath + "/" + path))
            {
                for (int i = 0; i < strs.Length; i++)
                {
                    sw.WriteLine(strs[i]);
                    sw.Flush();
                }

                sw.Close();
                Debug.Log("追加完成");
            }
        }
    }
    ////保存帐号密码
    //public static void SaveGameInfo(string zhanghao,string password)
    //{
    //    if (!Directory.Exists("file:///" + Application.persistentDataPath))
    //    {
    //        Directory.CreateDirectory("file:///" + Application.persistentDataPath);
    //    }
    //    if (!File.Exists("file:///" + Application.persistentDataPath + "/Document.txt"))
    //    {
    //        File.Create("file:///" + Application.persistentDataPath + "/Document.txt");
    //    }

    //    using (StreamWriter sw = new StreamWriter("file:///" + Application.persistentDataPath + "/Document.txt", false))
    //    {
    //        sw.WriteLine(zhanghao);
    //        sw.WriteLine(password);
    //        sw.Flush();
    //        sw.Close();
    //    }

    //    //ArrayList arrlist = readFile("Assets/StreamingAssets/App/Document.txt");
    //   // readText();
    //}
    ////保存帐号类型和authtoken
    //public static void SaveGameInfoTypeAndAuthtoken(string type, string authtoken)
    //{
    //    StreamWriter sw = File.AppendText("Assets/StreamingAssets/App/Document.txt");
    //    //StreamWriter sw = new StreamWriter("Assets/StreamingAssets/App/Document.txt", false);
    //    sw.WriteLine(type);
    //    sw.WriteLine(authtoken);
    //    sw.Flush();
    //    sw.Close();
    //}

    public static void readText()
    {
        string[] lines = File.ReadAllLines(@"Assets/StreamingAssets/App/Document.txt");
        foreach (string line in lines)
        {
            Debug.Log(line);
        }
    }
    protected static ArrayList readFile(string name)
    {
        ArrayList dataList = null;
        StreamReader reader = null;
        try
        {
            TextAsset ta = (TextAsset)Resources.Load(name, typeof(TextAsset));
            Stream stream = new MemoryStream(ta.bytes);
            reader = new StreamReader(stream);
            dataList = new ArrayList();
            string read;
            while ((read = reader.ReadLine()) != null)
            {
                //读数据
                dataList.Add(read);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        reader.Close();

        return dataList;
    }
}
