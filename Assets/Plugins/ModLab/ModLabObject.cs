using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Plugins.ModLab
{
    public class ModLabObject
    {
        public bool ValidType;
        private Type _validType;
        
        private readonly List<Mod> _loadedMods;

        public ModLabObject()
        {
            // create lists
            _loadedMods = new List<Mod>();
            // load in mods
            LoadMods();
            // load in scripts
            CompileMods();
        }
        
        // GRABBERS

        public Mod GetMod(string name)
        {
            return _loadedMods.FirstOrDefault(m => m.Name == name);
        }
        
        //

        private void LoadMods()
        {
            var modsLoc = Application.dataPath + "/Mods";
            if (!Directory.Exists(modsLoc)) Directory.CreateDirectory(modsLoc);
            var files = Directory.GetFiles(modsLoc, "*.mod");
            foreach (var file in files)
            {
                var mod = ModLab.DeserializeModAtFile(file);
                _loadedMods.Add(mod);
            }

            Debug.Log("Loaded " + _loadedMods.Count + " mod(s)");
        }

        private void CompileMods()
        {
            foreach (var mod in _loadedMods) mod.Compile();
            Debug.Log("Compiled mods");
        }

        public void RecompileScript(string modName, string scriptName, string newCode)
        {
            // save script change
            var loadedMod = _loadedMods.FirstOrDefault(mod => mod.Name == modName);
            var t = loadedMod?.EditClass(scriptName, newCode);
            ModLab.SerializeMod(loadedMod);
            #if UNITY_EDITOR
            var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null,null);
            #endif
            SceneManager.LoadScene(0);
        }
    }
}