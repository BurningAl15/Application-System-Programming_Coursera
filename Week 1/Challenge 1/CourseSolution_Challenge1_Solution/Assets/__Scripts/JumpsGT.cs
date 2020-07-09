using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(Text) )]
public class JumpsGT : MonoBehaviour {
    Text    txt;

	// Use this for initialization
	void Start () {
        txt = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        // This is a Ternary Operator: https://www.dotnetperls.com/ternary 
        txt.text = (PlayerShip.JUMPS >= 0) ? PlayerShip.JUMPS+" Jumps" : "";
	}
}
