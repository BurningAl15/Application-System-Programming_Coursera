using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
    public float degPerSec = 90;
	
	// Update is called once per frame
	void Update () {
        // In order for this to move in the Pause screen, we need to use Time.realtimeSinceStartup
        transform.rotation = Quaternion.Euler(0, 0, degPerSec * Time.realtimeSinceStartup);
	}
}
