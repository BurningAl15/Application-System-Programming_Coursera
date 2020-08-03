using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPartsDictionary : MonoBehaviour
{
    // NOTE: As written here, DICT could be modified by any script. Be careful with things like this.
    public static Dictionary<ShipPart.eShipPartType, ShipPartsScriptableObject> DICT;

    // This allows all of the ShipPartsScriptableObjects to be assigned to this in the Inspector.
    public ShipPartsScriptableObject[] shipPartSOs;

    private void Awake()
    {
        // Create a Dictionary of ShipPartsScriptableObjects, which will make them easier to look up later
        DICT = new Dictionary<ShipPart.eShipPartType, ShipPartsScriptableObject>();
        foreach (ShipPartsScriptableObject spso in shipPartSOs)
        {
            DICT.Add(spso.type, spso);
        }
    }
}
