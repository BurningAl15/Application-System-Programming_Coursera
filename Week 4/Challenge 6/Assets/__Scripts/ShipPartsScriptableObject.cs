using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Scriptable Objects/ShipPartsSO", fileName = "ShipPartsSO.asset")]
[System.Serializable]
public class ShipPartsScriptableObject : ScriptableObject
{
    public ShipPart.eShipPartType   type;
    public ShipPartInfo[]           partInfos;
}

[System.Serializable]
public class ShipPartInfo
{
    public string       name;
    public GameObject   prefab;
}
