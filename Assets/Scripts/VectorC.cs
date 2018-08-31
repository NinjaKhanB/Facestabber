using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorC : MonoBehaviour {
	public static bool equals(Vector2 a, Vector2 b) {
		if (Mathf.Abs(a.x - b.x) <= 0.0000005) {
			if (Mathf.Abs(a.y - b.y) <= 0.0000005) {
				return true;
			}
		}
		return false;
	}
}
