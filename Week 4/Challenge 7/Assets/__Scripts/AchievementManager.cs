using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    static private AchievementManager _S; // A private singleton for AchievementManager

    [Header("Set in Inspector")]
    public AchievementPopUp popUp;
    public StepRecord[]     stepRecords;
    public Achievement[]    achievements;


    // This is something that I commonly do in coding. Dictionaries are extremely
    //  useful and have constant time access, but they do not appear in the Inspector,
    //  so I have an Array or List in the Inspector and on Awake create references
    //  to the elements of that Array in a static private Dictionary.
    static private Dictionary<Achievement.eStepType, StepRecord> STEP_REC_DICT;

    private void Awake()
    {
        S = this;

        // Create references to the stepRecords in a new Dictionary
        // Because StepRecord is a class (not a struct), the Dictionary and the
        //  Array both point to the same StepRecord in memory.
        STEP_REC_DICT = new Dictionary<Achievement.eStepType, StepRecord>();
        foreach (StepRecord sRec in stepRecords)
        {
            STEP_REC_DICT.Add(sRec.type, sRec);
        }
    }


    void TriggerPopUp(string achievementName, string achievementDescription = "")
    {
        popUp.PopUp(achievementName, achievementDescription);
    }


    void UnlockPartsAfterLoadingGame(){

        foreach (Achievement ach in achievements) {
            if (ach.complete) {
                ShipCustomizationPanel.UnlockPart(ach.partType, ach.partNum);
            } else {
                ShipCustomizationPanel.LockPart(ach.partType, ach.partNum);
            }
        }

	}


    // ———————————————— Statics ———————————————— //

    /// <summary>
    /// <para>This static private property provides some protection for the Singleton _S.</para>
    /// <para>get {} does return null, but throws an error first.</para>
    /// <para>set {} allows overwrite of _S by a 2nd instance, but throws an error first.</para>
    /// <para>Another advantage of using a property here is that it allows you to place
    /// a breakpoint in the set clause and then look at the call stack if you fear that 
    /// something random is setting your _S value.</para>
    /// </summary>
    static private AchievementManager S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AchievementManager:S getter - Attempt to get "
                               + "value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AchievementManager:S setter - Attempt to set S "
                               + "when it has already been set.");
            }
            _S = value;
        }
    }


    /// <summary>
    /// Increments one of the StepRecords that can unlock Achievements.
    /// </summary>
    /// <param name="stepType">The eStepType to increment.</param>
    /// <param name="num">The amount to increment (default = 1).</param>
    static public void AchievementStep(Achievement.eStepType stepType, int num = 1)
    {
        StepRecord sRec = STEP_REC_DICT[stepType];
        if (sRec != null)
        {
            sRec.Progress(num);

            // Iterate through all of the possible Achievements and see if the step
            //  completes the Achievement
            foreach (Achievement ach in S.achievements)
            {
                if (!ach.complete)
                {
                    // Pass the step information to the Achievement, to see if it is completed
                    if (ach.CheckCompletion(stepType, sRec.num))
                    {
                        // The result is true if the Achievement was newly completed
                        AnnounceAchievementCompletion(ach);

                        // Tell Unity Analytics that the Achievement has been completed
                        CustomAnalytics.SendAchievementUnlocked(ach);

                        // Also save the game any time we complete an Achievement
                        SaveGameManager.Save();
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("AchievementManager:AchievementStep( " + stepType + ", " + num + " )"
                             + "was passed a stepType that is not in STEP_REC_DICT.");
        }
    }


    static public void AnnounceAchievementCompletion(Achievement ach)
    {
        ShipCustomizationPanel.UnlockPart(ach.partType, ach.partNum);

        string desc = ach.description.Replace("#", ach.stepCount.ToString("N0"));
        S.TriggerPopUp(ach.name, desc);
    }


    static public StepRecord[] GetStepRecords()
    {
        return S.stepRecords;
    }


    static public Achievement[] GetAchievements()
    {
        return S.achievements;
    }

    /// <summary>
    /// This will clear all Achievement and StepRecord progress and should be
    ///  called when the SaveGame file is deleted.
    /// </summary>
    internal static void ClearStepsAndAchievements()
    {
        // Clear the StepRecord progress
        foreach (StepRecord sRec in S.stepRecords) {
            sRec.num = 0;
        }

        // Clear Achievement completion
        foreach (Achievement ach in S.achievements) {
            ach.complete = false;
		}

        // Make sure that the parts are cleared correctly
        S.UnlockPartsAfterLoadingGame();
    }


    // Note: The Awake() on this script must run before the SaveGameManager.Awake().
    internal static void LoadDataFromSaveFile(SaveFile saveFile)
    {
        // Handle StepRecords
        foreach (StepRecord sRec in saveFile.stepRecords) {
            if (STEP_REC_DICT.ContainsKey(sRec.type)) {
                STEP_REC_DICT[sRec.type].num = sRec.num;
            }
        }

        // Handle Achievements
        foreach (Achievement achSF in saveFile.achievements) {
            // This nested loop is not an efficient way to do this, but the number of 
            //  Achievements is so small that it will work fine. I could have made
            //  a Dictionary<string,Achievement> for Achievements as I did with 
            //  StepRecords, but I wanted to show both ways of doing this.
            foreach (Achievement achAM in S.achievements) {
                if (achSF.name == achAM.name) {
                    // This is the same Achievement
                    achAM.complete = achSF.complete;
                }
            }
        }

        // Unlock the various parts based on loaded Achievement progress
        S.UnlockPartsAfterLoadingGame();
    }
}

/// <summary>
/// <para>The Achievement class allows the definition of Achievements for the 
///  player to complete.</para>
/// </summary>
[System.Serializable]
public class Achievement
{
    /// <summary>
    /// <para>This enum handles all of the steps toward Achievements that are 
    ///  listed in the AsteraX Requirements Doc. This enum can be expanded to 
    ///  allow for additional achievement steps as they are required.</para>
    /// <para>levelUp – The player advances to another level.</para>
    /// <para>bulletFired – The player fires a bullet.</para>
    /// <para>hitAsteroid – The player hits an Asteroid with a bullet.</para>
    /// <para>luckyShot – A bullet hits an Asteroid after wrapping around the screen.</para>
    /// <para>scoreAttained – The player earns a certain score.</para>
    /// </summary>
    public enum eStepType
    {
        levelUp,
        bulletFired,
        hitAsteroid,
        luckyShot,
        scoreAttained,
    }

    public string       name;              // The first line of the Achievement Pop-Up
    [Tooltip("A # in the description will be replaced by the stepCount value.")]
    public string       description;       // The second line of the Achievement Pop-Up
    public eStepType    stepType;          // What type of step triggers this Achievement?
    public int          stepCount;         // And how many of that thing do you need?
    public ShipPart.eShipPartType   partType; // The type of part unlocked by this Achievement
    public int          partNum;           // The type of part unlocked by this Achievement
    [SerializeField]
    private bool        _complete = false; // Has the player completed this Achievement

    // This property with an internal set clause protects _complete.
    public bool complete
    {
        get { return _complete; }
        internal set { _complete = value; }
    }

    /// <summary>
    /// Checks whether this Achievement is newly completed.
    /// <para>Returns false if the Achievement was already completed or if the 
    ///  num passed in does not complete the Achievement</para>
    /// <para>Returns true if this num completes the Achievement</para>
    /// </summary>
    /// <returns>True if this step completes the Achievement; false if it does not.</returns>
    /// <param name="type">The Achievement.eStepType of the num passed in</param>
    /// <param name="num">The current value of that StepRecord</param>
    public bool CheckCompletion(eStepType type, int num)
    {
        if (type != stepType || complete)
        {
            return false;
        }

        // Did this complete the Achievement?
        if (num >= stepCount)
        {
            // Achievement Completed!!!
            complete = true;
            return true;
        }
        return false;
    }

}


[System.Serializable]
public class StepRecord
{
    public Achievement.eStepType type;
    [Tooltip("Is this cumulative over time (like bullets fired) or based on an individual event (like reaching a certain level)?")]
    public bool     cumulative = false;
    [Tooltip("The current count of this step type. Only modify for testing purposes.")]
    [SerializeField]
    private int     _num = 0;

    public void Progress(int n)
    {
        if (cumulative)
        {
            _num += n;
        }
        else
        {
            _num = n;
        }
    }

    public int num
    {
        get { return _num; }
        internal set { _num = value; }
    }
}