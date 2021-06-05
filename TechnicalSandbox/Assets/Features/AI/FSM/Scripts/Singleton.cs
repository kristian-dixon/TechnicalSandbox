using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton <T> where T : class, new() 
{
    static T instance = null;

    public static T GetInstance()
    {
        if(instance == null)
        {
            instance = new T();
        }

        return instance;
    }
}
