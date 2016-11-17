using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class ObjectUtil
{
    /// <summary>
    /// 根据配置表字符串的值列表，创建指定数据对象
    /// </summary>
    /// <typeparam name="T">返回的类型，如果是带继承的类型，请尽量手动解析配置，字段的顺序可能对应不上</typeparam>
    /// <param name="values"></param>
    /// <param name="isDeclaredOnly">是否包含继承的字段,默认为false</param>
    /// <returns></returns>
    public static T CreateClassInstance<T>(string[] values, bool isDeclaredOnly=false) where T :new ()
    {
        T t_data = new T();
        Type t = typeof(T);
        FieldInfo[] fields;

        if(isDeclaredOnly)
        {
            fields=t.GetFields(BindingFlags.Public | BindingFlags.Instance);
        }
        else
        {
            fields=t.GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
        }

        for (int i=0;i<fields.Length;i++)
        {
            if (string.IsNullOrEmpty(values[i]))
                continue;

            object t_tempValue= values[i];

            if(fields[i].FieldType==typeof(bool)|| fields[i].FieldType == typeof(Boolean))
            {
                t_tempValue = int.Parse(values[i]);
            }

            fields[i].SetValue(t_data, ChangeType(t_tempValue, fields[i].FieldType));
        }

        return t_data;
    }

    public static object ChangeType(object value,Type type)
    {
        if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
        if (value == null) return null;
        if (type == value.GetType()) return value;
        if (type.IsEnum)
        {
            if (value is string)
                return Enum.Parse(type, value as string);
            else
                return Enum.ToObject(type, value);
        }
        if (!type.IsInterface && type.IsGenericType)
        {
            Type innerType = type.GetGenericArguments()[0];
            object innerValue = ChangeType(value, innerType);
            return Activator.CreateInstance(type, new object[] { innerValue });
        }
        if (value is string && type == typeof(Guid)) return new Guid(value as string);
        if (value is string && type == typeof(Version)) return new Version(value as string);
        if (!(value is IConvertible)) return value;
        return Convert.ChangeType(value, type);
    }
}
