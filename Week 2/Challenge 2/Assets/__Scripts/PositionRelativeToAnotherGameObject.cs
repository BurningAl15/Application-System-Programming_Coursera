using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple script that will position this GameObject relative to a "poi"
///  (Point of Interest).
/// </summary>
public class PositionRelativeToAnotherGameObject : MonoBehaviour {
    public Transform poi;
    Vector3 offset;

	// Use this for initialization
	void Start () {
        offset = transform.position - poi.position;
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = poi.position + offset;
	}
}
