using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace T3DImporter {
	[Serializable]
	public class Polygon {
		public Vector3 normal;
		public Vector3 origin;
		public List<Vector3> vectors = new List<Vector3>();

		/// <summary> Constructor </summary>
		public Polygon(StreamReader sr, Vector3 pivot) {
			string line;
			while ((line = sr.ReadLine()) != null) {
				line = line.Trim();
				if (line.StartsWith("Vertex")) {
					string[] parts = line.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
					Vector3 vec = Vector3.zero;
					bool success = true;
					success = success && float.TryParse(parts[1], out vec.x);
					success = success && float.TryParse(parts[2], out vec.y);
					success = success && float.TryParse(parts[3], out vec.z);
					if (!success) Debug.LogWarning("Error parsing vector\n" + line);
					vec = vec.ConvertSwizzle();
					vectors.Add(vec);
				}
				if (line == "End Polygon") return;
			}
		}
	}
}