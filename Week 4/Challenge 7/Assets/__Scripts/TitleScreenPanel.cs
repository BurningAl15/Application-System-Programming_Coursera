using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenPanel : MonoBehaviour {

    // Allows the Button child of this GameObject to call a static method
    public void StartGame()
    {
        AsteraX.StartGame();
    }

    // Allows the Button child of this GameObject to call a static method
    public void DeleteSaveFile()
    {
        SaveGameManager.DeleteSave();
    }

}
