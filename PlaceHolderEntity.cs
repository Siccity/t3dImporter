using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceHolderEntity : MonoBehaviour {

	public string type;
	public Color color;
	public Vector3 size = Vector3.one;

	void OnDrawGizmos() {
		Gizmos.color = color;
		Gizmos.DrawCube(transform.position, size);
	}
}
