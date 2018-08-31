using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Display : MonoBehaviour {
	public Shader shader;
	// Use this for initialization
	void Start () {
		Map map = new Map();
		bool comp = false;
		while (!comp) {
			map = new Map();
			comp = map.Generate(12, 4);
		}
		GameObject mapOBJ = new GameObject("Map");
		Mesh mesh = map.mapMesher();
		MeshFilter mf = mapOBJ.AddComponent<MeshFilter>();
		MeshRenderer mr = mapOBJ.AddComponent<MeshRenderer>();
		mr.material = new Material(shader);
		mf.mesh = mesh;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
