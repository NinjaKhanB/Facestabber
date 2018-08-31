using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge{
	public Vector2 p0;
	public Vector2 p1;
	public bool river = false;
	public Tile neighbor0;
	public Tile neighbor1;
	public int neighborNumber0;//what edge index this edge is in each neighbor tile
	public int neighborNumber1;
	public Edge(Vector2 p0, Vector2 p1) {
		this.p0 = p0;
		this.p1 = p1;
	}
	public Tile otherNeighbor(Tile self) {
		if (self == neighbor0) {
			return neighbor1;
		}
		if (self == neighbor1) {
			return neighbor0;
		}
		Debug.Log("ERROR: NOT ON EDGE");
		return null;
	}
	public void kill() {
		//Move Shared Points
		Vector2 newPoint = (p0 + p1) / 2f;
		Vector2[] tempPoints = new Vector2[] { p0, p1 };
		Tile[] tempNeighbors = new Tile[] { neighbor0, neighbor1 };
		int[] tempNeighborNumbers = new int[] { neighborNumber0, neighborNumber1 };
		for (int l = 0; l < tempPoints.Length; l++) {
			Vector2 point = tempPoints[l];
			for (int m = 0; m < tempNeighbors.Length; m++) {
				Tile neighbor = tempNeighbors[m];
				if (neighbor != null) {
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
						if (VectorC.equals(neighbor.edges[toCheck[n]].p0, point)) {
							neighbor.edges[toCheck[n]].p0 = newPoint;
						}
						if (VectorC.equals(neighbor.edges[toCheck[n]].p1, point)) {
							neighbor.edges[toCheck[n]].p1 = newPoint;
						}
					}
				}

			}
		}
		//Delete Edge
		for (int l = 0; l < tempNeighbors.Length; l++) {
			Tile neighbor = tempNeighbors[l];
			if (neighbor != null) {
				int neighborNumber = tempNeighborNumbers[l];
				Edge[] newEdges = new Edge[neighbor.edges.Length - 1];
				for (int m = 0; m < neighborNumber; m++) {
					newEdges[m] = neighbor.edges[m];
				}
				for (int m = neighborNumber + 1; m < neighbor.edges.Length; m++) {
					newEdges[m - 1] = neighbor.edges[m];
					if (neighbor.edges[m].neighbor0 == neighbor) {
						neighbor.edges[m].neighborNumber0 -= 1;
					} else {
						neighbor.edges[m].neighborNumber1 -= 1;
					}
				}
				neighbor.edges = newEdges;
			}
		}
		//Delete Neighbors
		for (int l = 0; l < tempNeighbors.Length; l++) {
			Tile neighbor = tempNeighbors[l];
			if (neighbor != null) {
				Tile otherNeighbor = tempNeighbors[-l + 1];
				int neighborNeighborNumber = 0;
				for (int m = 0; m < neighbor.neighbors.Length; m++) {
					if (neighbor.neighbors[m] == otherNeighbor) {
						neighborNeighborNumber = m;
					}
				}
				Tile[] newNeighbors = new Tile[neighbor.neighbors.Length - 1];
				for (int m = 0; m < neighborNeighborNumber; m++) {
					newNeighbors[m] = neighbor.neighbors[m];
				}
				for (int m = neighborNeighborNumber + 1; m < neighbor.neighbors.Length; m++) {
					newNeighbors[m - 1] = neighbor.neighbors[m];
				}
				neighbor.neighbors = newNeighbors;
			}
		}
	}
	
}
