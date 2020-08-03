//#define DEBUG_ShipCustomization_Testing

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPart : MonoBehaviour {
    public enum eShipPartType
    {
        body,
        turret
#if DEBUG_ShipCustomization_Testing
        , testBadPartType // This is only used #if DEBUG_ShipCustomization_Testing
#endif
    }

    public eShipPartType type;
}
