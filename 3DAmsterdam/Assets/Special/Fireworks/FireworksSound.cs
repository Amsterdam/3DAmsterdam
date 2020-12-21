using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworksSound : MonoBehaviour
{
    private ParticleSystem particleSystem;
    private AudioClip audioClip;
    private bool IsBorn = false;
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponentInParent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsBorn)
        {
            return;
        }
        if (particleSystem.particleCount>0)
        {
            IsBorn = true;
            GetComponentInParent<AudioSource>().Play();
        }
    }
}
