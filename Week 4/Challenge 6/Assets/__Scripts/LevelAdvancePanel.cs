using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(RectTransform) )]
[RequireComponent( typeof(Image) )]
public class LevelAdvancePanel : MonoBehaviour {
	static private LevelAdvancePanel S;

	public enum eLevelAdvanceState {
		none, idle, fadeIn, fadeIn2, fadeIn3, display, fadeOut, fadeOut2, fadeOut3
	}

    [Header("Set in Inspector")]
	public float fadeTime = 1f;
	public float displayTime = 1f;

    [Header("Set Dynamically")]
    [SerializeField]
	private eLevelAdvanceState state = eLevelAdvanceState.none;

	Image img;
	Text levelText, infoText;
    RectTransform levelRT;//, infoRT; // infoRT was unused, so I've commented it out. – JGB
	float stateStartTime, stateDuration;
	eLevelAdvanceState nextState;

	AsteraX.CallbackDelegate displayCallback, idleCallback;

	// Use this for initialization
	void Awake () {
		S = this;

		img = GetComponent<Image>();

        // Find the LevelText child
		Transform levelT = transform.Find("LevelText");
		if (levelT == null) {
			Debug.LogWarning("LevelAdvancePanel:Start() - LevelAdvancePanel lacks a child named LevelText.");
			return;
		}
		levelRT = levelT.GetComponent<RectTransform>();
        levelText = levelT.GetComponent<Text>();
        if (levelText == null) {
            Debug.LogWarning("LevelAdvancePanel:Start() - LevelAdvancePanel child LevelText needs a Text component.");
            return;
        }

        // Find the InfoText child
		Transform infoT = transform.Find("InfoText");
		if (infoT == null) {
			Debug.LogWarning("LevelAdvancePanel:Start() - LevelAdvancePanel lacks a child named InfoText.");
            return;
        }
        //infoRT = infoT.GetComponent<RectTransform>();; // infoRT was unused, so I've commented it out. – JGB
		infoText = infoT.GetComponent<Text>();
        if (infoText == null) {
			Debug.LogWarning("LevelAdvancePanel:Start() - LevelAdvancePanel child InfoText needs a Text component.");
            return;
        }

		SetState(eLevelAdvanceState.idle);
	}

	void SetState(eLevelAdvanceState newState) 
	{
		stateStartTime = realTime;

		switch (newState) {
			case eLevelAdvanceState.idle:
				gameObject.SetActive(false);
                if (idleCallback != null) {
					idleCallback();
					idleCallback = null;
                }
				break;

			case eLevelAdvanceState.fadeIn:
				gameObject.SetActive(true);
				// Set text            
				levelText.text = "Level "+AsteraX.GAME_LEVEL;
				levelRT.localScale = new Vector3(1,0,1);
				AsteraX.LevelInfo lvlInfo = AsteraX.GetLevelInfo();
				infoText.text = "Asteroids: "+lvlInfo.numInitialAsteroids+"\tChildren: "+lvlInfo.numSubAsteroids;
				infoText.color = Color.clear;
				// Set initial state
				img.color = Color.clear;
				levelRT.localScale = new Vector3(1,0,1);
				infoText.color = Color.clear;
                // Set timiing and advancement
				stateDuration = fadeTime * 0.2f;
				nextState = eLevelAdvanceState.fadeIn2;
				break;

			case eLevelAdvanceState.fadeIn2:
                // Set initial state
                img.color = Color.black;
                levelRT.localScale = new Vector3(1,0,1);
                infoText.color = Color.clear;
                // Set timiing and advancement
                stateDuration = fadeTime*0.6f;
                nextState = eLevelAdvanceState.fadeIn3;
				break;

			case eLevelAdvanceState.fadeIn3:
                // Set initial state
				img.color = Color.black;
                levelRT.localScale = new Vector3(1,1,1);
                infoText.color = Color.clear;
                // Set timiing and advancement
				stateDuration = fadeTime*0.2f;
				nextState = eLevelAdvanceState.display;
				break;

			case eLevelAdvanceState.display:
				stateDuration = displayTime;
				nextState = eLevelAdvanceState.fadeOut;
				if (displayCallback != null) {
					displayCallback();
					displayCallback = null;
				}
				break;

			case eLevelAdvanceState.fadeOut:
                // Set initial state
                img.color = Color.black;
                levelRT.localScale = new Vector3(1,1,1);
                infoText.color = Color.white;
                // Set timiing and advancement
				stateDuration = fadeTime * 0.2f;
				nextState = eLevelAdvanceState.fadeOut2;
				break;

			case eLevelAdvanceState.fadeOut2:
                // Set initial state
                img.color = Color.black;
                levelRT.localScale = new Vector3(1,1,1);
                infoText.color = Color.clear;
                // Set timiing and advancement
                stateDuration = fadeTime*0.6f;
                nextState = eLevelAdvanceState.fadeOut3;
                break;

			case eLevelAdvanceState.fadeOut3:
                // Set initial state
                img.color = Color.black;
                levelRT.localScale = new Vector3(1,0,1);
                infoText.color = Color.clear;
                // Set timiing and advancement
                stateDuration = fadeTime*0.2f;
                nextState = eLevelAdvanceState.idle;
                break;
		}

        state = newState;
	}

	// Update is called once per frame if enabled == true
	void Update () {
		if (state == eLevelAdvanceState.none) {
			return;
		}
		float u = (realTime - stateStartTime)/stateDuration;
		bool moveNext = false;
		if (u >= 1) {
			u = 1;
			moveNext = true;
		}
		float u1 = 1-u;
		float n;
		switch (state) {
			case eLevelAdvanceState.fadeIn: // Background
				img.color = new Color(0,0,0,u);
				break;

			case eLevelAdvanceState.fadeIn2: // LevelText
                n = LevelTextYScaleEffect(u);
				levelRT.localScale = new Vector3(1, n, 1);
				break;
                
			case eLevelAdvanceState.fadeIn3: // InfoText
				n = u*u;
				infoText.color = new Color(1,1,1,n);
				break;

			case eLevelAdvanceState.display:
				// Trigger the game to advance to the next level
				break;

			case eLevelAdvanceState.fadeOut: // InfoText
				n = u1*u1;
				infoText.color = new Color(1,1,1,n);
				break;

			case eLevelAdvanceState.fadeOut2: // LevelText
                n = LevelTextYScaleEffect(u1);
				levelRT.localScale = new Vector3(1,n,1);
				break;

			case eLevelAdvanceState.fadeOut3: // Background
				n = u1*u1;
				img.color = new Color(0,0,0,n);
				break;

			default:
                // Case for idle and none states
				break;            
		}

		if (moveNext) {
			SetState(nextState);
		}
	}

    float LevelTextYScaleEffect(float u) {
        //return u * Mathf.Cos(u * Mathf.PI * 2);
        return u + Mathf.Sin(Mathf.PI * u);
    }

    float realTime {
        get {
            return Time.realtimeSinceStartup;
        }
    }


	static public void AdvanceLevel(AsteraX.CallbackDelegate displayCB, AsteraX.CallbackDelegate idleCB) {
		if (S == null) {
			Debug.LogError("LevelAdvancePanel:AdvanceLevel() - Called while S is null.");
			return;
		}

		S.displayCallback = displayCB;
		S.idleCallback = idleCB;

		S.SetState(eLevelAdvanceState.fadeIn);
	}
}
