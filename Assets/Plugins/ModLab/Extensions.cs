using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.ModLab
{
    public static class Extensions
    {
        public static Component ReplaceComponent(this GameObject gameObject, Type a, Type b)
        {
            Object.Destroy(gameObject.GetComponent(a));
            var c1 = gameObject.GetComponent(a);
            var c2 = gameObject.AddComponent(b);
            
            Object.Destroy(c1);
            
            return c2;
        }
    }
}