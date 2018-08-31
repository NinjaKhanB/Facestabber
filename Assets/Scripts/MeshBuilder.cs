using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder : MonoBehaviour {
	public static Mesh tileMaker(Edge[] edges, float dropIn, Color color) {
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[edges.Length];
		Vector2 lastPoint = new Vector2();
		if (VectorC.equals(edges[0].p0,edges[1].p0) || VectorC.equals(edges[0].p0,edges[1].p1)) {
			lastPoint = edges[0].p1;
		} else {
			lastPoint = edges[0].p0;
		}
		for (int l = 0; l < edges.Length; l++) {
			Edge edge = edges[l];
			if (VectorC.equals(edge.p0,lastPoint)) {
				Vector2 point = edge.p1;
				vertices[l] = point;
				lastPoint = point;
			} else{
				Vector2 point = edge.p0;
				vertices[l] = point;
				lastPoint = point;
			}

		}
		//Vert Drop
		Vector3[] newVertices = new Vector3[vertices.Length];
		for (int l = 0; l < vertices.Length; l++) {
			int[] nextTo = new int[2];
			if (l > 0 && l < vertices.Length - 1) {
				nextTo[0] = l - 1;
				nextTo[1] = l + 1;
			}
			if (l == 0) {
				nextTo[0] = vertices.Length - 1;
				nextTo[1] = 1;
			}
			if (l == vertices.Length - 1) {
				nextTo[0] = l - 1;
				nextTo[1] = 0;
			}
			if (edges[l].river) {
				dropIn *= 2;
			}
			Vector2 vec0 = (vertices[nextTo[0]] - new Vector3(vertices[l].x, vertices[l].y)).normalized;
			Vector2 vec1 = (vertices[nextTo[1]] - new Vector3(vertices[l].x, vertices[l].y)).normalized;
			Vector2 directionalZeroedVec = Vector2.Lerp(vec0, vec1, 0.5f);
			directionalZeroedVec *= dropIn / Vector2.Distance(Vector2.zero, directionalZeroedVec);
			newVertices[l] = directionalZeroedVec + new Vector2(vertices[l].x, vertices[l].y);

		}
		vertices = newVertices;
		int[] triangles = new int[3 * (vertices.Length - 2)];
		for (int l = 0; l < vertices.Length - 2; l++) {
			triangles[l * 3] = 0;
			triangles[l * 3 + 2] = l + 1;
			triangles[l * 3 + 1] = l + 2;
		}
		Color[] colors = colorSetter(color, vertices);
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;
		return mesh;
	}
	public static Mesh rectangle(float width, float height, Color color) {
		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[] {
			new Vector3(0,0,0.01f),
			new Vector3(0,height,0.01f),
			new Vector3(width,0,0.01f),
			new Vector3(width,height,0.01f)
		};
		int[] triangles = new int[] {
			1,2,0,
			3,2,1
		};
		Color[] colors = colorSetter(color, vertices);
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;
		return mesh;
	}
	public static Color[] colorSetter(Color color, Vector3[] vertices) {
		Color[] colors = new Color[vertices.Length];
		for (int l = 0; l < colors.Length; l++) {
			colors[l] = color;
		}
		return colors;
	}
}
