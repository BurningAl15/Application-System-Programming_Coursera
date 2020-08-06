//#define DEBUG_AsteraX_LogMethods
//#define DEBUG_AsteraX_RespawnNotifications

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AsteraX : MonoBehaviour
{
    // Private Singleton-style instance. Accessed by static property S later in script
    static private AsteraX _S;
    static public List<LevelInfo>   LEVEL_LIST;
    static List<Asteroid>           ASTEROIDS;
    static List<Bullet>             BULLETS;
    static private bool             _PAUSED = false;
    static private eGameState       _GAME_STATE = eGameState.mainMenu;
    static public bool              GOT_HIGH_SCORE = false;
    
	// If you use a fully-qualified class name like this, you don't need "using UnityEngine.UI;" above.
    static UnityEngine.UI.Text  	SCORE_GT;
    // This is an automatic property
    public static int           	SCORE { get; private set; }
    
    const float MIN_ASTEROID_DIST_FROM_PLAYER_SHIP = 5;
    const float DELAY_BEFORE_RELOADING_SCENE = 4;

	public delegate void CallbackDelegate(); // Set up a generic delegate type.
    static public event CallbackDelegate GAME_STATE_CHANGE_DELEGATE;
    static public event CallbackDelegate PAUSED_CHANGE_DELEGATE;
    
	public delegate void CallbackDelegateV3(Vector3 v); // Set up a Vector3 delegate type.

    // System.Flags changes how eGameStates are viewed in the Inspector and lets multiple 
    //  values be selected simultaneously (similar to how Physics Layers are selected).
    // It's only valid for the game to ever be in one state, but I've added System.Flags
    //  here to demonstrate it and to make the ActiveOnlyDuringSomeGameStates script easier
    //  to view and modify in the Inspector.
    // When you use System.Flags, you still need to set each enum value so that it aligns 
    //  with a power of 2. You can also define enums that combine two or more values,
    //  for example the all value below that combines all other possible values.
    [System.Flags]
    public enum eGameState
    {
        // Decimal      // Binary
        none = 0,       // 00000000
        mainMenu = 1,   // 00000001
        preLevel = 2,   // 00000010
        level = 4,      // 00000100
        postLevel = 8,  // 00001000
        gameOver = 16,  // 00010000
        all = 0xFFFFFFF // 11111111111111111111111111111111
    }

    [Header("Set in Inspector")]
    [Tooltip("This sets the AsteroidsScriptableObject to be used throughout the game.")]
    public AsteroidsScriptableObject asteroidsSO;

    [Header("Set by Remote Settings")]
    public string levelProgression = "1:3/2,2:4/2,3:3/3,4:4/3,5:5/3,6:3/4,7:4/4,8:5/4,9:6/4,10:3/5";


    [Header("These reflect static fields and are otherwise unused")]
    [SerializeField]
    [Tooltip("This private field shows the game state in the Inspector and is set by the "
        + "GAME_STATE_CHANGE_DELEGATE whenever GAME_STATE changes.")]
    protected eGameState  _gameState;
    [SerializeField]
    [Tooltip("This private field shows the game state in the Inspector and is set by the "
    + "PAUSED_CHANGE_DELEGATE whenever PAUSED changes.")]
    protected bool        _paused;

    private void Awake()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:Awake()");
#endif

        S = this;
        
        // Rather than use the anonymous delegate that was here previously, I've instead
        //  created a method that conforms to the GAME_STATE_CHANGE_DELEGATE, thereby 
        //  avoiding the memory issues that can be caused by closures.
        GAME_STATE_CHANGE_DELEGATE += GameStateChanged;
        PAUSED_CHANGE_DELEGATE += PauseChanged;
        
        // Below is another way of doing this that used a C# anonymous delegate.
        // Anonymous delegates are disliked by Unity because of potential memory
        //  leak issues, so I've removed it, though I still wanted to discuss them
        //  here, hence this message and commented code.
        //GAME_STATE_CHANGE_DELEGATE += delegate ()
        //{
        //    // This is an example of a C# anonymous delegate. It's used to set the state of
        //    //  _gameState every time GAME_STATE changes.
        //    // Anonymous delegates like this do create "closures" like "this" below, which 
        //    //  stores the value of this when the anonymous delegate was created. Closures
        //    //  can be slow, but in this case, it is so rarely used that it doesn't matter.
        //    this._gameState = AsteraX.GAME_STATE;
        //};
        //PAUSED_CHANGE_DELEGATE += delegate ()
        //{
        //    this._paused = AsteraX.PAUSED;
        //};
        
		// This strange use of _gameState and _paused as an intermediary in the following 
		//  lines is solely to stop the Warning from popping up in the Console telling you 
        //  that _gameState was assigned but not used.
        _gameState = eGameState.mainMenu;
        GAME_STATE = _gameState;
        _paused = true;
        PauseGame(_paused);
    }

    void GameStateChanged() {
        this._gameState = AsteraX.GAME_STATE;
    }

    void PauseChanged() {
        this._paused = AsteraX.PAUSED;
    }

    private void OnDestroy()
    {
        GAME_STATE_CHANGE_DELEGATE -= GameStateChanged;
        PAUSED_CHANGE_DELEGATE -= PauseChanged;
        AsteraX.GAME_STATE = AsteraX.eGameState.none;
    }

    void Start()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:Start()");
