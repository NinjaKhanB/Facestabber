using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile{
	public Vector2 sitePoint;
	public Edge[] edges;
	public Tile[] neighbors;
	public Player[] owner;
	public Edge[] riverEdges;
	public int armies;
	public int tileType; //0 Farmland, 1 Mountain, 2 Village, 3 Town, 4 City
	public List <Vector2> uncheckedNeighborSites;
	public List<Tile> tempNeighbors;
	public Tile() {
		tileType = 0;
		owner = null;
	}
}
