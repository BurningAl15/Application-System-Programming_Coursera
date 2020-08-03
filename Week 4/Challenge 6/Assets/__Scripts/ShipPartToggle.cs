using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(Toggle) )]
public class ShipPartToggle : MonoBehaviour {
    public ShipPart.eShipPartType partType;
    public int      partNum;

    [HideInInspector]
    public Toggle   toggle;


    Text            label;
    Image           backgroundImage;

	// Use this for initialization
	void Awake () {
        toggle = GetComponent<Toggle>();

        // The Unity documentation shows using an anonymous delegate for this 
        //  callback, but anonymous delegates are generally frowned upon by
        //  Unity because of the memory/garbage collection issues that can be 
        //  caused by the closure that the delegate generates.
        // Here, instead, I've assigned the method ValueChanged(bool) as the 
        //  listener. Note: This is only allowed because ValueChanged accepts
        //  a single bool as its parameters.
        toggle.onValueChanged.AddListener(ValueChanged);

        // Set the label text of the Toggle
        label = GetComponentInChildren<Text>();
        if (label != null)
        {
            label.text = ShipPartsDictionary.DICT[partType].partInfos[partNum].name;
        }

        // Get a reference to the backgroundImage
        try {
            backgroundImage = transform.Find("Background").GetComponent<Image>();
        }
        catch
        {
            Debug.LogError("ShipPartToggle:Awake() – Could not find a child named " +
                           "Background that had an Image component.");
        }
	}

    void ValueChanged(bool tf)
    {
        if (toggle.isOn)
        {
            Debug.Log("Assigning ship "+partType+" to #:"+partNum);
            ShipCustomization.SetPart(partType, partNum);
        }
    }

    public bool isOn
    {
        get { return toggle.isOn; }
        set 
        {
            toggle.isOn = value;
        }
    }

    public void UnlockPart() {
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = true;
            backgroundImage.color = Color.white;
        }
    }


    public void LockPart() {
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = false;
            backgroundImage.color = Color.red;
            toggle.isOn = false;
        }
    }

}
