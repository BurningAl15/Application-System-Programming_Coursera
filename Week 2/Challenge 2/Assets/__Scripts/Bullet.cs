using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OffScreenWrapper))]
public class Bullet : MonoBehaviour {
    static private Transform _BULLET_ANCHOR;
    static Transform BULLET_ANCHOR {
        get {
            if (_BULLET_ANCHOR == null) {
                GameObject go = new GameObject("BulletAnchor");
                _BULLET_ANCHOR = go.transform;
            }
            return _BULLET_ANCHOR;
        }
    }

    [Header("Set in Inspector")]
    public float        bulletSpeed = 20;
    public float        lifeTime = 2;
    public GameObject   particleEffectPrefab;

    [Header("Dynamic")]
    public bool         bDidWrap = false;

    void Start()
    {
        transform.SetParent(BULLET_ANCHOR, true);

        // Set Bullet to self-destruct in lifeTime seconds
        Invoke("DestroyMe", lifeTime);

        // Set the velocity of the Bullet
        GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;

        // Attach the particle effect
        GameObject pe = Instantiate<GameObject>(particleEffectPrefab);
        pe.transform.SetParent(transform);
        pe.transform.localPosition = Vector3.zero;
    }

    void DestroyMe()
    {
        Destroy(gameObject);
    }
    
}
