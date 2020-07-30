using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/AsteroidsSO", fileName = "AsteroidsSO.asset")]
[System.Serializable]
public class AsteroidsScriptableObject : ScriptableObject
{
    static public AsteroidsScriptableObject S; // This Scriptable Object is an unprotected Singleton

    public AsteroidsScriptableObject()
    {
        S = this; // Assign the Singleton as part of the constructor.
    }

	public float        minVel = 5;
	public float        maxVel = 10;
    public float        maxAngularVel = 10;
	public int          initialSize = 3;
	public float        asteroidScale = 0.75f;
    public int          numSmallerAsteroidsToSpawn = 2;
    public int[]        pointsForAsteroidSize = {0, 400, 200, 100};

	public GameObject[] asteroidPrefabs;

	public GameObject[] asteroidParticlePrefabs;

    public GameObject GetAsteroidPrefab()
    {
        int ndx = Random.Range(0, asteroidPrefabs.Length);
        return asteroidPrefabs[ndx];
    }
        
	public GameObject GetAsteroidParticlePrefab()
	{
		int ndx = Random.Range(0,asteroidParticlePrefabs.Length);
		return asteroidParticlePrefabs[ndx];
	}
        
}
