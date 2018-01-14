using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace T3DImporter {
	public static class MeshUtils {
		public static void AddTri(this Mesh mesh, Vector3 a, Vector3 b, Vector3 c) {
			Vector3[] oldVerts = mesh.vertices;
			int[] oldTris = mesh.GetTriangles(0);
			Vector3[] oldNorms = mesh.normals;
			Vector2[] oldUvs = mesh.uv;
			Color[] oldColors = mesh.colors;

			Vector3 norm = Vector3.Cross(b-a, c-a).normalized;

			Vector3[] newVerts = new Vector3[] { a, b, c };
			Vector3[] newNorms = new Vector3[] { norm, norm, norm };
			Vector2[] newUvs = new Vector2[] { Vector3.zero, Vector3.zero, Vector3.zero };
			Color[] newColors = new Color[] { Color.white, Color.white, Color.white };
			int[] newTris = new int[] {
				oldVerts.Length + 0,
					oldVerts.Length + 1,
					oldVerts.Length + 2,
			};

			int[] tris = oldTris.Concat(newTris).ToArray();
			List<Vector3> verts = oldVerts.Concat(newVerts).ToList();
			List<Vector3> norms = oldNorms.Concat(newNorms).ToList();
			List<Vector2> uv = oldUvs.Concat(newUvs).ToList();
			List<Color> cols = oldColors.Concat(newColors).ToList();
			mesh.SetVertices(verts);
			mesh.SetColors(cols);
			mesh.SetNormals(norms);
			mesh.SetUVs(0, uv);
			mesh.SetTriangles(tris, 0);
		}

		public static void AddQuad(this Mesh mesh, Vector3 a, Vector3 b, Vector3 c, Vector3 d) {
			Vector3[] oldVerts = mesh.vertices;
			int[] oldTris = mesh.GetTriangles(0);
			Vector3[] oldNorms = mesh.normals;
			Vector2[] oldUvs = mesh.uv;
			Color[] oldColors = mesh.colors;

			Vector3 norm = Vector3.Cross(b-a, c-a).normalized;

			Vector3[] newVerts = new Vector3[] { a, b, c, d };
			Vector3[] newNorms = new Vector3[] { norm, norm, norm, norm };
			Vector2[] newUvs = new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
			Color[] newColors = new Color[] { Color.white, Color.white, Color.white, Color.white };
			int[] newTris = new int[] {
				oldVerts.Length + 0,
					oldVerts.Length + 1,
					oldVerts.Length + 2,
					oldVerts.Length + 2,
					oldVerts.Length + 3,
					oldVerts.Length + 0,
			};

			int[] tris = oldTris.Concat(newTris).ToArray();
			List<Vector3> verts = oldVerts.Concat(newVerts).ToList();
			List<Vector3> norms = oldNorms.Concat(newNorms).ToList();
			List<Vector2> uv = oldUvs.Concat(newUvs).ToList();
			List<Color> cols = oldColors.Concat(newColors).ToList();
			mesh.SetVertices(verts);
			mesh.SetColors(cols);
			mesh.SetNormals(norms);
			mesh.SetUVs(0, uv);
			mesh.SetTriangles(tris, 0);
		}

		public static Vector3 ConvertSwizzle(this Vector3 vec) {
			return new Vector3(vec.x,vec.z,-vec.y);
		}
		/*public static void AddPolygon(this Mesh mesh, List<Vector3> points) {
			Triangulator t = new Triangulator(points);
			int[] newTris = t.Triangulate();
			int[] oldTris = mesh.GetTriangles(0);
			int[] tris = oldTris.Concat(newTris).ToArray();
			Vector3[] newVerts = points.ToArray();
			Vector3[] oldVerts = mesh.vertices;
			Vector3[] verts = oldVerts.Concat(newVerts).ToArray();

		}

		public class Triangulator {
			private List<Vector2> m_points = new List<Vector2>();

			public Triangulator(List<Vector2> points) {
				m_points = points;
			}

			public int[] Triangulate() {
				List<int> indices = new List<int>();

				int n = m_points.Count;
				if (n < 3)
					return indices.ToArray();

				int[] V = new int[n];
				if (Area() > 0) {
					for (int v = 0; v < n; v++)
						V[v] = v;
				} else {
					for (int v = 0; v < n; v++)
						V[v] = (n - 1) - v;
				}

				int nv = n;
				int count = 2 * nv;
				for (int m = 0, v = nv - 1; nv > 2;) {
					if ((count--) <= 0)
						return indices.ToArray();

					int u = v;
					if (nv <= u)
						u = 0;
					v = u + 1;
					if (nv <= v)
						v = 0;
					int w = v + 1;
					if (nv <= w)
						w = 0;

					if (Snip(u, v, w, nv, V)) {
						int a, b, c, s, t;
						a = V[u];
						b = V[v];
						c = V[w];
						indices.Add(a);
						indices.Add(b);
						indices.Add(c);
						m++;
						for (s = v, t = v + 1; t < nv; s++, t++)
							V[s] = V[t];
						nv--;
						count = 2 * nv;
					}
				}

				indices.Reverse();
				return indices.ToArray();
			}

			private float Area() {
				int n = m_points.Count;
				float A = 0.0f;
				for (int p = n - 1, q = 0; q < n; p = q++) {
					Vector2 pval = m_points[p];
					Vector2 qval = m_points[q];
					A += pval.x * qval.y - qval.x * pval.y;
				}
				return (A * 0.5f);
			}

			private bool Snip(int u, int v, int w, int n, int[] V) {
				int p;
				Vector2 A = m_points[V[u]];
				Vector2 B = m_points[V[v]];
				Vector2 C = m_points[V[w]];
				if (Mathf.Epsilon >(((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
					return false;
				for (p = 0; p < n; p++) {
					if ((p == u) || (p == v) || (p == w))
						continue;
					Vector2 P = m_points[V[p]];
					if (InsideTriangle(A, B, C, P))
						return false;
				}
				return true;
			}

			private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
				float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
				float cCROSSap, bCROSScp, aCROSSbp;

				ax = C.x - B.x;
				ay = C.y - B.y;
				bx = A.x - C.x;
				by = A.y - C.y;
				cx = B.x - A.x;
				cy = B.y - A.y;
				apx = P.x - A.x;
				apy = P.y - A.y;
				bpx = P.x - B.x;
				bpy = P.y - B.y;
				cpx = P.x - C.x;
				cpy = P.y - C.y;

				aCROSSbp = ax * bpy - ay * bpx;
				cCROSSap = cx * apy - cy * apx;
				bCROSScp = bx * cpy - by * cpx;

				return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
			}
		}*/
	}
}