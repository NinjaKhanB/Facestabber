using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge{
	public Vector2 p0;
	public Vector2 p1;
	public bool river;
	public Tile neighbor0;
	public Tile neighbor1;
	public int neighborNumber0;//what edge index this edge is in each neighbor tile
	public int neighborNumber1;
	public Edge(Vector2 p0, Vector2 p1) {
		this.p0 = p0;
		this.p1 = p1;
	}
	public void kill() {

	}
}
