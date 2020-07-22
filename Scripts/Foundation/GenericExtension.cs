using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UHelper
{


public static class GenericExtension
{
    public static List<T> Clone<T>(this List<T> InList)    {
        T[] _newArr = new T[InList.Count];
        InList.CopyTo(_newArr);
        return _newArr.ToList();
    }
    
}



}
