using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.ModLab
{
    public class SingleReferenceHandler : MonoBehaviour
    {
        public List<Behaviour> Scripts;
        private List<Component> _loadedMonobehaviours;
        private List<object> _loadedObjects;
        private static SingleReferenceHandler _single;

        private void Awake()
        {
            Debug.Log(this.GetType().Assembly.FullName);
            _single = this;
            _loadedMonobehaviours = new List<Component>();
            _loadedObjects = new List<object>();

            foreach (var script in Scripts)
            {
                _loadedMonobehaviours.Add(gameObject.AddComponent(script.GetType()));
                Debug.Log("Added MonoBehaviour: " + script.GetType().Name);
            }
            
            // normal scripts
            _loadedObjects.Add(new ModLabObject());
        }

        public static T GetMonobehaviour<T>() where T : class 
        {
            return _single._loadedMonobehaviours.FirstOrDefault(s => s.GetType() == typeof(T)) as T;
        }

        public static T GetScript<T>() where T : class
        {
            return _single._loadedObjects.FirstOrDefault(s => s.GetType() == typeof(T)) as T;
        }
        
        public T GetScriptSingle<T>() where T : class
        {
            return _single._loadedObjects.FirstOrDefault(s => s.GetType() == typeof(T)) as T;
        }
    }
}