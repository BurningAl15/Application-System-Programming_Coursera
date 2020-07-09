#define DEBUG_PlayerShip_RespawnNotifications

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShip : MonoBehaviour
{
    // This is a somewhat protected private singleton for PlayerShip
    static private PlayerShip   _S;
    static public PlayerShip    S
    {
        get
        {
            return _S;
        }
        private set
        {
            if (_S != null)
            {
                Debug.LogWarning("Second attempt to set PlayerShip singleton _S.");
            }
            _S = value;
        }
    }

    static public int   JUMPS = 3;
    static public float	LAST_COLLISION = -1000;
    static public float COLLISION_DELAY = 1;


    [Header("Set in Inspector")]
    public float        shipSpeed = 10f;
    public GameObject   bulletPrefab;
    [Tooltip("The amount of time that the ship disappears during jump/teleport.")]
    public float        respawnDelay = 2;
    [Tooltip("The number of Jumps that the ship start the game with.")]
    public int          startingJumps = 3;

    Rigidbody           rigid;

    [SerializeField] private ParticleSystem _particleSystem;
    // [SerializeField] private ParticleSystem _teleportParticleSystem;
    [FormerlySerializedAs("_teleportParticleSystem")] [SerializeField] private ParticleManager _teleportDissapearParticleSystem;
    [SerializeField] private ParticleManager _teleportAppearParticleSystem;

    void Awake()
    {
        S = this;

        JUMPS = startingJumps;
        
        // NOTE: We don't need to check whether or not rigid is null because of [RequireComponent()] above
        rigid = GetComponent<Rigidbody>();
        SetEmission(0);
        _teleportDissapearParticleSystem.ParticleStop();
        _teleportAppearParticleSystem.ParticleStop();
    }

    private int currentParticleEmission = 0;
    private bool isMoving = false;
    
    void SetEmission(int emissionPower)
    {
        var emission = _particleSystem.emission;
        currentParticleEmission = emissionPower;
        emission.rateOverTime = currentParticleEmission;
    }
    
    void Update()
    {
        // Using Horizontal and Vertical axes to set velocity
        float aX = CrossPlatformInputManager.GetAxis("Horizontal");
        float aY = CrossPlatformInputManager.GetAxis("Vertical");

        Vector3 vel = new Vector3(aX, aY);
        if (vel.magnitude > 1)
        {
            // Avoid speed multiplying by 1.414 when moving at a diagonal
            vel.Normalize();
        }

        if (vel.magnitude > 0.05f)
        {
            if (!isMoving)
            {
                SetEmission(50);
                isMoving = true;
            }
        }
        else if (vel.magnitude <= .05f && isMoving)
        {
            SetEmission(0);
            isMoving = false;
        }

        rigid.velocity = vel * shipSpeed;

        // Mouse input for firing
        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }


    void Fire()
    {
        // Get direction to the mouse
        Vector3 mPos = Input.mousePosition;
        mPos.z = -Camera.main.transform.position.z;
        Vector3 mPos3D = Camera.main.ScreenToWorldPoint(mPos);

        // Instantiate the Bullet and set its direction
        GameObject go = Instantiate<GameObject>(bulletPrefab);
        go.transform.position = transform.position;
        go.transform.LookAt(mPos3D);
    }

    void OnCollisionEnter(Collision collision)
    {
        Asteroid a = collision.gameObject.GetComponent<Asteroid>();
        if (a == null) {
            return;
        }

        _teleportDissapearParticleSystem.transform.position = collision.transform.position;
        _teleportDissapearParticleSystem.ParticlesPlay();

        if (Time.time < LAST_COLLISION + COLLISION_DELAY) {
            return;
        } else {
            LAST_COLLISION = Time.time;
        }

        JUMPS--;
        if (JUMPS < 0) {
            gameObject.SetActive(false);
            AsteraX.GameOver();
            return;
        }

        // Respawn in a new location
        Respawn();
    }

    void Respawn() {
#if DEBUG_PlayerShip_RespawnNotifications
        Debug.Log("PlayerShip:Respawn()");
#endif
        StartCoroutine(AsteraX.FindRespawnPointCoroutine(transform.position, RespawnCallback,_teleportAppearParticleSystem)); 

        // Initially, I had made the gameObject inactive, but this caused the 
        //  coroutine called above to never return from yield!
        //gameObject.SetActive(false);

        // Now, instead, I turn off the OffScreenWrapper and move the GameObject 
        // outside the play area until  RespawnCallback is called.
        OffScreenWrapper wrapper = GetComponent<OffScreenWrapper>();
        if (wrapper != null) {
            wrapper.enabled = false;
        }
        transform.position = new Vector3(10000,10000,0);
    }

    void RespawnCallback(Vector3 newPos) {
#if DEBUG_PlayerShip_RespawnNotifications
        Debug.Log("PlayerShip:RespawnCallback( "+newPos+" )");
#endif
        transform.position = newPos;

        OffScreenWrapper wrapper = GetComponent<OffScreenWrapper>();
        if (wrapper != null) {
            wrapper.enabled = true;
        }
    }


    static public float MAX_SPEED
    {
        get
        {
            return S.shipSpeed;
        }
    }
    
	static public Vector3 POSITION
    {
        get
        {
            return S.transform.position;
        }
    }

    static public float RESPAWN_DELAY
    {
        get
        {
            return S.respawnDelay;
        }
    }

}
