using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Plugins.ModLab
{
    [System.Serializable]
    public class ModClass
    {
        public Type Type { get; private set; }
        public MethodInfo[] Methods { get; private set; }
        public FieldInfo[] Fields { get; private set; }

        private object Object;

        /// <summary>
        /// Do not use this constructor. Purely for postgen usage.
        /// </summary>
        public ModClass()
        {            
        }

        public ModClass(string code)
        {
            Type = ModLab.CompileCode(code);
            Object = ModLab.GetObject(Type);
            Methods = ModLab.GetMethods(Type);
            Fields = ModLab.GetFields(Type);
        }

        public void SetField(string field, object value)
        {
            Fields.FirstOrDefault(f=>f.Name == field)?
                .SetValue(Object, value);
        }

        public object GetFieldValue(string field)
        {
            return Fields.FirstOrDefault(f => f.Name == field)?.
                GetValue(Object);
        }
    }
}