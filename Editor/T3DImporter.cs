using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace T3DImporter {
	public static class T3DImporter {

		[MenuItem("File/Import/.t3d", false, 10)]
		static void Import() {
			string filePath = EditorUtility.OpenFilePanel("Select t3d", Application.dataPath, "t3d");
			if (string.IsNullOrEmpty(filePath)) {
				Debug.LogWarning("No file selected");
				return;
			} else {
				Map map = ImportMap(filePath);
				if (map == null) {
					Debug.LogWarning("Map contents couldn't be read");
					return;
				}
				Debug.Log(map);

				Transform root = new GameObject("Imported T3D map").transform;
				EditorCoroutineRunner.StartCoroutineWithUI(map.Spawn(root, new Vector3(0.01f,0.01f,0.01f)), "Importing", true);
			}
		}

		public static Map ImportMap(string filePath) {
			using(StreamReader sr = new StreamReader(filePath)) {
				string line;
				while ((line = sr.ReadLine()) != null) {
					line = line.Trim();
					if (line.StartsWith("Begin Map")) return new Map(sr);
				}
			}
			return null;
		}

		public static string GetParameter(this string line, string parameter) {
			return line.Split(new string[] { parameter }, StringSplitOptions.RemoveEmptyEntries) [1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) [0];
		}
	}
}