#endif
        // Register RemoteSettingsUpdated() to be called whenever RemoteSettings is updated.
        RemoteSettings.Updated += RemoteSettingsUpdated;
		
		ParseLevelProgression();

        ASTEROIDS = new List<Asteroid>();
        AddScore(0);

        SaveGameManager.Load();
    }


    void StartLevel(int levelNum)
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:StartLevel("+levelNum+")");
#endif
        if (LEVEL_LIST.Count == 0)
        {
            Debug.LogError("AsteraX:StartLevel(" + levelNum + ") - LEVEL_LIST is empty!");
            return;
        }
        if (levelNum >= LEVEL_LIST.Count)
        {
            levelNum = 1; // Just loop the levels for now. In a real game, this would be different.
        }

        GAME_STATE = eGameState.preLevel;
        GAME_LEVEL = levelNum;
        LevelInfo info = LEVEL_LIST[levelNum - 1];

        // Destroy any remaining Asteroids, Bullets, etc. (including particle effects)
        ClearAsteroids();
        ClearBullets();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("DestroyWithLevelChange"))
        {
            Destroy(go);
        }

        // Set up the asteroidsSO
        asteroidsSO.numSmallerAsteroidsToSpawn = info.numSubAsteroids;
        
        // Spawn the parent Asteroids, child Asteroids are taken care of by them
        for (int i = 0; i < info.numInitialAsteroids; i++)
        {
            SpawnParentAsteroid(i);
        }

        CustomAnalytics.SendLevelStart(GAME_LEVEL);
        AchievementManager.AchievementStep(Achievement.eStepType.levelUp, levelNum);

    }

    void EndLevel()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:EndLevel()");
#endif
        if (GAME_STATE != eGameState.none)
        {
            PauseGame(true);
            GAME_LEVEL++;
            GAME_STATE = eGameState.postLevel;
            LevelAdvancePanel.AdvanceLevel(LevelAdvanceDisplayCallback, LevelAdvanceIdleCallback);
        }
    }

    void LevelAdvanceDisplayCallback()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:LevelAdvanceDisplayCallback()");
