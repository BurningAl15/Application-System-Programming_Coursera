using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopUp : MonoBehaviour
{
    private static AchievementPopUp _S; // Protected private Singeton (see property S below)

    [Header("Set in Inspector")]
    public Text     popUpText, popUpDescription;
    public Vector3  startPosition, offscreenPosition;
    public float    offscreenYAdj;
    public float    moveSpeed = 10f;
    public float    movePauseTime = 2f;

    [Header("Set Dynamically")]
    public bool     bIsAlreadyPopping = false;
    public List<StringTuple> achievementList = new List<StringTuple>();

    // Use this for initialization
    void Start()
    {
        S = this;

        startPosition = transform.position;
        offscreenPosition = startPosition;
        offscreenPosition.y += offscreenYAdj;
        transform.position = offscreenPosition;
    }


    public void PopUp(StringTuple st)
    {
        PopUp(st.a, st.b);
    }
    public void PopUp(string achievementName, string achievementDescription = "")
    {
        if (!bIsAlreadyPopping)
        {
            bIsAlreadyPopping = true;

            popUpText.text = achievementName;
            popUpDescription.text = achievementDescription;
            transform.position = offscreenPosition;

            StartCoroutine(MoveToPosition());
        }
        else
        {
            StringTuple st = new StringTuple(achievementName, achievementDescription);
            achievementList.Add(st);
            StartCoroutine(WaitYourTurn());
        }
    }


    IEnumerator WaitYourTurn()
    {
        while (bIsAlreadyPopping)
        {
            yield return new WaitForSeconds(0.5f);
        }
        if (achievementList.Count > 0)
        {
            StringTuple st = achievementList[0];
            achievementList.RemoveAt(0);

            PopUp(st);
        }
    }


    IEnumerator MoveToPosition()
    {
        float step = (moveSpeed / (offscreenPosition - startPosition).magnitude * Time.fixedDeltaTime);
        float t = 0;
        float u;
        while (t <= 1.0f)
        {
            t += step;
            u = 1 - (1 - t) * (1 - t); // This does some fancy easing on the Lerp
            transform.position = Vector3.LerpUnclamped(offscreenPosition, startPosition, u);
            yield return new WaitForFixedUpdate();
        }
        transform.position = startPosition;

        yield return new WaitForSeconds(movePauseTime);

        t = 0;
        while (t <= 1.0f)
        {
            t += step;
            u = t * t; // This does some fancy easing on the Lerp
            transform.position = Vector3.Lerp(startPosition, offscreenPosition, u);
            yield return new WaitForFixedUpdate();
        }
        transform.position = offscreenPosition;

        bIsAlreadyPopping = false;
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
    static private AchievementPopUp S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AchievementPopUp:S getter - Attempt to get value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AchievementPopUp:S setter - Attempt to set S when it has already been set.");
            }
            _S = value;
        }
    }


    static public void ShowPopUp(string achievementName, string achievementDescription = "")
    {
        S.PopUp(achievementName, achievementDescription);
    }
}


[System.Serializable]
public struct StringTuple
{
    public string a, b;

    public StringTuple(string sA = "", string sB = "")
    {
        a = sA;
        b = sB;
    }
}
