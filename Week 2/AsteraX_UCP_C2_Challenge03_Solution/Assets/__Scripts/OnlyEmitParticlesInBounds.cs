using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class stops any attached ParticleSystems from emitting particles if the
///  GameObject to which it is attached is out of bounds of the ScreenBounds.
/// </summary>
[RequireComponent( typeof(ParticleSystem) )]
public class OnlyEmitParticlesInBounds : MonoBehaviour {
    
    private ParticleSystem.EmissionModule emitter;

	// Use this for initialization
	void Start () {
        // Get the EmissionModule of the attached ParticleSystem
        emitter = GetComponent<ParticleSystem>().emission;
	}
	
	// LateUpdate is called once per frame, after all Updates have been called.
	void LateUpdate () {
        if (ScreenBounds.OOB( transform.position )) {
            emitter.enabled = false;
        } else {
            emitter.enabled = true;
        }
	}

}
