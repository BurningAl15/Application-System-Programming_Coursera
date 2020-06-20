using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem parent;
    [SerializeField] private ParticleSystem children;
    private bool isParticleSystemAlive;
    void Update()
    {
        if (!parent.IsAlive())
            if(!children.IsAlive()) 
                ParticleStop();
    }

    public void ParticleStop()
    {
        parent.Stop();
        children.Stop();
        isParticleSystemAlive = false;
    }

    public void ParticlesPlay()
    {
        parent.Play();
        children.Play();
        isParticleSystemAlive = true;
    }

    public bool IsParticleSystemAlive
    {
        get => isParticleSystemAlive;
        // set => isParticleSystemAlive = value;
    }
}
