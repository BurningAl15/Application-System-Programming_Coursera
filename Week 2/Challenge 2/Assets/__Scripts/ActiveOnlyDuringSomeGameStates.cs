using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveOnlyDuringSomeGameStates : MonoBehaviour {

    //Solution Week 2A
    public enum ePauseEffect
    {
        ignorePause,
        activeWhenPaused,
        activeWhenNotPaused
    }
    
    // eGameStates is a System.Flags enum, so many values can be stored in a single field.
    [EnumFlags] // This uses the EnumFlagsAttribute from EnumFlagsAttributePropertyDrawer
    public AsteraX.eGameState   activeStates = AsteraX.eGameState.all;
    //Solution Week 2A
    public ePauseEffect pauseEffect = ePauseEffect.ignorePause;    
    
	// Use this for initialization
	public virtual void Awake () {

        // Also make sure to set self based on the current state when awakened
        DetermineActive();
        
        // Register this callback with the static public delegates on AsteraX.
        AsteraX.GAME_STATE_CHANGE_DELEGATE += DetermineActive;
        //Solution Week 2A
        AsteraX.PAUSED_CHANGE_DELEGATE += DetermineActive;
    }

    protected void OnDestroy()
    {
        // Unregister this callback from the static public delegates on AsteraX.
        AsteraX.GAME_STATE_CHANGE_DELEGATE -= DetermineActive;
        //Solution Week 2A
        AsteraX.PAUSED_CHANGE_DELEGATE -= DetermineActive;
    }


    protected virtual void DetermineActive()
    {
        // This line uses a bitwise AND (&) to compare each bit of activeStates and newState.
        // If the result is the same as newState, then the bit for that newState must also be
        //  true in activeStates, meaning that newState is one of the states where this
        //  GameObject should be active.
        bool shouldBeActive = (activeStates & AsteraX.GAME_STATE) == AsteraX.GAME_STATE;

        if (shouldBeActive)
        {
            switch (pauseEffect)
            {
                case ePauseEffect.activeWhenNotPaused:
                    shouldBeActive = !AsteraX.PAUSED;
                    break;
                case ePauseEffect.activeWhenPaused:
                    shouldBeActive = AsteraX.PAUSED;
                    break;
            }
        }
        
        gameObject.SetActive(shouldBeActive);
    }
    
}
