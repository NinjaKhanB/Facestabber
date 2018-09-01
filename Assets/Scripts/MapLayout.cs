using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLayout : MonoBehaviour {
	public static void generate(Map map) {
		int seed = (int)(UnityEngine.Random.value * 10000);
		float scale = 4f;
		Tile[] tiles = map.tiles;
		//Mountain and Lake Setup
		int mountainNumber = tiles.Length / 300;
		int mountainLength = 7;
		int lakeNumber = tiles.Length / 300;
		List<int> naturalNeed = new List<int>();
		for (int l = 0; l < mountainNumber; l++) {
			naturalNeed.Add(1); //Mountains are 1
		}
		for (int l = 0; l < lakeNumber; l++) {
			naturalNeed.Add(2); //Lakes are 2
		}

		var naturalPlacement = evenDis(map,naturalNeed, (int)Mathf.Sqrt((int)tiles.Length)/3, lakeNumber+mountainNumber);
		for (int l = 0; l < naturalPlacement.Length / 2; l++) {
			int tileNum = (int)naturalPlacement[l, 1];
			Tile tile = tiles[tileNum];
			//Mountain
			if ((int)naturalPlacement[l, 0] == 1) {
				Vector2 point = tile.sitePoint;
				float angle = UnityEngine.Random.Range(0f, 2f * (float)Math.PI);
				Vector2 line = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				line.Normalize();
				Tile curTile = tile;
				for (int m = 0; m < mountainLength; m++) {
					curTile.mountain = true;
					Tile[] nies = neighborVec(curTile, line);
					if (nies.Length > 1) {
						nies[1].mountain = true;
					}
					if (nies.Length > 0) {
						curTile = nies[0];
					}
				}
			}
			tile = tiles[tileNum];
			if ((int)naturalPlacement[l, 0] == 2) {
				tile.lake = true;
				for (int m = 0; m < tile.neighbors.Length; m++) {
					if (tile.neighbors[m] != null) {
						tile.neighbors[m].lake = true;
					}
				}
			}
		}
	}
	public static System.Object[,] evenDis<T>(Map map, List<T> disList, int iterations, int objectNum) {
		int listNumber = disList.Count;
		Tile[] tiles = map.tiles;
		List <int> startingNums = new List<int>();
		for (int l = 0; l < listNumber; l++) {
			int num = Mathf.FloorToInt(UnityEngine.Random.Range(0, tiles.Length));
			startingNums.Add(num);
		}
		int[] curList = startingNums.ToArray();
		for (int l = 0; l < iterations; l++) {
			for (int m = 0; m < curList.Length; m++) {
				Vector2 forces = Vector2.zero;
				Tile thisOne = tiles[curList[m]];
				for (int n = 0; n < curList.Length; n++) {
					Tile other = tiles[curList[n]];
					if (m != n) {
						Vector2 difVector = other.sitePoint - thisOne.sitePoint;
						float len = Vector2.Distance(Vector2.zero, difVector);
						float mag = 1 / (len * len);
						forces += difVector.normalized * mag;
					}
					Vector2[] walls = new Vector2[] {Vector2.up, Vector2.down,Vector2.left,Vector2.right};
					for (int o = 0; o < walls.Length; o++) {
						float len = 0;
						Vector2 dir = walls[o];
						if (dir == Vector2.down) {
							len = thisOne.sitePoint.y;
						}
						if (dir == Vector2.up) {
							len = map.height - thisOne.sitePoint.y;
						}
						if (dir == Vector2.left) {
							len = thisOne.sitePoint.x;
						}
						if (dir == Vector2.right) {
							len = map.width - thisOne.sitePoint.x;
						}
						float mag = 1f / (len * len);
						forces += 2/((float)objectNum) * dir * mag;
					}
				}
				Vector2 forceNorm = forces.normalized;
				Debug.Log(forceNorm * 100f);
				var neiVec = neighborVec(thisOne, forceNorm);
				if (neiVec.Length > 0) {
					curList[m] = neiVec[0].refInt;
				}
			}
		}
		System.Object[,] retList = new System.Object[disList.Count, 2];
		for (int l = 0; l < curList.Length; l++) {
			retList[l, 0] = disList[l];
			retList[l, 1] = curList[l];
		}
		return retList;
	}
	public static Tile[] neighborVec(Tile curTile, Vector2 vec) {
		List<Tile> viables = new List<Tile>();
		for (int n = 0; n < curTile.neighbors.Length; n++) {
			if (curTile.neighbors[n] != null) {
				viables.Add(curTile.neighbors[n]);
			}
		}
		Tile[] nies = viables.ToArray();
		float[] distances = new float[nies.Length];
		for (int n = 0; n < distances.Length; n++) {
			distances[n] = Vector2.Dot(nies[n].sitePoint - curTile.sitePoint, vec);
		}
		Array.Sort(distances, nies);
		return nies;
	}
}
//Lake
			/*
			float mapLevel = 0;
			for (int m = 0; m < tile.edges.Length; m++) {
				Vector2 sitePoint = tile.edges[m].p0;
				mapLevel += Mathf.PerlinNoise(sitePoint.x * scale + seed, sitePoint.y * scale + seed);
			}
			mapLevel /= tile.edges.Length;
			if (mapLevel <= 0.15f) {
				tile.lake = true;
			}
			*/