#endif
        StartLevel(GAME_LEVEL);
    }

    void LevelAdvanceIdleCallback()
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:LevelAdvanceIdleCallback()");
#endif
        GAME_STATE = eGameState.level;

        PauseGame(false); // unpause the game
    }

    void SpawnParentAsteroid(int i)
    {
#if DEBUG_AsteraX_LogMethods
        Debug.Log("AsteraX:SpawnParentAsteroid("+i+")");
#endif

        Asteroid ast = Asteroid.SpawnAsteroid();
        ast.gameObject.name = "Asteroid_" + i.ToString("00");
        // Find a good location for the Asteroid to spawn
        Vector3 pos;
        do
        {
            pos = ScreenBounds.RANDOM_ON_SCREEN_LOC;
        } while ((pos - PlayerShip.POSITION).magnitude < MIN_ASTEROID_DIST_FROM_PLAYER_SHIP);

        ast.transform.position = pos;
        ast.size = asteroidsSO.initialSize;
    }

    void ClearAsteroids()
    {
        // Some Asteroids in ASTEROIDS are children of others, so we should de-parent them
        //  before destroying their parents. Because Asteroids were added with parents
        //  first, working backwards through the ASTEROIDS List seems like a good way to
        //  do this. By working backwards through ASTEROIDS, we also would avoid some issues
        //  as Asteroids removed themselves from the ASTEROIDS list, but due to GAME_STATE
        //  protections in RemoveAsteroid(), we don't have that problem.
        Asteroid ast;
        for (int i = ASTEROIDS.Count - 1; i >= 0; i--)
        {
            ast = ASTEROIDS[i];
            ast.transform.SetParent(null); // De-parent the Asteroid
            Destroy(ast.gameObject);
        }

        // Because of GAME_STATE protections in RemoveAsteroid(), destroying the Asteroids
        //  above does NOT remove them from ASTEROIDS, so they need to be cleared now.
        ASTEROIDS.Clear();
    }

    void ClearBullets()
    {
        if (BULLETS == null)
        {
            return;
        }
        // Bullets are much simpler to clear than Asteroids because there are no parent-child dependencies
        // Because Bullet.OnDestroy() will in turn call AsteraX.RemoveBullet(), we need to work backwards through BULLETS
        for (int i = BULLETS.Count - 1; i >= 0; i--)
        {
            Destroy(BULLETS[i].gameObject);
        }
    }


    void ParseLevelProgression()
    {
        // This takes the information from levelProgression and puts it into LEVEL_LIST;
        LEVEL_LIST = new List<LevelInfo>();

        // NOTE: There are more performant and memory-friendly ways to do this parsing, but
        //  since this only happens once per launch (or twice if updated by Remote Settings)
        //  it's not worth it to worry too much about performance or memory.
        // NOTE: There is little protection here for bad data in the levelProgression field. In 
        //  an actual production environment, you would absolutely need to add more protection.
        string[] levelStrings = levelProgression.Split(',');
        for (int i = 0; i < levelStrings.Length; i++)
        {
            string[] levelBits = levelStrings[i].Split(':');
            string levelName = "Level " + levelBits[0];
            string[] asteroidStrings = levelBits[1].Split('/');
            int numInitialAsteroids, numSubAsteroids;
            if (!int.TryParse(asteroidStrings[0], out numInitialAsteroids)
                || !int.TryParse(asteroidStrings[1], out numSubAsteroids))
            {
                Debug.LogError("AsteraX:ParseLevelProgression() - Attempt to parse bad asteroid numbers" +
                               "for " + levelName + ": " + levelStrings[i]);
                return; // This is throwing an error anyway, so we'll exit the method.
            }
            // We should have good data now
            LevelInfo levelInfo = new LevelInfo(i + 1, levelName, numInitialAsteroids, numSubAsteroids);
            LEVEL_LIST.Add(levelInfo);
        }
        Debug.Log("AsteraX:ParseLevelProgression() - Parsed levelProgression:\n" + levelProgression);
    }

    void RemoteSettingsUpdated()
    {
        string newLevelProgression = RemoteSettings.GetString("levelProgression", "");
        if (newLevelProgression != "")
        {
            levelProgression = newLevelProgression;
            Debug.Log("AsteraX:RemoteSettingsUpdated() - Calling ParseLevelProgression() "
                  + "with levelProgression:\n" + levelProgression);
            ParseLevelProgression();
        }
        else
        {
            Debug.Log("AsteraX:RemoteSettingsUpdated() - Did not receive proper " +
                      "levelProgression from RemoteSettings.");
        }
    }

    public void PauseGameToggle()
    {
        PauseGame(!PAUSED);
    }

    public void PauseGame(bool toPaused)
    {
        PAUSED = toPaused;
        if (PAUSED)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }


    private void Update()
    {
        if (GAME_STATE == eGameState.level && ASTEROIDS.Count == 0)
        {
            // The player has destroyed all the Asteroids and completed this level
            if (_S != null)
            {
                _S.EndLevel();
            }
        }
    }
    
    
	public void EndGame()
    {
        GAME_STATE = eGameState.gameOver;
        Invoke("ReloadScene", DELAY_BEFORE_RELOADING_SCENE);
    }
    

    void ReloadScene()
    {
        // Reload the scene to restart the game
        // Note: This exposes a long-time Unity bug where reloading the scene 
        //  during gameplay within the Editor causes the lighting to all go 
        //  dark and the engine to think that it needs to rebuild the lighting.
        //  This bug does not cause any issues outside of the Editor.
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }





    static public void AddAsteroid(Asteroid asteroid)
    {
        if (ASTEROIDS.IndexOf(asteroid) == -1)
        {
            ASTEROIDS.Add(asteroid);
        }
    }
    static public void RemoveAsteroid(Asteroid asteroid)
    {
        if (GAME_STATE != eGameState.level)
        {
            // If this is not in the middle of a level, don't do anything. RemoveAsteroid is called
            //  by Asteroid:OnDestroy(), so this prevents removal from happening if the game is in
            //  any state other than level, which avoids modifying the ASTEROIDS List in the for 
            //  loop of ClearAsteroids().
            return;
        }
        if (ASTEROIDS.IndexOf(asteroid) != -1)
        {
            ASTEROIDS.Remove(asteroid);
        }
    }

    static public void AddBullet(Bullet bullet)
    {
        if (BULLETS == null)
        {
            BULLETS = new List<Bullet>();
        }
        if (BULLETS.IndexOf(bullet) == -1)
        {
            BULLETS.Add(bullet);
            
            // Notify the AchievementManager that this has happened
            AchievementManager.AchievementStep(Achievement.eStepType.bulletFired, 1);
        }
    }

    static public void RemoveBullet(Bullet bullet)
    {
        if (BULLETS == null)
        {
            return;
        }
        BULLETS.Remove(bullet);
    }



    // ---------------- Static Section ---------------- //

    /// <summary>
    /// <para>This static private property provides some protection for the Singleton _S.</para>
    /// <para>get {} does return null, but throws an error first.</para>
    /// <para>set {} allows overwrite of _S by a 2nd instance, but throws an error first.</para>
    /// <para>Another advantage of using a property here is that it allows you to place
    /// a breakpoint in the set clause and then look at the call stack if you fear that 
    /// something random is setting your _S value.</para>
    /// </summary>
    static private AsteraX S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AsteraX:S getter - Attempt to get value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AsteraX:S setter - Attempt to set S when it has already been set.");
            }
            _S = value;
        }
    }


    static public AsteroidsScriptableObject AsteroidsSO
    {
        get
        {
            if (S != null)
            {
                return S.asteroidsSO;
            }
            return null;
        }
    }

    static public bool PAUSED
    {
        get
        {
            return _PAUSED;
        }
        private set
        {
            if (value != _PAUSED)
            {
                _PAUSED = value;
                // Need to update all of the handlers
                // Any time you use a delegate, you run the risk of it not having any handlers
                //  assigned to it. In that case, it is null and will throw a null reference
                //  exception if you try to call it. So *any* time you call a delegate, you 
                //  should check beforehand to make sure it's not null.
                if (PAUSED_CHANGE_DELEGATE != null)
                {
                    PAUSED_CHANGE_DELEGATE();
                }
            }

        }
    }

    static public eGameState GAME_STATE
    {
        get
        {
            return _GAME_STATE;
        }
        set
        {
            if (value != _GAME_STATE)
            {
                _GAME_STATE = value;
                // Need to update all of the handlers
                // Any time you use a delegate, you run the risk of it not having any handlers
                //  assigned to it. In that case, it is null and will throw a null reference
                //  exception if you try to call it. So *any* time you call a delegate, you 
                //  should check beforehand to make sure it's not null.
                if (GAME_STATE_CHANGE_DELEGATE != null)
                {
                    GAME_STATE_CHANGE_DELEGATE();
                }
            }
        }
    }

    // This is called an automatic property. It allows protection of a static field and automatically
    //  generates a static field that it reads and writes.
    static public int GAME_LEVEL
    {
        get; private set;
    }

    static public void StartGame()
    {
        GOT_HIGH_SCORE = false;
        GAME_LEVEL = 0;
        _S.EndLevel();
    }

    static public void GameOver()
    {
        SaveGameManager.CheckHighScore(SCORE);
        SaveGameManager.Save();
        CustomAnalytics.SendFinalShipPartChoice();
        CustomAnalytics.SendGameOver();
        _S.EndGame();
    }


	[System.Serializable]
    public struct LevelInfo
    {
        public int levelNum;
        public string levelName;
        public int numInitialAsteroids;
        public int numSubAsteroids;

        public LevelInfo(int lNum, string name, int initial, int sub)
        {
            levelNum = lNum;
            levelName = name;
            numInitialAsteroids = initial;
            numSubAsteroids = sub;
        }
    }


    static public LevelInfo GetLevelInfo(int lNum = -1)
    {
        if (lNum == -1)
        {
            lNum = GAME_LEVEL;
        }
        // lNum is 1-based where LEVEL_LIST is 0-based, so LEVEL_LIST[0] is lNum 1
        if (lNum < 1 || lNum > LEVEL_LIST.Count)
        {
            Debug.LogError("AsteraX:GetLevelInfo() - Requested level number of " + lNum + " does not exist.");
            return new LevelInfo(-1, "NULL", 1, 1);
        }
        return (LEVEL_LIST[lNum - 1]);
    }

    
	static public void AddScore(int num)
    {
        // Find the ScoreGT Text field only once.
        if (SCORE_GT == null)
        {
            GameObject go = GameObject.Find("ScoreGT");
            if (go != null)
            {
                SCORE_GT = go.GetComponent<UnityEngine.UI.Text>();
            }
            else
            {
                Debug.LogError("AsteraX:AddScore() - Could not find a GameObject named ScoreGT.");
                return;
            }
            SCORE = 0;
        }
        // SCORE holds the definitive score for the game.
        SCORE += num;

        if ( !GOT_HIGH_SCORE && SaveGameManager.CheckHighScore(SCORE) ) {
            // We just got the high score
            GOT_HIGH_SCORE = true;
            // Announce it using the AchievementPopUp
            AchievementPopUp.ShowPopUp("High Score!","You've achieved a new high score.");
        }

        // Show the score on screen. For info on numeric formatting like "N0", see:
        //  https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
        SCORE_GT.text = SCORE.ToString("N0");
        
		AchievementManager.AchievementStep(Achievement.eStepType.scoreAttained, SCORE);
    }


    const int RESPAWN_DIVISIONS = 8;
    const int RESPAWN_AVOID_EDGES = 2; // Note: This number must be greater than 0!
    static Vector3[,] RESPAWN_POINTS;
    /// <summary>
    /// <para>Given the point of the PlayerShip when it hit an Asteroid, this method
    /// chooses a respawn point. The RESPAWN_POINT_GRID_DIVISIONS above determines
    /// how many points the game will check. If that number is 8, then the game 
    /// will check 49 (7x7) points within the play area (dividing each dimension
    /// into 8ths and avoiding the edges of the play area).</para>
    /// <para>This method will not find and avoid the location closest to the 
    /// PlayerShip's previous location and then will iterate through all points
    /// and all Asteroids.</para>
    /// <para>This process is not very performant (though given the
    /// small numbers of objects, it's still really fast), so we'll have it use 
    /// a coroutine to demonstrate their use.</para>
    /// </summary>
    /// <returns>The respawn point for the PlayerShip.</returns>
    /// <param name="prevPos">Previous position of the PlayerShip.</param>
    /// <param name="callback">Method to be called when this method is finished.</param>
    static public IEnumerator FindRespawnPointCoroutine(Vector3 prevPos, CallbackDelegateV3 callback)
    {
# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine( "+prevPos+", [CallbackDelegateV3] )");
#endif
        // Spawn particle effect for disappearing
        Instantiate(PlayerShip.DISAPPEAR_PARTICLES, prevPos, Quaternion.identity);

        // Set up the RESPAWN_POINTS once
        if (RESPAWN_POINTS == null)
        {
            RESPAWN_POINTS = new Vector3[RESPAWN_DIVISIONS + 1, RESPAWN_DIVISIONS + 1];
            Bounds playAreaBounds = ScreenBounds.BOUNDS;
            float dX = playAreaBounds.size.x / RESPAWN_DIVISIONS;
            float dY = playAreaBounds.size.y / RESPAWN_DIVISIONS;
            for (int i = 0; i <= RESPAWN_DIVISIONS; i++)
            {
                for (int j = 0; j <= RESPAWN_DIVISIONS; j++)
                {
                    RESPAWN_POINTS[i, j] = new Vector3(
                        playAreaBounds.min.x + i * dX,
                        playAreaBounds.min.y + j * dY,
                        0);
                }
            }
        }

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() yielding for "+PlayerShip.RESPAWN_DELAY+" seconds.");
#endif

        // Wait a few seconds before choosing the nextPos
        yield return new WaitForSeconds(PlayerShip.RESPAWN_DELAY * 0.8f);

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() back from yield.");
#endif

        float distSqr, closestDistSqr = float.MaxValue;
        int prevI = 0, prevJ = 0;

        // Check points against prevPos (avoiding edges of space)
        for (int i = RESPAWN_AVOID_EDGES; i <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; i++)
        {
            for (int j = RESPAWN_AVOID_EDGES; j <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; j++)
            {
                // sqrMagnitude avoids doing a needless (and costly) square root operation
                distSqr = (RESPAWN_POINTS[i, j] - prevPos).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    prevI = i;
                    prevJ = j;
                }
            }
        }

        float furthestDistSqr = 0;
        Vector3 nextPos = prevPos;
        // Now, iterate through each of the RESPAWN_POINTS to find the one with 
        //  the largest distance to the closest Asteroid (again avoid edges of space)
        for (int i = RESPAWN_AVOID_EDGES; i <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; i++)
        {
            for (int j = RESPAWN_AVOID_EDGES; j <= RESPAWN_DIVISIONS - RESPAWN_AVOID_EDGES; j++)
            {
                if (i == prevI && j == prevJ)
                {
                    continue;
                }
                closestDistSqr = float.MaxValue;
                // Find distance to the closest Asteroid
                for (int k = 0; k < ASTEROIDS.Count; k++)
                {
                    distSqr = (ASTEROIDS[k].transform.position - RESPAWN_POINTS[i, j]).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestDistSqr = distSqr;
                    }
                }

                // If this is further than before, this is the best spawn loc
                if (closestDistSqr > furthestDistSqr)
                {
                    furthestDistSqr = closestDistSqr;
                    nextPos = RESPAWN_POINTS[i, j];
                }
            }
        }

        // Spawn particle effect for appearing
        Instantiate(PlayerShip.APPEAR_PARTICLES, nextPos, Quaternion.identity);

        // Give the particle effect just a bit of time before the ship respawns
        yield return new WaitForSeconds(PlayerShip.RESPAWN_DELAY * 0.2f);

# if DEBUG_AsteraX_RespawnNotifications
        Debug.Log("AsteraX:FindRespawnPointCoroutine() calling back!");
#endif
        callback(nextPos);
    }

}
