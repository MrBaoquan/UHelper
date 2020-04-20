using System.Linq;
using System.Reflection;
using System;
using UnityEngine;

namespace UHelper
{
    
public static class UReflection
{
    public static string GetPropertyValue(string InField, object InObject)
    {
        try
        {
            Type _class = InObject.GetType();
            object o = _class.GetProperty(InField).GetValue(InObject, null);
            string Value = Convert.ToString(o);
            if (string.IsNullOrEmpty(Value)) return null;
            return Value;
        }
        catch
        {
            return null;
        }
    }


    public static bool SetFieldValue<T>( object InObject, string InField,T Value)
    {
        try
        {
            Type _class = InObject.GetType();
            FieldInfo _fieldInfo = _class.GetField(InField);
            object _safeValue = Convert.ChangeType(Value, _fieldInfo.FieldType);
            _fieldInfo.SetValue(InObject,_safeValue);
            return true;
        }
        catch(Exception exc)
        {
            Debug.LogError(exc.Message);
            return false;
        }
    }

    public static System.Type[] SubClasses(Type InBaseClass)
    {
        return Assembly.GetAssembly(InBaseClass).GetTypes().Where(_type=>_type.IsSubclassOf(InBaseClass)).ToArray();
    }


}

}