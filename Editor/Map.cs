using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace T3DImporter {
	[Serializable]
	public class Map {
		public List<Actor> actors = new List<Actor>();

		/// <summary> Constructor </summary>
		public Map(StreamReader sr) {
			string line;
			while ((line = sr.ReadLine()) != null) {
				line = line.Trim();
				if (line.StartsWith("Begin Actor")) {
					string type = line.GetParameter("Class=");
					if (type == "Brush") actors.Add(new Brush(line, sr));
					else if (type == "Light") actors.Add(new Light(line, sr));
					else actors.Add(new PlaceHolder(line, sr));
				}
				if (line == "End Map") return;
			}
		}

		public IEnumerator Spawn(Transform parent, Vector3 scale) {
			//List<GameObject> brushes = new List<GameObject>();
			List<CSGObject> adds = new List<CSGObject>();
			List<Bounds> bounds = new List<Bounds>();
			int progress = 0;
			foreach (Actor actor in actors) {
				progress++;
				EditorCoroutineRunner.UpdateUIProgressBar(progress / (float) actors.Count);
				yield return new WaitForSeconds(0.05f);
				GameObject go = actor.Spawn(parent, scale);
				if (actor is Brush) {
					Brush brush = actor as Brush;
					CSGObject csg = go.AddComponent<CSGObject>();
					Bounds b = go.GetComponent<MeshRenderer>().bounds;
					if (brush.additive) {
						adds.Add(csg);
						bounds.Add(b);
					} else {
						for (int i = 0; i < adds.Count; i++) {
							if (b.Intersects(bounds[i])) {
								adds[i].PerformCSG(CsgOperation.ECsgOperation.CsgOper_Subtractive, new GameObject[] { go });
							}
						}
						GameObject.DestroyImmediate(go);
					}
				}
			}
		}

		public override string ToString() {
			return "Map(" + actors.Count + " actors, " + actors.Where(x => x is Brush).Count() + " brushes)";
		}
	}
}