using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(RectTransform) )]
[RequireComponent( typeof(Image) )]
public class GameOverPanel : ActiveOnlyDuringSomeGameStates {

    public enum eGameOverPanelState {
        none, idle, fadeIn, fadeIn2, fadeIn3, display
    }
    
    [Header("Set in Inspector")]
    [Tooltip("The amount of time that it will take for this panel to fade in or fade out.")]
    public float fadeTime = 1f;
    
    [Header("Set Dynamically")]
    [SerializeField]
    private eGameOverPanelState state = eGameOverPanelState.none;
    
    Image img;
    Text levelText, infoText;
    RectTransform levelRT;//, infoRT; // infoRT was unused, so I've commented it out. – JGB
    float stateStartTime, stateDuration;
    eGameOverPanelState nextState;
    
    AsteraX.CallbackDelegate displayCallback, idleCallback;
    
    // Use this for initialization
    override public void Awake () {
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
        //infoRT = infoT.GetComponent<RectTransform>(); // infoRT was unused, so I've commented it out. – JGB
        infoText = infoT.GetComponent<Text>();
        if (infoText == null) {
            Debug.LogWarning("LevelAdvancePanel:Start() - LevelAdvancePanel child InfoText needs a Text component.");
            return;
        }
        
        SetState(eGameOverPanelState.idle);

        base.Awake();
    }

    protected override void DetermineActive()
    {
        base.DetermineActive();
        if (AsteraX.GAME_STATE == AsteraX.eGameState.gameOver) {
            // This should only happen when the game is over
            SetState(eGameOverPanelState.fadeIn);
        }
    }

    void SetState(eGameOverPanelState newState) 
    {
        stateStartTime = realTime;
        
        switch (newState) {
            case eGameOverPanelState.idle:
                gameObject.SetActive(false);
                if (idleCallback != null) {
                    idleCallback();
                    idleCallback = null;
                }
                break;
                
            case eGameOverPanelState.fadeIn:
                gameObject.SetActive(true);
                // Set text            
                levelRT.localScale = new Vector3(1,0,1);
                infoText.text = "Final Level: " //+ AsteraX.GAME_LEVEL
                    + "\nFinal Score: "+AsteraX.SCORE.ToString("N0");
                infoText.color = Color.clear;
                // Set initial state
                img.color = Color.clear;
                levelRT.localScale = new Vector3(1,0,1);
                infoText.color = Color.clear;
                // Set timiing and advancement
                stateDuration = fadeTime * 0.2f;
                nextState = eGameOverPanelState.fadeIn2;
                break;
                
            case eGameOverPanelState.fadeIn2:
                // Set initial state
                img.color = Color.black;
                levelRT.localScale = new Vector3(1,0,1);
                infoText.color = Color.clear;
                // Set timiing and advancement
                stateDuration = fadeTime*0.6f;
                nextState = eGameOverPanelState.fadeIn3;
                break;
                
            case eGameOverPanelState.fadeIn3:
                // Set initial state
                img.color = Color.black;
                levelRT.localScale = new Vector3(1,1,1);
                infoText.color = Color.clear;
                // Set timiing and advancement
                stateDuration = fadeTime*0.2f;
                nextState = eGameOverPanelState.display;
                break;
                
            case eGameOverPanelState.display:
                stateDuration = 999999; // A very long time
                nextState = eGameOverPanelState.none;
                if (displayCallback != null) {
                    displayCallback();
                    displayCallback = null;
                }
                break;
        }
        
        state = newState;
    }
    
    // Update is called once per frame
    void Update () {
        if (state == eGameOverPanelState.none) {
            return;
        }
        float u = (realTime - stateStartTime)/stateDuration;
        bool moveNext = false;
        if (u > 1) {
            u = 1;
            moveNext = true;
        }
        float n;
        switch (state) {
            case eGameOverPanelState.fadeIn: // Background
                img.color = new Color(0,0,0,u);
                break;
                
            case eGameOverPanelState.fadeIn2: // LevelText
                n = LevelTextYScaleEffect(u);
                levelRT.localScale = new Vector3(1, n, 1);
                break;
                
            case eGameOverPanelState.fadeIn3: // InfoText
                n = u*u;
                infoText.color = new Color(1,1,1,n);
                break;
                
            case eGameOverPanelState.display:
                // Just wait until the game restarts based on AsteraX code
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
        return u * Mathf.Cos(u * Mathf.PI * 2);
        //return u + Mathf.Sin(Mathf.PI * u);
    }
    
    float realTime {
        get {
            return Time.realtimeSinceStartup;
        }
    }
}
