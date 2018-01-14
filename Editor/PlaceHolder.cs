using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace T3DImporter {
	public class PlaceHolder : Actor {
		public string type;
		public enum LabelIcon {
			Gray = 0,
			Blue,
			Teal,
			Green,
			Yellow,
			Orange,
			Red,
			Purple
		}

		public PlaceHolder(string line, StreamReader sr) : base(line, sr) {
			type = line.GetParameter("Class=");
		}

		public override GameObject Spawn(Transform parent, Vector3 scale) {
			GameObject go = base.Spawn(parent, scale);
			PlaceHolderEntity phe = go.AddComponent<PlaceHolderEntity>();
			phe.type = type;
			switch (type) {
				case "PathNode":
					phe.color = new Color(1, 1, 0, 0.5f);
					phe.size = new Vector3(1, 0.2f, 1);
					break;
				case "AmbientSound":
					GameObject.DestroyImmediate(phe);
					go.AddComponent<UnityEngine.AudioSource>();
					break;
				case "PatrolPoint":
					phe.color = new Color(0, 0, 1, 0.5f);
					phe.size = new Vector3(0.2f, 1, 0.2f);
					break;
				default:
					phe.color = new Color(0, 1, 0, 0.5f);
					break;
			}
			return go;
		}
	}
}