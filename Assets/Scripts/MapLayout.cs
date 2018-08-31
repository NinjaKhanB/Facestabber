using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLayout : MonoBehaviour {
	public static void generate(Map map) {
		int seed = (int)(UnityEngine.Random.value * 10000);
		float scale = 4f;
		Tile[] tiles = map.tiles;
		//Mountain Setup
		int mountainNumber = tiles.Length / 150;
		int mountainLength = 7;
		int[] mountainStart = new int[tiles.Length];
		for (int l = 0; l < mountainNumber; l++) {
			int num = Mathf.FloorToInt(UnityEngine.Random.Range(0, tiles.Length));
			mountainStart[num] = 1;
		}
		//Lake Setup
		int lakeNumber = tiles.Length / 75;
		int[] lakeStart = new int[tiles.Length];
		for (int l = 0; l < lakeNumber; l++) {
			int num = Mathf.FloorToInt(UnityEngine.Random.Range(0, tiles.Length));
			lakeStart[num] = 1;
		}
		for (int l = 0; l < tiles.Length; l++) {
			Tile tile = tiles[l];
			//Mountain
			Vector2 point = tile.sitePoint;
			if (mountainStart[l] == 1 && !tile.mountain) {
				Tile curTile = tile;
				float angle = UnityEngine.Random.Range(0f,2f*(float)Math.PI);
				Vector2 line = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
				line.Normalize();
				for (int m = 0; m < mountainLength; m++) {
					curTile.mountain = true;
					Tile[] nies = neighborVec(curTile, line);
					if (nies.Length > 1) {
						nies[1].mountain = true;
					}
					if (nies.Length >0) {
						curTile = nies[0];
					}
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
		}
	}
	public static T[] evenDis<T>(Map map, T[] disList, int iterations) {
		int listNumber = disList.Length;
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
				Tile thisOne = tiles[m];
				for (int n = 0; n < curList.Length; n++) {
					Tile other = tiles[n];
					if (m != n) {
						Vector2 difVector = other.sitePoint - thisOne.sitePoint;
						float len = Vector2.Distance(Vector2.zero, difVector);
						float mag = 1 / (len * len);
						forces += difVector.normalized * mag;
					}
				}
				Vector2 forceNorm = forces.normalized;
				tiles[m] = neighborVec(thisOne,forceNorm)[0];
			}
		}
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
