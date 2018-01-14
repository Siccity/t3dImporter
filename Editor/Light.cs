using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace T3DImporter {
	[Serializable]
	public class Light : Actor {

		public float brightness;
		public float hue;
		public float saturation;
		public float radius;

		public Light(string line, StreamReader sr) : base(line, sr) { }

		public override void ParseLine(string line, StreamReader sr) {
			base.ParseLine(line, sr);
			if (line.StartsWith("LightBrightness")) float.TryParse(line.Split('=') [1], out brightness);
			else if (line.StartsWith("LightHue")) float.TryParse(line.Split('=') [1], out hue);
			else if (line.StartsWith("LightSaturation")) float.TryParse(line.Split('=') [1], out saturation);
			else if (line.StartsWith("LightRadius")) float.TryParse(line.Split('=') [1], out radius);
		}

		public override GameObject Spawn(Transform parent, Vector3 scale) {
			GameObject go = base.Spawn(parent, scale);
			UnityEngine.Light light = go.AddComponent<UnityEngine.Light>();
			light.color = Color.HSVToRGB(hue/255f, saturation/255f, brightness/255f);
			light.range = radius;
			return go;
		}
	}
}