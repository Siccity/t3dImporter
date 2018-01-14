using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace T3DImporter {
	[Serializable]
	public class Brush : Actor {
		public string brushName;
		public bool additive;
		public List<Polygon> polyList = new List<Polygon>();

		/// <summary> Constructor </summary>
		public Brush(string line, StreamReader sr) : base(line, sr) { }

		public override void ParseLine(string line, StreamReader sr) {
			base.ParseLine(line, sr);
			if (line.StartsWith("Begin Polygon")) polyList.Add(new Polygon(sr, prePivot));
			else if (line == "CsgOper=CSG_Subtract") additive = false;
			else if (line == "CsgOper=CSG_Add") additive = true;
			else if (line == "Brush=") brushName = line.Split('=')[1];
			else if (line == "End Brush") return;
		}

		public Mesh ToMesh(Vector3 scale) {
			Mesh mesh = new Mesh();
			foreach (Polygon poly in polyList) {
				if (poly.vectors.Count == 4) {
					mesh.AddQuad(Vector3.Scale(poly.vectors[0] - prePivot, scale), Vector3.Scale(poly.vectors[1] - prePivot, scale), Vector3.Scale(poly.vectors[2] - prePivot, scale), Vector3.Scale(poly.vectors[3] - prePivot, scale));
				} else if (poly.vectors.Count == 3) {
					mesh.AddTri(Vector3.Scale(poly.vectors[0] - prePivot, scale), Vector3.Scale(poly.vectors[1] - prePivot, scale), Vector3.Scale(poly.vectors[2] - prePivot, scale));
				} else {
					Debug.LogWarning("Unsupported polygon skipped (" + poly.vectors.Count + "verts)");
				}
			}
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
			return mesh;
		}

		public override GameObject Spawn(Transform parent, Vector3 scale) {
			GameObject go = base.Spawn(parent, scale);

			go.name += additive ? "(add)" : "(sub)";
			go.AddComponent<MeshFilter>().mesh = ToMesh(scale);
			go.AddComponent<MeshRenderer>().material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
			return go;
		}
	}
}