using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

public class Map{
	public int playerNumber;
	public float width = 1.4f;
	public float height = 1;
	public Tile[] tiles;
	public Tile[] villages;
	public Tile[] towns;
	public Tile[] cities;
	public Map() {

	}
	public bool Generate(int playerNumber, int regularity) {
		float smallestTile = 0.001f;

		int villageCount = 5 * playerNumber;
		int townCount = 2 * playerNumber;
		int cityCount = (int)Mathf.Ceil(playerNumber / 3f);

		int tileNum = playerNumber * 150;
		List<Vector2f> pointPos = new List<Vector2f>(); //Making random Voronoi Sites
		for (int l = 0; l < tileNum; l++) {
			float x = UnityEngine.Random.Range(0f, width);
			float y = UnityEngine.Random.Range(0f, height);
			pointPos.Add(new Vector2f(x, y));
		}
		Rectf bounds = new Rectf(0, 0, width, height);
		Voronoi mapVoronoi = new Voronoi(pointPos, bounds, regularity); //Computing Voronoi
		List<Vector2f> siteCords = mapVoronoi.SiteCoords();
		tiles = new Tile[siteCords.Count];
		List<Vector2[]> pointBlacklist = new List<Vector2[]>();
		//Making Tiles
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = new Tile(l);
			//Edges
			List<LineSegment> curEdges = mapVoronoi.VoronoiBoundarayForSite(siteCords[l]);//Getting Edge Points

			List <Edge> edges = new List<Edge>();
			List<Vector2> hZeros = new List<Vector2>();
			List<Vector2> vZeros = new List<Vector2>();
			for (int m = 0; m < curEdges.Count; m++) {
				Vector2 p0 = V2f(curEdges[m].p0);
				Vector2 p1 = V2f(curEdges[m].p1);
				float close = 0.0001f;
				if (Mathf.Abs(p0.x) <= close || Mathf.Abs(width - p0.x) <= close) {
					hZeros.Add(p0);
				}
				if (Mathf.Abs(p1.x) <= close || Mathf.Abs(width - p1.x) <= close) {
					hZeros.Add(p1);
				}
				if (Mathf.Abs(p0.y) <= close || Mathf.Abs(height - p0.y) <= close) {
					vZeros.Add(p0);
				}
				if (Mathf.Abs(p1.y) <= close || Mathf.Abs(height - p1.y) <= close) {
					vZeros.Add(p1);
				}
				Edge edge = new Edge(p0, p1); //Making an edge
				edge.neighbor0 = tile;
				edges.Add(edge);
			}
			if (hZeros.Count >= 2) {
				Edge edge = new Edge(hZeros[0], hZeros[1]); //Making an edge
				edge.neighbor0 = tile;
				edges.Add(edge);
			}
			if (vZeros.Count >= 2) {
				Edge edge = new Edge(vZeros[0], vZeros[1]); //Making an edge
				edge.neighbor0 = tile;
				edges.Add(edge);
			}
			if (hZeros.Count >= 1 && vZeros.Count >= 1) {
				Edge edge = new Edge(hZeros[0], new Vector2(hZeros[0].x, vZeros[0].y)); //Making an edge
				edge.neighbor0 = tile;
				edges.Add(edge);
				Edge edge2 = new Edge(vZeros[0], new Vector2(hZeros[0].x, vZeros[0].y)); //Making an edge
				edge2.neighbor0 = tile;
				edges.Add(edge2);
			}
			tile.edges = edges.ToArray();
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
					return false;
				}
			}
			for (int m = 0; m < tile.tempNeighbors.Count; m++) {
				if (tile.tempNeighbors[m] == null) {
					tile.tempNeighbors.Remove(tile.tempNeighbors[m]);
				}
			}
			tile.neighbors = tile.tempNeighbors.ToArray();
			tile.tempNeighbors = null;
			tile.uncheckedNeighborSites = null;
		}
		//Del Duplicate Edges
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			List<Tile> fixedNeighbors = new List<Tile>();
			fixedNeighbors.AddRange(tile.neighbors);
			for (int m = 0; m < tile.neighbors.Length; m++) {
				Tile otherTile = tile.neighbors[m];
				bool found = false;
				for (int n = 0; n < tile.edges.Length; n++) {
					for (int o = 0; o < otherTile.edges.Length; o++) {
						bool sameEdge = false;
						if (VectorC.equals(tile.edges[n].p0,otherTile.edges[o].p0) && VectorC.equals(tile.edges[n].p1,otherTile.edges[o].p1)) {
							sameEdge = true;
						}
						if (VectorC.equals(tile.edges[n].p0,otherTile.edges[o].p1) && VectorC.equals(tile.edges[n].p1,otherTile.edges[o].p0)) {
							sameEdge = true;
						}

						if (sameEdge) {
							found = true;
							tile.edges[n] = otherTile.edges[o]; //Take the other's tile as your own
							tile.edges[n].neighbor0 = otherTile;
							tile.edges[n].neighbor1 = tile;
						}

					}
				}
				if (!found) {
					fixedNeighbors.Remove(otherTile);
				}
			}
			tile.neighbors = fixedNeighbors.ToArray();
		}
		//Order Edges and Neighbors according to the circle
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			//Edges
			Edge[] edges = tile.edges;
			float[] angles = new float[edges.Length];
			for (int m = 0; m < edges.Length; m++) {
				Vector2 relVec = Vector2.Lerp(edges[m].p0, edges[m].p1, 0.5f) - tile.sitePoint;
				angles[m] = Mathf.Atan2(relVec.y, relVec.x);
			}
			Array.Sort(angles,edges);
			//Neighbors
			Tile[] orderedNeighbors = new Tile[tile.neighbors.Length];
			for (int n = 0; n < orderedNeighbors.Length; n++) {
				//Debug.DrawLine(tile.sitePoint, tile.neighbors[n].sitePoint, Color.cyan, Mathf.Infinity);
				Edge edge = edges[n];
				if (edge.neighbor0 != tile) {
					orderedNeighbors[n] = edge.neighbor0;
					edge.neighborNumber1 = n;
				} else {
					orderedNeighbors[n] = edge.neighbor1;
					edge.neighborNumber0 = n;
				}
			}
			tile.neighbors = orderedNeighbors; //Setting Proper
		}
		//Oceans
		/*
		float seed = (int)(UnityEngine.Random.value * 100000f);
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			Vector2 difVec = tile.sitePoint - new Vector2(width/2f, height/2f);
			float angle = Mathf.Atan2(difVec.y, difVec.x);
			float a = width / 2f * 1.15f;
			float b = height / 2f * 1.15f;
			float rad = a * b/ (Mathf.Sqrt(a * a * Mathf.Sin(angle) * Mathf.Sin(angle) + b * b * Mathf.Cos(angle) * Mathf.Cos(angle)));
			float totScale = 0.6f;
			float xscale = 3f * totScale;
			float yscale = 1 / 3f * totScale;
			for (int m = 0; m < 2; m++) {
				rad -= yscale * Mathf.PerlinNoise(Mathf.Cos(angle) * xscale + seed, Mathf.Sin(angle) * xscale + seed);
				xscale /= 2;
				yscale /= 2;
			}
			float perlin = rad;
			if (Vector2.Distance(Vector2.zero,difVec) > perlin) {
				tile.ocean = true;
			}
		}
		*/
		//Setting map
		MapLayout.generate(this);
		//Building meshes
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			/*/Debugging..........................................................................
			for (int q = 0; q < tile.edges.Length; q++) {
				Debug.DrawLine(tile.edges[q].p0, tile.edges[q].p1, Color.green, Mathf.Infinity);
			}
			*///End Debugging......................................................................
			tile.meshBuilder();
		}
		return true;
	}

	//Funcitions
	
	public Mesh mapMesher() {
		Color lineColor = Color.HSVToRGB(65, 7, 31);
		Mesh background = MeshBuilder.rectangle(width, height, lineColor);
		List <Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Color> colors = new List<Color>();
		vertices.AddRange(background.vertices);
		triangles.AddRange(background.triangles);
		colors.AddRange(background.colors);
		for (int l = 0; l < tiles.Length; l++) {
			List<int> tempTriangles = new List<int>();
			tempTriangles.AddRange(tiles[l].mesh.triangles);
			for (int m = 0; m < tempTriangles.Count; m++) {
				tempTriangles[m] += vertices.Count;
			}
			vertices.AddRange(tiles[l].mesh.vertices);
			triangles.AddRange(tempTriangles);
			colors.AddRange(tiles[l].mesh.colors);
		}
		Mesh combinedMesh = new Mesh();
		combinedMesh.vertices = vertices.ToArray();
		combinedMesh.triangles = triangles.ToArray();
		combinedMesh.colors = colors.ToArray();
		return combinedMesh;
	}

	private Vector2 V2f(Vector2f vect) {
		return new Vector2(vect.x, vect.y);
	}
	private Vector2f V2f(Vector2 vect) {
		return new Vector2f(vect.x, vect.y);
	}
}
