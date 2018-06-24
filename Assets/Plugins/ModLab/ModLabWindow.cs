using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.VersionControl;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Plugins.ModLab
{
    public class ModLabWindow : EditorWindow
    {

        private Vector2 _scrollRect;
        private string _modName, _modVersion, _modDescription, _modLocation, _modAuthor;
        private bool _buildingMod, _builtMod;
        private int _activeTab;
        private List<Object> _givenFiles = new List<Object>();
        private List<int> _selectedFiles = new List<int>();
        private GUIStyle _itemOffStyle;
        
        private readonly Dictionary<Type, string> _icons = new Dictionary<Type, string>
        {
            {typeof(MonoScript), "cs Script Icon"},
            {typeof(GameObject), "GameObject Icon"},
            {typeof(Texture2D), "textureExternal"},
            {typeof(TextAsset), "TextAsset Icon"}
        };
        
        private readonly List<Type> _supportedTypes = new List<Type>()
        {
            typeof(MonoScript)
        };

        [MenuItem("Window/Mod Lab")]
        public static void Init()
        {
            var window = GetWindow<ModLabWindow>(utility: false, focus: true, title: "Mod Lab");
            window.Show();
        }

        private void OnGUI()
        {
            if (_itemOffStyle == null)
            {
                var style = new GUIStyle("ObjectPickerResultsEven");
                _itemOffStyle = style;
            }

            GUI.enabled = !_buildingMod;
            GUILayout.BeginHorizontal();
            GUI.color = _activeTab == 0 ? Color.green : Color.white;
            if (GUILayout.Button("Information")) _activeTab = 0;
            GUI.color = _activeTab == 1 ? Color.green : Color.white;
            if (GUILayout.Button("Files")) _activeTab = 1;
            GUILayout.Space(position.size.x - 6f - 165f);

            GUI.enabled = !string.IsNullOrEmpty(_modName) && !string.IsNullOrEmpty(_modVersion) && !string.IsNullOrEmpty(_modAuthor) /*&& !string.IsNullOrEmpty(_modLocation)*/;
            GUI.color = _activeTab == 2 ? Color.green : Color.yellow;
            if (GUILayout.Button("Build")) _activeTab = 2;
            GUI.color = Color.white;
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            switch (_activeTab)
            {
                case 0: InformationTab(); break;
                case 1: FilesTab(); break;
                case 2: BuildTab(); break;
            }

            if (_activeTab != 2) _buildingMod = _builtMod = false;
            
            Repaint();
        }

        private void InformationTab()
        {
            var nameRect = new Rect(8f, 24f, position.size.x - 16f, 16f);
            var versionRect = new Rect(8f, nameRect.y + nameRect.height + 2, position.size.x - 16f, 16f);
            var authorRect = new Rect(8f, versionRect.y + versionRect.height + 2, position.size.x - 16f, 16f);
            var descriptionRect = new Rect(8f, authorRect.y + authorRect.height + 2, position.size.x - 16f, 64f);
            var locationRect = new Rect(8f, position.size.y - 16f - 8f, position.size.x - 16f - 40f, 16f);
            var locationBtnRect = new Rect(locationRect.x + locationRect.width + 2f, locationRect.y, 40f, 16f);

            GUI.color = string.IsNullOrEmpty(_modName) ? Color.yellow : Color.white;
            _modName = EditorGUI.TextField(nameRect, "Name*", _modName);
            GUI.color = string.IsNullOrEmpty(_modVersion) ? Color.yellow : Color.white;
            _modVersion = EditorGUI.TextField(versionRect, "Version*", _modVersion);
            GUI.color = string.IsNullOrEmpty(_modAuthor) ? Color.yellow : Color.white;
            _modAuthor = EditorGUI.TextField(authorRect, "Author*", _modAuthor);
            GUI.color = Color.white;
            _modDescription = EditorGUI.TextField(descriptionRect, "Description", _modDescription);
//            GUI.color = string.IsNullOrEmpty(_modLocation) ? Color.yellow : Color.white;
//            _modLocation = EditorGUI.TextField(locationRect, "Save Location", _modLocation);
            GUI.color = Color.white;
//            if (GUI.Button(locationBtnRect, "..."))
//            {
//                var loc = EditorUtility.SaveFilePanelInProject("Choose Mod Location", "Mod", "mod", string.Empty);
//                if (string.IsNullOrEmpty(loc)) return;
//                _modLocation = loc;
//            }
        }

        private void FilesTab()
        {
            var dropArea = new Rect(8f, 24f, position.size.x - 16f, position.size.y - 24f - 24f);
            GUI.Box(dropArea, string.Empty);

            var e = Event.current;
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (e.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(e.mousePosition)) return;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
//                        if (_givenFiles.All(ev => ev.GetType() == typeof(Behaviour)))
//                        {
                            foreach (var obj in DragAndDrop.objectReferences)
                            {
                                if (_givenFiles.Contains(obj)) continue;
                                _givenFiles.Add(obj);
                            }

                            e.Use();
//                        }
//                        else DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                    break;
            }

            GUILayout.BeginArea(dropArea);
            var y = 0;
            foreach (var item in _givenFiles)
            {
                var iconRect = new Rect(0f, y, 18f, 18f);
                var labelRect = new Rect(iconRect.x + 16f, y, dropArea.width, 16f);
                var totalRect = new Rect(0f, labelRect.y, labelRect.width + iconRect.width, 16f);
                var type = item.GetType();
                var icon = GetIcon(type);
                var typeSupported = _supportedTypes.Contains(type);
                if(!typeSupported) GUI.color = new Color(1,0,0,0.5f);
                GUI.Box(totalRect, string.Empty, _selectedFiles.Contains(item.GetInstanceID()) ? "OL SelectedRow" : GUIStyle.none);
                GUI.Box(iconRect, string.IsNullOrEmpty(icon) ? null : EditorGUIUtility.IconContent(icon).image, "Label");
                if (GUI.Button(labelRect, item.name + (!typeSupported ?  " (" + type.Name + " is not supported)" : string.Empty), "Label"))
                {
                    var id = item.GetInstanceID();
                    if (_selectedFiles.Contains(id)) _selectedFiles.Remove(id);
                    else _selectedFiles.Add(id);
                }
                GUI.color = Color.white;
                y += 16;
            }
            GUILayout.EndArea();
            
            GUI.Label(new Rect(8f, position.size.y - 20f, position.size.x - 16f, 16f), "Files: " + _givenFiles.Count);
            GUI.enabled = _selectedFiles.Count > 0;
            if (GUI.Button(new Rect(position.size.x - 24f - 42f, position.size.y - 20f, 60f, 16f), "Delete"))
            {
                foreach (var a in _selectedFiles)
                {
                    _givenFiles.Remove(_givenFiles.FirstOrDefault(f => f.GetInstanceID() == a));
                }
                _selectedFiles.Clear();
            }

            GUI.enabled = true;
        }

        private string GetIcon(Type type)
        {
            if (!_icons.ContainsKey(type)) return string.Empty;
            return _icons[type];
        }

        private void BuildTab()
        {
            if (!_buildingMod && !_builtMod)
            {
                _buildingMod = true;
                ModLab.ClearDebug();
                var mod = new Mod(_modName, _modVersion, _modAuthor, _modDescription);
                foreach (var s in _givenFiles)
                {
                    ModLab.AddDebug("Adding asset: " + s.name);
                    var type = s.GetType();
                    if(type == typeof(MonoScript)) mod.AddScript(s);
                    else ModLab.AddDebug("Unable to add asset: " + s.name + " (" + s.GetType() + ")");
                }
                ModLab.SerializeMod(mod);
                _builtMod = true;
                _buildingMod = false;
            }
            
            var logRect = new Rect(8f, 24f, position.size.x - 16f, position.size.y - 24f - 8f);
            GUI.enabled = false;
            EditorGUI.TextArea(logRect, ModLab.DebugText);
            GUI.enabled = true;
        }

        private string StripAsset()
        {
            var split = Application.dataPath.Split(new string[] {"/Assets"}, StringSplitOptions.None);
            return split[0] + '/';
        }
    }
}