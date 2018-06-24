using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Object = System.Object;

namespace Plugins.ModLab
{
    [System.Serializable]
    public class Mod
    {
        public string Name;
        public string Version;
        public string Author;
        public string Description;
        // loadables
        public Dictionary<string, string> Scripts { get; private set; }
        // post-loadables
        [NonSerialized] public Dictionary<string, ModClass> PostGenClasses;

        public delegate void ScriptChanged(string edit);

        public Mod(string name, string version, string author, string description)
        {
            Name = name;
            Version = version;
            Author = author;
            Description = description;
            Scripts = new Dictionary<string, string>();
            PostGenClasses = new Dictionary<string, ModClass>();
            //DefaultScripts = new Dictionary<string, string>();
        }

        public void Compile()
        {
            CompileTypes();
        }

        public void CompileTypes()
        {
            PostGenClasses = new Dictionary<string, ModClass>();
            //DefaultScripts = new Dictionary<string, string>();
            foreach (var k in Scripts.Keys)
            {
                var v = Scripts[k];
                //DefaultScripts.Add(k, v);
                PostGenClasses.Add(k, new ModClass(v));
                Debug.Log('[' + Name + "] Loaded Script: " + k);
            }
        }

        public Type EditClass(string className, string code)
        {
            if (!Scripts.ContainsKey(className)) return null;
            Scripts[className] = code;
            // reload script
            var t = ModLab.CompileCode(code);
            Debug.Log(t.Name);
            return t;
        }

        public void AddScript(UnityEngine.Object script)
        {
            Scripts.Add(script.name, script.ToString());
        }
    }
}