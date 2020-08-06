using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class MobileFireButton : MonoBehaviour
{
    private bool registeredWithPauseChanged = false;
    private Image img;
    
    void Start()
    {
        img = GetComponent<Image>();
        //Initially disable the image
        img.raycastTarget = false;

        if (Application.isMobilePlatform)
        {
            RegisterWithPauseChanged();
            PauseChangedCallback();
        }
        else
        {
            //If this is the editor, check every second
#if UNITY_EDITOR
            StartCoroutine(CheckForUnityRemote(1));
#else
            //If this is not a mobile platform & not the editor, disable this button
            //gameObject.SetActive(false);
#endif
        }

    }

    IEnumerator CheckForUnityRemote(float delay)
    {
        while (!registeredWithPauseChanged)
        {
            if (UnityEditor.EditorApplication.isRemoteConnected)
            {
                Debug.Log("MobileFireButton:CheckForUnityRemote() - Connected to Unity Remote");
                RegisterWithPauseChanged();
                PauseChangedCallback();
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
        }
    }
    
    void RegisterWithPauseChanged()
    {
        if (registeredWithPauseChanged) return;
        
        //Remove any previous registration
        AsteraX.PAUSED_CHANGE_DELEGATE -= PauseChangedCallback;
        //Register
        AsteraX.PAUSED_CHANGE_DELEGATE += PauseChangedCallback;

        registeredWithPauseChanged = true;
    }

    void PauseChangedCallback()
    {
        //Enabling or disabling raycastTarget traps or ignores taps
        img.raycastTarget = !AsteraX.PAUSED;
    }
}
