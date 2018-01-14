using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace T3DImporter {
	[Serializable]
	public abstract class Actor {
		public string name;
		public Vector3 position;
		public Vector3 rotation;

		public Vector3 oldPosition;
		public Vector3 prePivot;

		//public Vector3 mainScale; // scales current brush before rotation.
		//public Vector3 translation;
		//public Vector3 rotation;

		public Vector3 postScale = Vector3.one;

		/// <summary> Constructor </summary>
		public Actor(string line, StreamReader sr) {
			string[] parms = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string parm in parms) {
				if (parm.StartsWith("Name")) name = parm.Split('=') [1];
			}
			while ((line = sr.ReadLine()) != null) {
				line = line.Trim();
				if (line == "End Actor") return;
				else ParseLine(line.Trim(), sr);
			}
		}

		public virtual void ParseLine(string line, StreamReader sr) {
			if (line.StartsWith("PrePivot")) prePivot = ParseLocation(line);
			else if (line.StartsWith("Rotation")) rotation = ParseRotation(line);
			else if (line.StartsWith("Location")) position = ParseLocation(line);
			else if (line.StartsWith("OldLocation")) oldPosition = ParseLocation(line);
			else if (line.StartsWith("PostScale")) postScale = ParsePostScale(line);
		}

		public Vector3 ParseLocation(string line) {
			line = line.Split('(', ')') [1];
			string[] xyz = line.Split(',');
			Vector3 vec = Vector3.zero;
			for (int i = 0; i < xyz.Length; i++) {
				if (xyz[i].StartsWith("X=")) float.TryParse(xyz[i].Split('=') [1], out vec.x);
				if (xyz[i].StartsWith("Y=")) float.TryParse(xyz[i].Split('=') [1], out vec.y);
				if (xyz[i].StartsWith("Z=")) float.TryParse(xyz[i].Split('=') [1], out vec.z);
			}
			vec = vec.ConvertSwizzle();
			return vec;
		}

		public Vector3 ParsePostScale(string line) {
			line = line.Split(new string[] {"Scale=(", ")"}, StringSplitOptions.RemoveEmptyEntries) [1];
			string[] xyz = line.Split(',');
			Vector3 vec = Vector3.one;
			for (int i = 0; i < xyz.Length; i++) {
				if (xyz[i].StartsWith("X=")) float.TryParse(xyz[i].Split('=') [1], out vec.x);
				if (xyz[i].StartsWith("Y=")) float.TryParse(xyz[i].Split('=') [1], out vec.y);
				if (xyz[i].StartsWith("Z=")) float.TryParse(xyz[i].Split('=') [1], out vec.z);
			}
			vec = vec.ConvertSwizzle();
			vec.z = -vec.z;
			return vec;
		}

		public Vector3 ParseRotation(string line) {
			line = line.Split('(', ')') [1];
			string[] xyz = line.Split(',');
			Vector3 vec = Vector3.zero;
			for (int i = 0; i < xyz.Length; i++) {
				if (xyz[i].StartsWith("Pitch=")) float.TryParse(xyz[i].Split('=') [1], out vec.x);
				if (xyz[i].StartsWith("Yaw=")) float.TryParse(xyz[i].Split('=') [1], out vec.y);
				if (xyz[i].StartsWith("Roll=")) float.TryParse(xyz[i].Split('=') [1], out vec.z);
			}
			vec.x = (vec.x / 65536f) * 360f;
			vec.y = (vec.y / 65536f) * 360f;
			vec.z = (vec.z / 65536f) * 360f;
			vec = new Vector3(vec.z, vec.y, vec.x);
			return vec;
		}

		public virtual GameObject Spawn(Transform parent, Vector3 scale) {
			GameObject go = new GameObject(name);
			go.transform.parent = parent;
			position.Scale(scale);

			//prePivot = Quaternion.Euler(rotation) * prePivot;
			//prePivot.Scale(postScale);
			go.transform.position = position;
			postScale = Quaternion.Euler(rotation) * postScale;
			postScale.x = Mathf.Abs(postScale.x);
			postScale.y = Mathf.Abs(postScale.y);
			postScale.z = Mathf.Abs(postScale.z);
			go.transform.rotation = Quaternion.Euler(rotation);
			go.transform.localScale = postScale;
			return go;
		}
	}
}