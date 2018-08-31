using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile{
	public Vector2 sitePoint;
	public Edge[] edges;
	public Tile[] neighbors;
	public Player owner;
	public Edge[] riverEdges;
	public int armies;
	public bool mountain = false;
	public bool lake = false;
	public bool ocean = false;
	public int tileType; //0 Farmland, 1 Mountain, 2 Village, 3 Town, 4 City
	public List <Vector2> uncheckedNeighborSites;
	public List<Tile> tempNeighbors;
	public Mesh mesh;
	public int refInt;
	Color color;
	public Tile(int refInt) {
		this.refInt = refInt;
		tileType = 0;
		owner = null;
		uncheckedNeighborSites = new List<Vector2>();
		tempNeighbors = new List<Tile>();
	}
	public void meshBuilder() {
		this.colorUpdate();
		float dropIn = 0.002f;
		mesh = MeshBuilder.tileMaker(edges, dropIn, color);
	}
	public void colorUpdate() {
		if (owner == null) {
			color = Color.HSVToRGB(65f/360f, 0.07f, 0.88f);
		} else {
			Color full = Color.HSVToRGB(owner.hue / 360f, 0.100f, 0.32f);
			Color empty = Color.HSVToRGB(owner.hue / 360f, 0.68f, 0.85f);
			float fullNum = 2 / Mathf.PI * Mathf.Atan(armies / 3f);
			color = Color.Lerp(empty, full, fullNum);
		}
		if (lake) {
			color = Color.HSVToRGB(211/360f, 0.82f, 0.48f);
		}
		if (mountain) {
			color = Color.HSVToRGB(0f, 0f, 0.28f);
		}
		if (ocean) {
			color = Color.HSVToRGB(225/360f, 0.82f, 0.46f);
		}
	}
}
