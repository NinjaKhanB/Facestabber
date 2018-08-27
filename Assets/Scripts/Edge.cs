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
		//Move Shared Points
		Vector2 newPoint = p0 + p1 / 2;
		Vector2[] tempPoints = new Vector2[] { p0, p1 };
		Tile[] tempNeighbors = new Tile[] { neighbor0, neighbor1 };
		int[] tempNeighborNumbers = new int[] { neighborNumber0, neighborNumber1 };
		for (int l = 0; l < tempPoints.Length; l++) {
			Vector2 point = tempPoints[l];
			for (int m = 0; m < tempNeighbors.Length; m++) {
				Tile neighbor = tempNeighbors[m];
				int neighborNumber = tempNeighborNumbers[m];

				int[] toCheck = new int[2];
				if (neighborNumber > 0 && neighborNumber < neighbor.edges.Length - 1) {
					toCheck[0] = neighborNumber - 1;
					toCheck[1] = neighborNumber + 1;
				}
				if (neighborNumber == 0) {
					toCheck[0] = neighbor.edges.Length - 1;
					toCheck[1] = 1;
				}
				if (neighborNumber == neighbor.edges.Length - 1) {
					toCheck[0] = neighborNumber - 1;
					toCheck[1] = 0;
				}
				for (int n = 0; n < toCheck.Length; n++) {
					if (neighbor.edges[toCheck[n]].p0.Equals(point)) {
						neighbor.edges[toCheck[n]].p0 = newPoint;
					}
					if (neighbor.edges[toCheck[n]].p1.Equals(point)) {
						neighbor.edges[toCheck[n]].p1 = newPoint;
					}
				}

			}
		}
		//Delete Edge
		for (int l = 0; l < tempNeighbors.Length; l++) {
			Tile neighbor = tempNeighbors[l];
			int neighborNumber = tempNeighborNumbers[l];
			Edge[] newEdges = new Edge[neighbor.edges.Length-1];
			for (int m = 0; m < neighborNumber; m++) {
				newEdges[m] = neighbor.edges[m];
			}
			for (int m = neighborNumber + 1; m < neighbor.edges.Length; m++) {
				newEdges[m-1] = neighbor.edges[m];
			}
			neighbor.edges = newEdges;
		}
	}
	
}
