using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Plugins.ModLab
{
    public class ModLabDynamicSample : MonoBehaviour
    {
        public object ReferenceObject;
        public string ReferenceObjectName;
        
        private List<Mod> _loadedMods;
        private ObservableCollection<object> _objectCache;
        private Dictionary<object, List<FieldInfo>> _requesterCache;
        private List<ModLabObjectChangedEvent> _changedEvents;
        
        private void Start()
        {
            _loadedMods = new List<Mod>();
            _objectCache = new ObservableCollection<object>();
            _requesterCache = new Dictionary<object, List<FieldInfo>>();
            _changedEvents = new List<ModLabObjectChangedEvent>();
            _objectCache.CollectionChanged += ObjectCacheOnCollectionChanged;
            
            LoadMods();
            LoadScripts();

            var mod = GetMod("Sample Mod");
            var behaviour = GetClass(mod, "SampleBehaviour");
            AddModComponent(behaviour);

            var r = GetType().GetField("ReferenceObject");
            ReferenceObject = TakeCopyOfObject("Plugins.ModLab.SampleBehaviour", ref r);
        }

        private void ObjectCacheOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var o in e.NewItems)
            {
                Debug.Log(o + " has changed.");
            }
        }

        private void Update()
        {
            ReferenceObjectName = ReferenceObject == null ? "Not set" : ReferenceObject.GetType().FullName;
        }

        private void LoadMods()
        {
            var modsLoc = Application.dataPath + "/Mods";
            if (!Directory.Exists(modsLoc)) Directory.CreateDirectory(modsLoc);
            var files = Directory.GetFiles(modsLoc, "*.mod");
            foreach (var file in files)
            {
                Debug.Log(file);
                var mod = ModLab.DeserializeModAtFile(file);
                _loadedMods.Add(mod);
            }
        }
        
        private void LoadScripts()
        {
            foreach (var mod in _loadedMods) mod.CompileTypes();
        }

        private void AddModComponent(ModClass modClass)
        {
            var obj = ModLab.GetObject(modClass.Type);
            _objectCache.Add(obj);
            Debug.Log("Added ModComponent to the cache: " + modClass.Type.Name);
        }

        public object TakeCopyOfObject(string fullName, ref FieldInfo info)
        {
            var obj = _objectCache.FirstOrDefault(o => o.GetType().FullName == fullName);
            // store info
            if (!_requesterCache.ContainsKey(obj)) _requesterCache.Add(obj, new List<FieldInfo>());
            _requesterCache[obj].Add(info);
            return obj;
        }

        private Mod GetMod(string mod)
        {
            return _loadedMods.FirstOrDefault(m => m.Name == mod);
        }

        private ModClass GetClass(Mod mod, string modClass)
        {
            return mod.PostGenClasses[modClass];
        }
    }
}