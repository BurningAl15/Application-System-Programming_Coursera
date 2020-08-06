//#define DEBUG_ShipCustomization_Testing

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCustomization : MonoBehaviour {
    static private ShipCustomization _S; // Protected Singleton. See S property below.

    Dictionary<ShipPart.eShipPartType, Transform> currPartsDict;

    private void Start()
    {
        S = this;

        // Find replaceable parts on this ship or its children
        currPartsDict = new Dictionary<ShipPart.eShipPartType, Transform>();
        ShipPart[] parts = GetComponentsInChildren<ShipPart>();
        foreach (ShipPart sp in parts)
        {
            currPartsDict.Add(sp.type, sp.transform);
        }
    }

    public static bool SetPart(ShipPart.eShipPartType type, int num)
    {
        return S.SetPartNS(type, num);
    }
    bool SetPartNS(ShipPart.eShipPartType type, int num)
    {
        // Ensure that this is asking for an extant ship part
        if (!ShipPartsDictionary.DICT.ContainsKey(type))
        {
            Debug.LogError("ShipCustomization:SetPartNS - ShipPartsDictionary.DICT does not contain type: " + type);
            return false;
        }
        if (!currPartsDict.ContainsKey(type))
        {
            Debug.LogError("ShipCustomization:SetPartNS - currPartsDict does not contain type: " + type);
            return false;
        }
        if (num < 0 || ShipPartsDictionary.DICT[type].partInfos.Length <= num)
        {
            Debug.LogError("ShipCustomization:SetPartNS - Attempt to choose nonextant " + type + ": "+num);
            return false;
        }

        // Now we know that this is a valid type and part num...
        // Pull the information from the current part in that place
        Transform currTrans = currPartsDict[type];
        Vector3 lPos = currTrans.localPosition;
        Quaternion lRot = currTrans.localRotation;
        Transform parentTrans = currTrans.parent;

        // Generate a new part and position it correctly
        Transform newTrans = Instantiate<GameObject>(ShipPartsDictionary.DICT[type].partInfos[num].prefab).transform;
        newTrans.SetParent(parentTrans);
        newTrans.localPosition = lPos;
        newTrans.localRotation = lRot;

        // Replace the currTrans with the newTrans in the currPartsDict
        currPartsDict[type] = newTrans;

        // Destroy the old one
        Destroy(currTrans.gameObject);

        SaveGameManager.Save();

        return true;
    }


    /// <summary>
    /// <para>This static public property provides some protection for the Singleton _S.</para>
    /// <para>get {} does return null, but throws an error first.</para>
    /// <para>set {} allows overwrite of _S by a 2nd instance, but throws an error first.</para>
    /// <para>Another advantage of using a property here is that it allows you to place
    /// a breakpoint in the set clause and then look at the call stack if you fear that 
    /// something random is setting your _S value.</para>
    /// </summary>
    static public ShipCustomization S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("ShipCustomization:S getter - Attempt to get value of S before it has been set.");
                return null;
            }
            return _S;
        }
        private set
        {
            if (_S != null)
            {
                Debug.LogError("ShipCustomization:S setter - Attempt to set S when it has already been set.");
            }
            _S = value;
        }
    }



#if DEBUG_ShipCustomization_Testing
    // Everything in here is part of testing that should be turned off once we know this will work.
    [Header("DEBUG ONLY")]
    public bool                     checkToSwapPart;
    public ShipPart.eShipPartType   swapToType;
    public int                      swapToNum;

    private void OnDrawGizmos()
    {
        if (!Application.isEditor || !Application.isPlaying)
        {
            return;
        }

        if (checkToSwapPart)
        {
            ShipCustomization.SetPart(swapToType, swapToNum);
            checkToSwapPart = false;
        }
    }

#endif
}
