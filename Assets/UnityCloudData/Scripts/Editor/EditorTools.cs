using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace UnityCloudData
{
	/// <summary>
	/// Return a list of components from all prefabs in the project folder.  Code adapted from EZGUI.
	/// </summary>
	public class EditorTools 
	{
		/// <summary>
		/// Parse the command line into a key/value pair dictionary.  Arguments that begin with - are keys
		/// </summary>
		/// <returns>
		/// A <see cref="Dictionary<System.String, System.String>"/>
		/// </returns>
		public static Dictionary<string, string> ParseCommandLine()
		{
			string[] args = System.Environment.GetCommandLineArgs();
			Dictionary<string, string> clDict = new Dictionary<string, string>();
			string lastArg = null;
			
			for(int i=1; i < args.Length; i++)
			{
				if(args[i].Substring(0,1) == "-")
				{
					lastArg = args[i].Substring(1, args[i].Length - 1);
					if(false == clDict.ContainsKey(lastArg))
						clDict.Add(lastArg, "");
				}
				else if(lastArg != null)
				{
					clDict[lastArg] = args[i];
					lastArg = null;
				}
			}
			
			return clDict;
		}
		
		/// <summary>
		/// Find all components in all prefabs of the given type
		/// </summary>
		/// <returns>
		/// A <see cref="List<Component>"/>
		/// </returns>
		public static List<Component> GetAllComponentsInPrefabs<T>() where T : Component
		{
			string[] files;
			GameObject obj;
			Component[] c;
			List<Component> components = new List<Component>();
		
			// Stack of folders:
			Stack<string> stack = new Stack<string>();
		
			// Add root directory:
			stack.Push(Application.dataPath);
		
			// Continue while there are folders to process
			while (stack.Count > 0)
			{
				// Get top folder:
				string dir = stack.Pop();
		
				try
				{
					// Get a list of all prefabs in this folder:
					files = Directory.GetFiles(dir, "*.prefab");
		
					// Process all prefabs:
					for (int i = 0; i < files.Length; ++i)
					{
						// Make the file path relative to the assets folder:
						files[i] = files[i].Substring(Application.dataPath.Length - 6);
		
						obj = (GameObject)AssetDatabase.LoadAssetAtPath(files[i], typeof(GameObject));
		
						if (obj != null)
						{
							c = obj.GetComponentsInChildren<T>(true);
		
							for (int j = 0; j < c.Length; ++j)
								components.Add(c[j]);
						}
					}
		
					// Add all subfolders in this folder:
					foreach (string dn in Directory.GetDirectories(dir))
					{
						stack.Push(dn);
					}
				}
				catch
				{
					// Error
					Debug.LogError("Could not access folder: \"" + dir + "\"");
				}
			}
			
			return components;
		}
	}
}