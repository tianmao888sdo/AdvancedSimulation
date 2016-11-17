using System;
using UnityEngine;
public class ScriptUtil
{
    ///// <summary>
    ///// 给游戏对象添加脚本，待测试
    ///// </summary>
    ///// <param name="script"></param>
    ///// <param name="scriptName"></param>
    ///// <param name="gameObject"></param>
    ///// <returns></returns>
    //public static void AddComponentOfScript(UnityEngine.Object script, string path, string scriptName, GameObject gameObject)
    //{
    //    UnityEngine.Object t_obj;

    //    if (Global.It.LocalResMgr.GetLocalResource(script, path, scriptName, false, out t_obj))
    //    {
    //        ResCallback(path, scriptName, t_obj, gameObject);
    //    }
    //    else
    //    {
    //        t_obj = Global.It.LocalResMgr.GetLocalResource(script, path + GlobalConfig.BUNDLE_EXT_NAME, scriptName,
    //        (t_path, name, obj, args) => { ResCallback(t_path, name, obj, gameObject); },
    //        ResourceType.script, Order.o_default, false);
    //    }
    //}

    //private static void ResCallback(string path, string name, UnityEngine.Object obj, GameObject gameObject)
    //{
    //    Type t_script;
    //    TextAsset asset = obj as TextAsset;
    //    System.Reflection.Assembly assembly = System.Reflection.Assembly.Load(asset.bytes);
    //    t_script = assembly.GetType(name);
    //    gameObject.AddComponent(t_script);
    //}
}