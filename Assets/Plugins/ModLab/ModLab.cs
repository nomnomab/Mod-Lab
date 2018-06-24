using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using UnityEditor;
using UnityEngine;

namespace Plugins.ModLab
{
	public static class ModLab
	{
		[PreferenceItem("Mod Lab")]
		public static void ModLabItem()
		{
			var unityLocation = PlayerPrefs.HasKey("unityLoc") ? PlayerPrefs.GetString("unityLoc") : string.Empty;
			// D:\Users\nomno\Documents\Unity\Utilities\Library\ScriptAssemblies
			var tempLocation = StripAsset() + @"Library\ScriptAssemblies\";

			EditorGUILayout.HelpBox(@"Example Location: D:\Program Files\Unity2017.3.1f1\Editor\Data\Managed\UnityEngine\", MessageType.Info);
			GUI.enabled = false;
			EditorGUILayout.LabelField("Unity Managed/UnityEngine Folder");
			GUILayout.BeginHorizontal();
			EditorGUILayout.TextField(string.Empty, unityLocation);
			GUI.enabled = true;
			if (GUILayout.Button("...", GUILayout.Width(60)))
			{
				var loc = EditorUtility.OpenFolderPanel("Select Unity Managed Folder", "", "");
				if (string.IsNullOrEmpty(loc)) return;
				PlayerPrefs.SetString("unityLoc", loc + '/');
			}
			GUI.enabled = false;
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Library/ScriptAssemblies Folder");
			EditorGUILayout.TextField(string.Empty, tempLocation);
			GUI.enabled = true;
		}
		
		private static string StripAsset()
		{
			var split = Application.dataPath.Split(new string[] {"/Assets"}, StringSplitOptions.None);
			return split[0] + '/';
		}
		
		public static string DebugText
		{
			get
			{
				var sB = new StringBuilder();
				foreach (var s in DebugTextQueue) sB.AppendLine(s);
				return sB.ToString();
			}
		}

		private static readonly Queue<string> DebugTextQueue = new Queue<string>();

		public static void AddDebug(object obj)
		{
			DebugTextQueue.Enqueue(obj.ToString());
			if (DebugTextQueue.Count > 30) DebugTextQueue.Dequeue();
		}

		public static void ClearDebug()
		{
			DebugTextQueue.Clear();
		}
		
		public static Type CompileCode(string content)
		{
			var provider = new CSharpCodeProvider();
			var p = new CompilerParameters();
			var managedLoc = PlayerPrefs.GetString("unityLoc");
			p.ReferencedAssemblies.Add(managedLoc + @"UnityEngine.CoreModule.dll");
			p.ReferencedAssemblies.Add(managedLoc + @"UnityEngine.dll");
			p.ReferencedAssemblies.Add(managedLoc + @"UnityEngine.PhysicsModule.dll");
			p.ReferencedAssemblies.Add(StripAsset() + @"Library\ScriptAssemblies\Assembly-CSharp-firstpass.dll");
			var results = provider.CompileAssemblyFromSource(
				p, content);
			var groupName = content.Contains("namespace ") 
				? content.Split(new string[] { "namespace " }, StringSplitOptions.None)[1].Split(' ')[0]
				: string.Empty;
			var className = content.Split(new string[] { "class " }, StringSplitOptions.None)[1].Split(' ')[0];
			groupName = Regex.Replace(groupName, @"\t|\n|\r|{|//|\\", "");
			className = Regex.Replace(className, @"\t|\n|\r|{|//|\\", "");
		    Debug.Log(groupName + (string.IsNullOrEmpty(groupName) ? "" : ".") + className);
			
			if (results.Errors.HasErrors)
			{
				var sb = new StringBuilder();

				foreach (CompilerError error in results.Errors)
				{
					sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
				}

				throw new InvalidOperationException(sb.ToString());
			}
			
			return results.CompiledAssembly.GetType(groupName + (string.IsNullOrEmpty(groupName) ? "" : ".") + className);
		}

		public static object GetObject(Type type, params object[] args)
		{
			return Activator.CreateInstance(type, args);
		}

		public static MethodInfo GetMethod(Type type, string method)
		{
			return type.GetMethod(method);
		}

		public static MethodInfo[] GetMethods(Type type)
		{
			return type.GetMethods();
		}

		public static FieldInfo GetField(Type type, string field)
		{
			return type.GetField(field);
		}
		
		public static FieldInfo[] GetFields(Type type)
		{
			return type.GetFields();
		}

		public static void SetField(object obj, FieldInfo field, object value)
		{
			field.SetValue(obj, value);
		}

		public static PropertyInfo GetProperty(Type type, string property)
		{
			return type.GetProperty(property);
		}

		public static void SerializeMod(Mod mod)
		{
			var modsLoc = Application.dataPath + "/Mods";
			AddDebug("Creating mod at path (" + modsLoc + ")");
			if (!Directory.Exists(modsLoc))
			{
				Directory.CreateDirectory(modsLoc);
				AddDebug("Directory did not exist, created it now");
			}
			var fs = new FileStream(modsLoc + "/" + mod.Name + ".mod", FileMode.Create);
			var formatter = new BinaryFormatter();
			try
			{
				formatter.Serialize(fs, mod);
				AddDebug("Serialized data to mod");
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
			finally
			{
				fs.Close();
				AddDebug("Created new asset: " + modsLoc + "/" + mod.Name + ".mod");
				Debug.Log("Created new asset: " + modsLoc + "/" + mod.Name + ".mod");
			}

			AddDebug("\nSuccess!");
		}

		public static Mod DeserializeMod(string modName)
		{
			var modsLoc = Application.dataPath + "/Mods";
			var fs = new FileStream(modsLoc + '/' + modName, FileMode.Open);
			Mod mod = null;
			try
			{
				var formatter = new BinaryFormatter();
				mod = (Mod) formatter.Deserialize(fs);
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
			finally
			{
				fs.Close();
				Debug.Log("Loaded mod: " + mod?.Name);
			}

			return mod;
		}

		public static Mod DeserializeModAtFile(string file)
		{
			file = file.Replace('\\', '/');
			var split = file.Split('/');
			file = split[split.Length - 1];
			Debug.Log(file);
			return DeserializeMod(file);
		}
	
	}
}
