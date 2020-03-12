using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class GameObjectExtensionMethods
{
    public static string GetGameObjectPath(this GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }
}
