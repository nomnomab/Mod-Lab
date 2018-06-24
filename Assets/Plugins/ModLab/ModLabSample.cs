using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Plugins.ModLab
{
    public class ModLabSample : MonoBehaviour
    {
        public InputField DebugText;
        public Button DebugRecompileButton;
        private string DebugCodeText => DebugText.text;

        private void Start()
        {
            var modLabObj = SingleReferenceHandler.GetScript<ModLabObject>();
            var sampleMod = modLabObj.GetMod("Sample Mod");
            var behaviour = sampleMod?.PostGenClasses["SampleBehaviour"];
            DebugText.text = sampleMod?.Scripts["SampleBehaviour"];
            gameObject.AddComponent(behaviour?.Type);
            DebugRecompileButton.onClick.AddListener(() =>
            {
                modLabObj.RecompileScript("Sample Mod", "SampleBehaviour", DebugCodeText);
            });
        }
    }
}