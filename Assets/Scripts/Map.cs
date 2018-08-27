using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

public class Map{
	int playerNumber;
	float width = 1;
	float height = 1;
    Tile[] tiles;
    Tile[] villages;
    Tile[] towns;
    Tile[] cities;
	public Map() {

	}
    public void Generate(int playerNumber,int regularity){
		float smallestTile = 0.001f;

        int villageCount = 5 * playerNumber;
        int townCount =  2 * playerNumber;
        int cityCount = (int)Mathf.Ceil(playerNumber / 3);

        int tileNum= playerNumber * 45;
		List <Vector2f> pointPos = new List<Vector2f>(); //Making random Voronoi Sites
        for (int l = 0; l < tileNum; l++){
			float x = Random.Range(0f, width);
			float y = Random.Range(0f, height);
			pointPos.Add(new Vector2f(x, y));
        }
		Rectf bounds = new Rectf(0, 0, width, height);
		Voronoi mapVoronoi = new Voronoi(pointPos, bounds, regularity); //Computing Voronoi
		List<Vector2f> siteCords = mapVoronoi.SiteCoords();
		tiles = new Tile[siteCords.Count];
		//Making Tiles
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = new Tile();
			//Edges
			List<LineSegment> curEdges = mapVoronoi.VoronoiBoundarayForSite(siteCords[l]);//Getting Edge Points
			Edge[] edges = new Edge[curEdges.Count];
			for (int m = 0; m < curEdges.Count; m++) {
				Edge edge = new Edge(V2f(curEdges[m].p0), V2f(curEdges[m].p1)); //Making an edge
				edge.neighbor0 = tile;
				edges[m] = edge;
			}
			tile.edges = edges;
			tile.sitePoint = V2f(siteCords[l]);
			List<Vector2f> neighborSites = mapVoronoi.NeighborSitesForSite(V2f(tile.sitePoint));//getting Neighbor sites
			tile.neighbors = new Tile[neighborSites.Count];
			for (int m = 0; m < neighborSites.Count; m++) { //setting unchecked neighbor sites
				tile.uncheckedNeighborSites.Add(V2f(neighborSites[m]));
			}

			tiles[l] = tile; //Set into main array
		}
		//Finding Neighbors
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			for (int m = 0; m < tile.uncheckedNeighborSites.Count; m++) {
				bool found = false;
				for (int n = l + 1; n < tiles.Length; n++) {
					if (tile.uncheckedNeighborSites[m] == tiles[n].sitePoint) { //Is it a match
						Tile newTile = tiles[n];
						tile.tempNeighbors.Add(newTile); //Exchange neighbor status
						newTile.tempNeighbors.Add(tile);
						found = true;
						for (int o = 0; o < newTile.uncheckedNeighborSites.Count; o++) { //Getting rid of other's unknown neighbor site
							if (tile.sitePoint == newTile.uncheckedNeighborSites[o]) {
								newTile.uncheckedNeighborSites.RemoveAt(o);
								o = newTile.uncheckedNeighborSites.Count + 1000000; //stopping the search
							}
						}
						n = tiles.Length + 1000000; //stopping the search
					}
				}
				if (!found) {
					Debug.Log("ERROR: DID NOT FIND NEIGHBOR");
				}
			}
			tile.neighbors = tile.tempNeighbors.ToArray();
			tile.tempNeighbors = null;
			tile.uncheckedNeighborSites = null;
		}
		//Del Duplicate Edges
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			for (int m = 0; m < tile.neighbors.Length; m++) {
				Tile otherTile = tile.neighbors[m];
				for (int n = 0; n < tile.edges.Length; n++) {
					for (int o = 0; o < otherTile.edges.Length; o++) {
						bool sameEdge = false;
						if (tile.edges[n].p0.Equals(otherTile.edges[o].p0) && tile.edges[n].p1.Equals(otherTile.edges[o].p1)) {
							sameEdge = true;
						}
						if (tile.edges[n].p0.Equals(otherTile.edges[o].p1) && tile.edges[n].p1.Equals(otherTile.edges[o].p0)) {
							sameEdge = true;
						}

						if (sameEdge) {
							tile.edges[n] = otherTile.edges[o]; //Take the other's tile as your own
							tile.edges[n].neighbor0 = otherTile;
							tile.edges[n].neighbor1 = tile;
						}

					}
				}
			}
		}
		//Order Edges and Neighbors according to the circle
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			//Edges
			Edge[] oldEdges = tile.edges;
			Edge[] orderedEdges = new Edge[oldEdges.Length];
			int lastFound = 0; //Last matching point pos within the edge
			for (int n = 1; n < orderedEdges.Length; n++) {
				Vector2 findPoint;
				if (lastFound == 0) {
					findPoint = orderedEdges[n - 1].p1;
				} else {
					findPoint = orderedEdges[n - 1].p0;
				}
				for (int o = 1; o < oldEdges.Length; o++) {
					if (oldEdges[o] != null) {
						Edge oldEdge = oldEdges[o];
						bool found = false;
						if (oldEdge.p0.Equals(findPoint)) {
							found = true;
							lastFound = 0;
						}
						if (oldEdge.p1.Equals(findPoint)) {
							found = true;
							lastFound = 1;
						}
						if (found) {
							orderedEdges[n] = oldEdge;
							oldEdges[o] = null;
						}
					}
				}
			}
			//Neighbors
			Tile[] orderedNeighbors = new Tile[tile.neighbors.Length];
			for (int n = 0; n < orderedNeighbors.Length; n++) {
				Edge edge = orderedEdges[n];
				if (edge.neighbor0 != tile) {
					orderedNeighbors[n] = edge.neighbor0;
					edge.neighborNumber1 = n;
				} else {
					orderedNeighbors[n] = edge.neighbor1;
					edge.neighborNumber0 = n;
				}
			}
			tile.edges = orderedEdges; //Setting Proper
			tile.neighbors = orderedNeighbors;
		}
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			for (int m = 0; m < tile.edges.Length; m++) {
				Edge edge = tile.edges[m];
				if (Vector2.Distance(edge.p0,edge.p1) < smallestTile) {
					edge.kill();
				}
			}
		}
	}

	//Funcitions
	private Vector2 V2f(Vector2f vect) {
		return new Vector2(vect.x, vect.y);
	}
	private Vector2f V2f(Vector2 vect) {
		return new Vector2f(vect.x, vect.y);
	}
}
