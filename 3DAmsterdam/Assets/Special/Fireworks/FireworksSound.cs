using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworksSound : MonoBehaviour
{
    //public ParticleSystem particleSystem;
    public AudioSource audioSource;
    private bool IsBorn = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsBorn)
        {
            return;
        }
        if (GetComponent<ParticleSystem>().particleCount>0)
        {
            IsBorn = true;
            audioSource.Play();
        }
    }
}
